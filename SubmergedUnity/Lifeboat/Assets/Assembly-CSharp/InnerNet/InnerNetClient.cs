using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Hazel;
using Hazel.Udp;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace InnerNet
{
	public abstract class InnerNetClient : MonoBehaviour
	{
		public const int CurrentClient = -3;

		public const int HostInherit = -2;

		internal const byte DataFlag = 1;

		internal const byte RpcFlag = 2;

		internal const byte SpawnFlag = 4;

		internal const byte DespawnFlag = 5;

		internal const byte SceneChangeFlag = 6;

		internal const byte ReadyFlag = 7;

		internal const byte ChangeSettingsFlag = 8;

		internal const byte ConsoleDeclareClientPlatformFlag = 205;

		internal const byte PS4RoomRequest = 206;

		internal const byte XboxDeclareXuid = 207;

		internal const byte PS4RoomRequest_DoesRoomExist = 1;

		internal const byte PS4RoomRequest_DoesRoomExistReply = 2;

		public float MinSendInterval = 0.1f;

		private uint NetIdCnt = 1U;

		private float timer;

		public AssetReference[] SpawnableObjects;

		public InnerNetObject[] NonAddressableSpawnableObjects;

		private bool InOnlineScene;

		private HashSet<uint> DestroyedObjects = new HashSet<uint>();

		public List<InnerNetObject> allObjects = new List<InnerNetObject>();

		private Dictionary<uint, InnerNetObject> allObjectsFast = new Dictionary<uint, InnerNetObject>();

		private MessageWriter[] Streams;

		private int msgNum;

		private static readonly DisconnectReasons[] disconnectReasons = new DisconnectReasons[]
		{
			DisconnectReasons.Error,
			DisconnectReasons.GameFull,
			DisconnectReasons.GameStarted,
			DisconnectReasons.GameNotFound,
			DisconnectReasons.IncorrectVersion,
			DisconnectReasons.Banned,
			DisconnectReasons.Kicked,
			DisconnectReasons.Hacking,
			DisconnectReasons.ServerFull,
			DisconnectReasons.Custom
		};

		public const int MaxRecentClients = 20;

		private const int DefaultSecondsSuspendedBeforeDisconnect = 30;

		public static int SecondsSuspendedBeforeDisconnect = 30;

		public const int NoClientId = -1;

		private string networkAddress = "127.0.0.1";

		private int networkPort;

		private UnityUdpClientConnection connection;

		public MatchMakerModes mode;

		public GameModes GameMode;

		public int GameId = 32;

		public int HostId;

		public int ClientId = -1;

		protected List<ClientData> allClients = new List<ClientData>();

		protected CircleBuffer<ClientData> recentClients = new CircleBuffer<ClientData>(20);

		public DisconnectReasons LastDisconnectReason;

		public string LastCustomDisconnect;

		private readonly List<Action> PreSpawnDispatcher = new List<Action>();

		private readonly List<Action> Dispatcher = new List<Action>();

		public InnerNetClient.GameStates GameState;

		private List<Action> TempQueue = new List<Action>();

		private volatile bool appPaused;

		private void FixedUpdate()
		{
			if (this.connection != null)
			{
				this.connection.FixedUpdate();
			}
			if (this.mode == MatchMakerModes.None || this.Streams == null)
			{
				this.timer = 0f;
				return;
			}
			this.timer += Time.fixedDeltaTime;
			if (this.timer < this.MinSendInterval)
			{
				return;
			}
			this.timer = 0f;
			List<InnerNetObject> obj = this.allObjects;
			lock (obj)
			{
				for (int i = 0; i < this.allObjects.Count; i++)
				{
					InnerNetObject innerNetObject = this.allObjects[i];
					if (innerNetObject && innerNetObject.IsDirty && (innerNetObject.AmOwner || (innerNetObject.OwnerId == -2 && this.AmHost)))
					{
						MessageWriter messageWriter = this.Streams[(int) innerNetObject.sendMode];
						messageWriter.StartMessage(1);
						messageWriter.WritePacked(innerNetObject.NetId);
						if (innerNetObject.Serialize(messageWriter, false))
						{
							messageWriter.EndMessage();
						}
						else
						{
							messageWriter.CancelMessage();
						}
					}
				}
			}
			for (int j = 0; j < this.Streams.Length; j++)
			{
				MessageWriter messageWriter2 = this.Streams[j];
				if (messageWriter2.HasBytes(7))
				{
					messageWriter2.EndMessage();
					this.SendOrDisconnect(messageWriter2);
					messageWriter2.Clear((SendOption) j);
					messageWriter2.StartMessage(5);
					messageWriter2.Write(this.GameId);
				}
			}
		}

		public T FindObjectByNetId<T>(uint netId) where T : InnerNetObject
		{
			InnerNetObject innerNetObject;
			if (this.allObjectsFast.TryGetValue(netId, out innerNetObject))
			{
				return (T)((object)innerNetObject);
			}
			return default(T);
		}

		public MessageWriter StartRpcImmediately(uint targetNetId, byte callId, SendOption option, int targetClientId = -1)
		{
			MessageWriter messageWriter = MessageWriter.Get(option);
			if (targetClientId < 0)
			{
				messageWriter.StartMessage(5);
				messageWriter.Write(this.GameId);
			}
			else
			{
				messageWriter.StartMessage(6);
				messageWriter.Write(this.GameId);
				messageWriter.WritePacked(targetClientId);
			}
			messageWriter.StartMessage(2);
			messageWriter.WritePacked(targetNetId);
			messageWriter.Write(callId);
			return messageWriter;
		}

		public void FinishRpcImmediately(MessageWriter msg)
		{
			msg.EndMessage();
			msg.EndMessage();
			this.SendOrDisconnect(msg);
			msg.Recycle();
		}

		public void SendSelfClientInfoToAll()
		{
			MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
			messageWriter.StartMessage(5);
			messageWriter.Write(this.GameId);
			messageWriter.StartMessage(205);
			messageWriter.WritePacked(this.ClientId);
			messageWriter.WritePacked((int) Application.platform);
			messageWriter.EndMessage();
			messageWriter.EndMessage();
			this.SendOrDisconnect(messageWriter);
			messageWriter.Recycle();
		}

		public void SendXuidToAll()
		{
			Debug.Log("InnerNetClient.During::SendXuidToAll");
		}

		public void PS4_AskIfRoomExists()
		{
			Debug.LogError("PS4_AskIfRoomExists");
			MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
			messageWriter.StartMessage(5);
			messageWriter.Write(this.GameId);
			messageWriter.StartMessage(206);
			messageWriter.WritePacked(this.ClientId);
			messageWriter.Write(1);
			messageWriter.EndMessage();
			messageWriter.EndMessage();
			this.SendOrDisconnect(messageWriter);
			messageWriter.Recycle();
		}

		public void PS4_ReplyWithMyRoom()
		{
		}

		public void SendRpc(uint targetNetId, byte callId, SendOption option = SendOption.Reliable)
		{
			this.StartRpc(targetNetId, callId, option).EndMessage();
		}

		public MessageWriter StartRpc(uint targetNetId, byte callId, SendOption option = SendOption.Reliable)
		{
			MessageWriter messageWriter = this.Streams[(int) option];
			messageWriter.StartMessage(2);
			messageWriter.WritePacked(targetNetId);
			messageWriter.Write(callId);
			return messageWriter;
		}

		private void SendSceneChange(string sceneName)
		{
			this.InOnlineScene = string.Equals(sceneName, "OnlineGame");
			if (!this.AmConnected)
			{
				return;
			}
			Debug.Log("Changed To " + sceneName);
			base.StartCoroutine(this.CoSendSceneChange(sceneName));
		}

		private IEnumerator CoSendSceneChange(string sceneName)
		{
			List<InnerNetObject> obj = this.allObjects;
			lock (obj)
			{
				for (int i = this.allObjects.Count - 1; i > -1; i--)
				{
					if (!this.allObjects[i])
					{
						this.allObjects.RemoveAt(i);
					}
				}
				goto IL_9C;
			}
			IL_85:
			yield return null;
			IL_9C:
			if (this.AmConnected && this.ClientId < 0)
			{
				goto IL_85;
			}
			if (!this.AmConnected)
			{
				yield break;
			}
			ClientData clientData = this.FindClientById(this.ClientId);
			if (clientData != null)
			{
				Debug.Log(string.Format("Self changed scene: {0} {1}", this.ClientId, sceneName));
				yield return this.CoOnPlayerChangedScene(clientData, sceneName);
			}
			else
			{
				Debug.Log(string.Format("Couldn't find self in clients: {0}: ", this.ClientId) + sceneName);
			}
			if (!this.AmHost && this.connection.State == ConnectionState.Connected)
			{
				MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
				messageWriter.StartMessage(5);
				messageWriter.Write(this.GameId);
				messageWriter.StartMessage(6);
				messageWriter.WritePacked(this.ClientId);
				messageWriter.Write(sceneName);
				messageWriter.EndMessage();
				messageWriter.EndMessage();
				this.SendOrDisconnect(messageWriter);
				messageWriter.Recycle();
			}
			yield break;
		}

		public void Spawn(InnerNetObject netObjParent, int ownerId = -2, SpawnFlags flags = SpawnFlags.None)
		{
			if (this.AmHost)
			{
				ownerId = ((ownerId == -3) ? this.ClientId : ownerId);
				MessageWriter msg = this.Streams[1];
				this.WriteSpawnMessage(netObjParent, ownerId, flags, msg);
				return;
			}
			if (!this.AmClient)
			{
				return;
			}
			Debug.LogError("Tried to spawn while not host:" + ((netObjParent != null) ? netObjParent.ToString() : null));
		}

		private void WriteSpawnMessage(InnerNetObject netObjParent, int ownerId, SpawnFlags flags, MessageWriter msg)
		{
			msg.StartMessage(4);
			msg.WritePacked(netObjParent.SpawnId);
			msg.WritePacked(ownerId);
			msg.Write((byte)flags);
			InnerNetObject[] componentsInChildren = netObjParent.GetComponentsInChildren<InnerNetObject>();
			msg.WritePacked(componentsInChildren.Length);
			foreach (InnerNetObject innerNetObject in componentsInChildren)
			{
				innerNetObject.OwnerId = ownerId;
				innerNetObject.SpawnFlags = flags;
				if (innerNetObject.NetId == 0U)
				{
					InnerNetObject innerNetObject2 = innerNetObject;
					uint netIdCnt = this.NetIdCnt;
					this.NetIdCnt = netIdCnt + 1U;
					innerNetObject2.NetId = netIdCnt;
					this.allObjects.Add(innerNetObject);
					this.allObjectsFast.Add(innerNetObject.NetId, innerNetObject);
				}
				msg.WritePacked(innerNetObject.NetId);
				msg.StartMessage(1);
				innerNetObject.Serialize(msg, true);
				msg.EndMessage();
			}
			msg.EndMessage();
		}

		public void Despawn(InnerNetObject objToDespawn)
		{
			if (objToDespawn.NetId < 1U)
			{
				Debug.LogError("Tried to net destroy: " + ((objToDespawn != null) ? objToDespawn.ToString() : null));
				return;
			}
			MessageWriter messageWriter = this.Streams[1];
			messageWriter.StartMessage(5);
			messageWriter.WritePacked(objToDespawn.NetId);
			messageWriter.EndMessage();
			this.RemoveNetObject(objToDespawn);
		}

		private bool AddNetObject(InnerNetObject obj)
		{
			uint num = obj.NetId + 1U;
			if (num > this.NetIdCnt)
			{
				this.NetIdCnt = num;
			}
			if (!this.allObjectsFast.ContainsKey(obj.NetId))
			{
				this.allObjects.Add(obj);
				this.allObjectsFast.Add(obj.NetId, obj);
				return true;
			}
			return false;
		}

		public void RemoveNetObject(InnerNetObject obj)
		{
			int num = this.allObjects.IndexOf(obj);
			if (num > -1)
			{
				this.allObjects.RemoveAt(num);
			}
			this.allObjectsFast.Remove(obj.NetId);
			obj.NetId = uint.MaxValue;
		}

		public void RemoveUnownedObjects()
		{
			HashSet<int> hashSet = new HashSet<int>();
			hashSet.Add(-2);
			List<ClientData> obj = this.allClients;
			lock (obj)
			{
				for (int i = 0; i < this.allClients.Count; i++)
				{
					ClientData clientData = this.allClients[i];
					if (clientData.Character)
					{
						hashSet.Add(clientData.Id);
					}
				}
			}
			List<InnerNetObject> obj2 = this.allObjects;
			lock (obj2)
			{
				for (int j = this.allObjects.Count - 1; j > -1; j--)
				{
					InnerNetObject innerNetObject = this.allObjects[j];
					if (!innerNetObject)
					{
						this.allObjects.RemoveAt(j);
					}
					else if (!hashSet.Contains(innerNetObject.OwnerId))
					{
						innerNetObject.OwnerId = this.ClientId;
						 UnityEngine.Object.Destroy(innerNetObject.gameObject);
					}
				}
			}
		}

		private void HandleGameData(MessageReader parentReader)
		{
			try
			{
				while (parentReader.Position < parentReader.Length)
				{
					MessageReader messageReader = parentReader.ReadMessageAsNewBuffer();
					MessageReader reader = messageReader;
					int num = this.msgNum;
					this.msgNum = num + 1;
					base.StartCoroutine(this.HandleGameDataInner(reader, num));
				}
			}
			finally
			{
				parentReader.Recycle();
			}
		}

		private IEnumerator HandleGameDataInner(MessageReader reader, int msgNum)
		{
			int cnt = 0;
			reader.Position = 0;
			byte tag = reader.Tag;
			switch (tag)
			{
			case 1:
				try
				{
					InnerNetObject innerNetObject;
					for (;;)
					{
						uint num = reader.ReadPackedUInt32();
						if (this.allObjectsFast.TryGetValue(num, out innerNetObject))
						{
							break;
						}
						if (this.DestroyedObjects.Contains(num))
						{
							goto IL_14A;
						}
						Debug.LogWarning("Stored data for " + num.ToString());
						int num2 = cnt;
						cnt = num2 + 1;
						if (num2 > 10)
						{
							goto Block_15;
						}
						reader.Position = 0;
						yield return Effects.Wait(0.1f);
					}
					innerNetObject.Deserialize(reader, false);
					goto IL_14A;
					Block_15:
					yield break;
				}
				finally
				{
					reader.Recycle();
				}
				IL_14A:
				goto IL_4CB;
			case 2:
				try
				{
					byte b;
					InnerNetObject innerNetObject2;
					for (;;)
					{
						uint num3;
						try
						{
							num3 = reader.ReadPackedUInt32();
						}
						catch (Exception ex)
						{
							Debug.LogError(string.Format("Error in {0} try {1}, Pos:{2}/{3}: {4}", new object[]
							{
								msgNum,
								cnt,
								reader.Position,
								reader.Length,
								ex
							}));
							Debug.LogException(ex, this);
							throw;
						}
						b = reader.ReadByte();
						if (this.allObjectsFast.TryGetValue(num3, out innerNetObject2))
						{
							break;
						}
						if (num3 == 4294967295U || this.DestroyedObjects.Contains(num3))
						{
							goto IL_2AC;
						}
						Debug.LogWarning(string.Format("Stored Msg {0} RPC {1} for ", msgNum, (RpcCalls)b) + num3.ToString());
						int num2 = cnt;
						cnt = num2 + 1;
						if (num2 > 10)
						{
							goto Block_22;
						}
						reader.Position = 0;
						yield return Effects.Wait(0.1f);
					}
					innerNetObject2.HandleRpc(b, reader);
					goto IL_2AC;
					Block_22:
					yield break;
				}
				finally
				{
					reader.Recycle();
				}
				IL_2AC:
				goto IL_4CB;
			case 3:
				goto IL_43B;
			case 4:
				base.StartCoroutine(this.CoHandleSpawn(reader));
				goto IL_4CB;
			case 5:
				try
				{
					uint num4 = reader.ReadPackedUInt32();
					this.DestroyedObjects.Add(num4);
					InnerNetObject innerNetObject3 = this.FindObjectByNetId<InnerNetObject>(num4);
					if (innerNetObject3 && !innerNetObject3.AmOwner)
					{
						this.RemoveNetObject(innerNetObject3);
						 UnityEngine.Object.Destroy(innerNetObject3.gameObject);
					}
					yield break;
				}
				finally
				{
					reader.Recycle();
				}
				break;
			case 6:
				break;
			case 7:
				try
				{
					ClientData clientData = this.FindClientById(reader.ReadPackedInt32());
					if (clientData != null)
					{
						clientData.IsReady = true;
					}
					yield break;
				}
				finally
				{
					reader.Recycle();
				}
				goto IL_3CC;
			default:
				switch (tag)
				{
				case 205:
					goto IL_3CC;
				case 206:
					goto IL_3DE;
				case 207:
					goto IL_418;
				default:
					goto IL_43B;
				}
				break;
			}
			int num5 = reader.ReadPackedInt32();
			ClientData clientData2 = this.FindClientById(num5);
			string text = reader.ReadString();
			if (clientData2 != null && !string.IsNullOrWhiteSpace(text))
			{
				base.StartCoroutine(this.CoOnPlayerChangedScene(clientData2, text));
				goto IL_4CB;
			}
			Debug.Log(string.Format("Couldn't find client {0} to change scene to {1}", num5, text));
			reader.Recycle();
			goto IL_4CB;
			IL_3CC:
			try
			{
				yield break;
			}
			finally
			{
				reader.Recycle();
			}
			IL_3DE:
			try
			{
				Debug.Log("Client " + reader.ReadPackedUInt32().ToString() + " asked if a PS4 room exists, but we're not on PS4 so we don't care");
				yield break;
			}
			finally
			{
				reader.Recycle();
			}
			IL_418:
			try
			{
				ulong.Parse(reader.ReadString());
				yield break;
			}
			finally
			{
				reader.Recycle();
			}
			IL_43B:
			Debug.Log(string.Format("Bad tag {0} at {1}+{2}={3}:  ", new object[]
			{
				reader.Tag,
				reader.Offset,
				reader.Position,
				reader.Length
			}) + string.Join<byte>(" ", reader.Buffer.Take(128)));
			reader.Recycle();
			IL_4CB:
			yield break;
			yield break;
		}

		private IEnumerator CoHandleSpawn(MessageReader reader)
		{
			uint spawnId = reader.ReadPackedUInt32();
			if ((ulong)spawnId < (ulong)((long)this.SpawnableObjects.Length))
			{
				int ownerId = reader.ReadPackedInt32();
				ClientData ownerClient = this.FindClientById(ownerId);
				int frames = 0;
				while (frames < 1000 && ownerId > 0 && ownerClient == null)
				{
					Debug.LogWarning("Delay spawn for unowned " + spawnId.ToString());
					yield return null;
					ownerClient = this.FindClientById(ownerId);
					int num = frames + 1;
					frames = num;
				}
				if (ownerId > 0 && ownerClient == null)
				{
					yield break;
				}
				InnerNetObject innerNetObject = this.NonAddressableSpawnableObjects.FirstOrDefault((InnerNetObject f) => f.SpawnId == spawnId);
				InnerNetObject innerNetObject2;
				if (innerNetObject)
				{
					innerNetObject2 = UnityEngine.Object.Instantiate<InnerNetObject>(innerNetObject);
				}
				else
				{
					AsyncOperationHandle<GameObject> spawnPrefab = this.SpawnableObjects[(int)spawnId].InstantiateAsync(null, false);
					yield return spawnPrefab;
					innerNetObject2 = spawnPrefab.Result.GetComponent<InnerNetObject>();
					spawnPrefab = default(AsyncOperationHandle<GameObject>);
				}
				innerNetObject2.SpawnFlags = (SpawnFlags)reader.ReadByte();
				if (innerNetObject2.SpawnFlags.HasFlag(SpawnFlags.IsClientCharacter) && ownerClient != null)
				{
					if (!ownerClient.Character)
					{
						ownerClient.InScene = true;
						ownerClient.Character = (innerNetObject2 as PlayerControl);
					}
					else if (innerNetObject2)
					{
						Debug.LogWarning(string.Format("Double spawn character: {0} already has {1}", ownerClient.Id, ownerClient.Character.NetId));
						 UnityEngine.Object.Destroy(innerNetObject2.gameObject);
						yield break;
					}
				}
				int num2 = reader.ReadPackedInt32();
				InnerNetObject[] componentsInChildren = innerNetObject2.GetComponentsInChildren<InnerNetObject>();
				if (num2 != componentsInChildren.Length)
				{
					Debug.LogError(string.Format("Children didn't match for spawnable {0} ({1}): {2} != {3}", new object[]
					{
						spawnId,
						innerNetObject2,
						num2,
						componentsInChildren.Length
					}));
					 UnityEngine.Object.Destroy(innerNetObject2.gameObject);
					yield break;
				}
				for (int i = 0; i < num2; i++)
				{
					InnerNetObject innerNetObject3 = componentsInChildren[i];
					innerNetObject3.NetId = reader.ReadPackedUInt32();
					innerNetObject3.OwnerId = ownerId;
					if (this.DestroyedObjects.Contains(innerNetObject3.NetId))
					{
						innerNetObject2.NetId = uint.MaxValue;
						 UnityEngine.Object.Destroy(innerNetObject2.gameObject);
						break;
					}
					if (!this.AddNetObject(innerNetObject3))
					{
						innerNetObject2.NetId = uint.MaxValue;
						 UnityEngine.Object.Destroy(innerNetObject2.gameObject);
						break;
					}
					MessageReader messageReader = reader.ReadMessage();
					if (messageReader.Length > 0)
					{
						try
						{
							innerNetObject3.Deserialize(messageReader, true);
						}
						catch (Exception arg)
						{
							Debug.LogError(string.Format("Failed to deserialize initial info: {0}: {1}", innerNetObject3, arg));
						}
					}
				}
				ownerClient = null;
			}
			else
			{
				Debug.LogError("Couldn't find spawnable prefab: " + spawnId.ToString());
			}
			yield break;
		}

		public void SetEndpoint(string addr, ushort port)
		{
			this.networkAddress = addr;
			this.networkPort = (int)port;
		}

		public bool AmConnected
		{
			get
			{
				return this.connection != null;
			}
		}

		public int Ping
		{
			get
			{
				if (this.connection == null)
				{
					return 0;
				}
				return (int)this.connection.AveragePingMs;
			}
		}

		public int BytesSent
		{
			get
			{
				if (this.connection == null)
				{
					return 0;
				}
				return (int)this.connection.Statistics.TotalBytesSent;
			}
		}

		public int BytesGot
		{
			get
			{
				if (this.connection == null)
				{
					return 0;
				}
				return (int)this.connection.Statistics.TotalBytesReceived;
			}
		}

		public int Resends
		{
			get
			{
				if (this.connection == null)
				{
					return 0;
				}
				return this.connection.Statistics.MessagesResent;
			}
		}

		public bool AmHost
		{
			get
			{
				return this.HostId == this.ClientId;
			}
		}

		public bool AmClient
		{
			get
			{
				return this.ClientId > 0;
			}
		}

		public bool IsGamePublic { get; private set; }

		public bool IsGameStarted
		{
			get
			{
				return this.GameState == InnerNetClient.GameStates.Started;
			}
		}

		public bool IsGameOver
		{
			get
			{
				return this.GameState == InnerNetClient.GameStates.Ended;
			}
		}

		public virtual void Start()
		{
			SceneManager.activeSceneChanged += delegate(Scene oldScene, Scene scene)
			{
				this.SendSceneChange(scene.name);
			};
			this.ClientId = -1;
			this.GameId = 32;
		}

		private void SendOrDisconnect(MessageWriter msg)
		{
			try
			{
				this.connection.Send(msg);
			}
			catch (Exception ex)
			{
				Debug.Log("Failed to send message: " + ex.Message);
				Debug.LogException(ex, this);
				this.EnqueueDisconnect(DisconnectReasons.Error, "Failed to send message: " + ex.Message);
			}
		}

		public ClientData GetHost()
		{
			List<ClientData> obj = this.allClients;
			lock (obj)
			{
				for (int i = 0; i < this.allClients.Count; i++)
				{
					ClientData clientData = this.allClients[i];
					if (clientData.Id == this.HostId)
					{
						return clientData;
					}
				}
			}
			return null;
		}

		public ClientData GetClientFromCharacter(InnerNetObject character)
		{
			if (!character)
			{
				return null;
			}
			List<ClientData> obj = this.allClients;
			lock (obj)
			{
				for (int i = 0; i < this.allClients.Count; i++)
				{
					ClientData clientData = this.allClients[i];
					if (clientData.Character == character)
					{
						return clientData;
					}
				}
			}
			return null;
		}

		public int GetClientIdFromCharacter(InnerNetObject character)
		{
			ClientData clientFromCharacter = this.GetClientFromCharacter(character);
			if (clientFromCharacter == null)
			{
				return -1;
			}
			return clientFromCharacter.Id;
		}

		public virtual void OnDestroy()
		{
			if (this.AmConnected)
			{
				this.DisconnectInternal(DisconnectReasons.Destroy, null);
			}
		}

		public IEnumerator CoConnect()
		{
			if (this.AmConnected)
			{
				yield break;
			}
			for (;;)
			{
				string ipAddr = this.networkAddress;
				DestroyableSingleton<DisconnectPopup>.Instance.Close();
				this.LastDisconnectReason = DisconnectReasons.ExitGame;
				this.NetIdCnt = 1U;
				this.DestroyedObjects.Clear();
				if (this.GameMode == GameModes.OnlineGame)
				{
					if (DestroyableSingleton<EOSManager>.Instance.ProductUserId == null)
					{
						break;
					}
					yield return DestroyableSingleton<AuthManager>.Instance.CoConnect(ipAddr, (ushort)(this.networkPort + 2));
					yield return DestroyableSingleton<AuthManager>.Instance.CoWaitForNonce(5f);
				}
				IPEndPoint ipendPoint = new IPEndPoint(IPAddress.Parse(ipAddr), this.networkPort);
				this.connection = new UnityUdpClientConnection(ipendPoint, 0);
				this.connection.KeepAliveInterval = 1000;
				this.connection.DisconnectTimeout = 7500;
				this.connection.ResendPingMultiplier = 1.2f;
				this.connection.DataReceived += this.OnMessageReceived;
				this.connection.Disconnected += this.OnDisconnect;
				this.connection.ConnectAsync(this.GetConnectionData());
				while (this.connection != null && this.connection.State == ConnectionState.Connecting)
				{
					yield return null;
				}
				if ((this.connection != null && this.connection.State == ConnectionState.Connected) || this.LastDisconnectReason == DisconnectReasons.IncorrectVersion || this.LastDisconnectReason == DisconnectReasons.InvalidName)
				{
					goto IL_20C;
				}
				Debug.Log(string.Format("Failed to connected to: {0}:{1}", ipAddr, this.networkPort));
				if (!DestroyableSingleton<ServerManager>.Instance.TrackServerFailure(ipAddr))
				{
					goto IL_20C;
				}
				this.DisconnectInternal(DisconnectReasons.NewConnection, null);
			}
			this.EnqueueDisconnect(DisconnectReasons.NotAuthorized, null);
			yield break;
			IL_20C:
			yield break;
		}

		private void Connection_DataReceivedRaw(byte[] data)
		{
			Debug.Log("Client Got: " + string.Join(" ", from b in data
			select b.ToString()));
		}

		private void Connection_DataSentRaw(byte[] data, int length)
		{
			Debug.Log("Client Sent: " + string.Join(" ", (from b in data
			select b.ToString()).ToArray<string>(), 0, length));
		}

		public void Connect(MatchMakerModes mode)
		{
			base.StartCoroutine(this.CoConnect(mode));
		}

		private IEnumerator CoConnect(MatchMakerModes mode)
		{
			if (this.mode != MatchMakerModes.None)
			{
				this.DisconnectInternal(DisconnectReasons.NewConnection, null);
				yield return Effects.Wait(0.1f);
			}
			this.mode = mode;
			yield return this.CoConnect();
			if (this.connection == null || this.connection.State != ConnectionState.Connected)
			{
				this.HandleDisconnect(DisconnectReasons.Error, null);
				yield break;
			}
			MatchMakerModes matchMakerModes = this.mode;
			if (matchMakerModes == MatchMakerModes.Client)
			{
				this.JoinGame();
				yield return this.WaitWithTimeout(() => this.ClientId >= 0, "Failed to join game. Try again later.", 15);
				bool amConnected = this.AmConnected;
				yield break;
			}
			if (matchMakerModes != MatchMakerModes.HostAndClient)
			{
				yield break;
			}
			this.GameId = 0;
			PlayerControl.GameOptions = SaveManager.GameHostOptions;
			this.HostGame(PlayerControl.GameOptions);
			yield return this.WaitWithTimeout(() => this.GameId != 0, "Failed to create a game. Try again later.", 15);
			if (!this.AmConnected)
			{
				yield break;
			}
			this.JoinGame();
			yield return this.WaitWithTimeout(() => this.ClientId >= 0, "Failed to join game after creating it. Try again later.", 15);
			bool amConnected2 = this.AmConnected;
			yield break;
		}

		public IEnumerator WaitForConnectionOrFail()
		{
			while (this.AmConnected)
			{
				switch (this.mode)
				{
				case MatchMakerModes.None:
					goto IL_5F;
				case MatchMakerModes.Client:
					if (this.ClientId >= 0)
					{
						yield break;
					}
					break;
				case MatchMakerModes.HostAndClient:
					if (this.GameId != 0 && this.ClientId >= 0)
					{
						yield break;
					}
					break;
				default:
					goto IL_5F;
				}
				yield return null;
				continue;
				IL_5F:
				yield break;
			}
			yield break;
		}

		private IEnumerator WaitWithTimeout(Func<bool> success, string errorMessage, int durationSeconds = 15)
		{
			bool failed = true;
			for (float timer = 0f; timer < (float)durationSeconds; timer += Time.deltaTime)
			{
				if (success())
				{
					failed = false;
					break;
				}
				if (!this.AmConnected)
				{
					yield break;
				}
				yield return null;
			}
			if (failed && errorMessage != null)
			{
				this.LastCustomDisconnect = errorMessage;
				this.EnqueueDisconnect(DisconnectReasons.Custom, "Couldn't connect");
			}
			yield break;
		}

		public void Update()
		{
			if (Input.GetKeyDown(KeyCode.Return) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
			{
				ResolutionManager.ToggleFullscreen();
			}
			this.TempQueue.Clear();
			List<Action> obj = this.Dispatcher;
			lock (obj)
			{
				this.TempQueue.AddAll(this.Dispatcher);
				this.Dispatcher.Clear();
			}
			for (int i = 0; i < this.TempQueue.Count; i++)
			{
				Action action = this.TempQueue[i];
				try
				{
					action();
				}
				catch (Exception ex)
				{
					Debug.LogError("InnerNetClient::Update Exception 1");
					Debug.LogException(ex, this);
				}
			}
			if (this.InOnlineScene)
			{
				this.TempQueue.Clear();
				obj = this.PreSpawnDispatcher;
				lock (obj)
				{
					this.TempQueue.AddAll(this.PreSpawnDispatcher);
					this.PreSpawnDispatcher.Clear();
				}
				for (int j = 0; j < this.TempQueue.Count; j++)
				{
					Action action2 = this.TempQueue[j];
					try
					{
						action2();
					}
					catch (Exception ex2)
					{
						Debug.LogError("InnerNetClient::Update Exception 2");
						Debug.LogException(ex2, this);
					}
				}
			}
		}

		private void OnDisconnect(object sender, DisconnectedEventArgs e)
		{
			MessageReader message = e.Message;
			if (message != null && message.Position < message.Length)
			{
				if (message.ReadByte() == 1)
				{
					MessageReader messageReader = message.ReadMessage();
					DisconnectReasons disconnectReasons = (DisconnectReasons)messageReader.ReadByte();
					if (disconnectReasons == DisconnectReasons.Custom)
					{
						this.LastCustomDisconnect = messageReader.ReadString();
						if (string.IsNullOrWhiteSpace(this.LastCustomDisconnect))
						{
							this.LastCustomDisconnect = "The server disconnected you without a specific error message.";
						}
					}
					this.EnqueueDisconnect(disconnectReasons, this.LastCustomDisconnect);
					return;
				}
				this.LastCustomDisconnect = "Forcibly disconnected from the server:\r\n\r\n" + (e.Reason ?? "Null");
				this.EnqueueDisconnect(DisconnectReasons.Custom, this.LastCustomDisconnect);
				return;
			}
			else
			{
				if (e.Reason == null)
				{
					this.LastCustomDisconnect = "You disconnected from the server.\r\n\r\nNull";
					this.EnqueueDisconnect(DisconnectReasons.Custom, this.LastCustomDisconnect);
					return;
				}
				if (!e.Reason.Contains("The remote sent a"))
				{
					this.LastCustomDisconnect = "You disconnected from the server.\r\n\r\n" + e.Reason;
					this.EnqueueDisconnect(DisconnectReasons.Custom, this.LastCustomDisconnect);
					return;
				}
				this.EnqueueDisconnect(DisconnectReasons.Error, e.Reason);
				return;
			}
		}

		public void HandleDisconnect(DisconnectReasons reason, string stringReason = null)
		{
			if (AmongUsClient.Instance.GameMode == GameModes.FreePlay)
			{
				return;
			}
			base.StopAllCoroutines();
			this.DisconnectInternal(reason, stringReason);
			this.OnDisconnected();
			this.GameId = -1;
		}

		protected void EnqueueDisconnect(DisconnectReasons reason, string stringReason = null)
		{
			UnityUdpClientConnection unityUdpClientConnection = this.connection;
			List<Action> dispatcher = this.Dispatcher;
			lock (dispatcher)
			{
				this.Dispatcher.Add(delegate
				{
					this.HandleDisconnect(reason, stringReason);
				});
			}
		}

		protected void DisconnectInternal(DisconnectReasons reason, string stringReason = null)
		{
			Debug.Log(string.Format("Client DC because {0}: {1}", reason, stringReason ?? "null"));
			if (reason != DisconnectReasons.NewConnection && reason != DisconnectReasons.FocusLostBackground)
			{
				this.LastDisconnectReason = reason;
				if (reason != DisconnectReasons.ExitGame && DestroyableSingleton<DisconnectPopup>.InstanceExists)
				{
					DestroyableSingleton<DisconnectPopup>.Instance.Show();
				}
			}
			if (this.mode == MatchMakerModes.HostAndClient)
			{
				this.GameId = 0;
			}
			if (this.mode == MatchMakerModes.Client || this.mode == MatchMakerModes.HostAndClient)
			{
				this.ClientId = -1;
			}
			this.mode = MatchMakerModes.None;
			this.GameState = InnerNetClient.GameStates.NotJoined;
			UnityUdpClientConnection unityUdpClientConnection = Interlocked.Exchange<UnityUdpClientConnection>(ref this.connection, null);
			if (unityUdpClientConnection != null)
			{
				try
				{
					unityUdpClientConnection.Dispose();
				}
				catch (Exception ex)
				{
					Debug.LogError("InnerNetClient::DisconnectInternal Exception");
					Debug.LogException(ex, this);
				}
			}
			if (DestroyableSingleton<InnerNetServer>.InstanceExists)
			{
				DestroyableSingleton<InnerNetServer>.Instance.StopServer();
			}
			List<Action> obj = this.Dispatcher;
			lock (obj)
			{
				this.Dispatcher.Clear();
			}
			obj = this.PreSpawnDispatcher;
			lock (obj)
			{
				this.PreSpawnDispatcher.Clear();
			}
			if (reason != DisconnectReasons.Error)
			{
				this.TempQueue.Clear();
			}
			this.allObjects.Clear();
			this.allClients.Clear();
			this.recentClients.Clear();
			this.allObjectsFast.Clear();
		}

		public void HostGame(GameOptionsData settings)
		{
			this.IsGamePublic = false;
			MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
			messageWriter.StartMessage(0);
			messageWriter.WriteBytesAndSize(settings.ToBytes(2));
			messageWriter.Write((byte)SaveManager.ChatModeType);
			messageWriter.EndMessage();
			this.SendOrDisconnect(messageWriter);
			messageWriter.Recycle();
			Debug.Log("Client requesting new game.");
		}

		public void ReportPlayer(int clientId, ReportReasons reason)
		{
			ClientData recentClient = this.GetRecentClient(clientId);
			if (recentClient != null)
			{
				recentClient.HasBeenReported = true;
			}
			MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
			messageWriter.StartMessage(17);
			messageWriter.Write(this.GameId);
			messageWriter.WritePacked(clientId);
			messageWriter.Write((byte)reason);
			messageWriter.EndMessage();
			this.SendOrDisconnect(messageWriter);
			messageWriter.Recycle();
		}

		public void JoinGame()
		{
			this.ClientId = -1;
			if (!this.AmConnected)
			{
				this.LastCustomDisconnect = "Disconnected before joining game.";
				this.HandleDisconnect(DisconnectReasons.Custom, null);
				return;
			}
			if (this.Streams == null)
			{
				this.Streams = new MessageWriter[2];
				for (int i = 0; i < this.Streams.Length; i++)
				{
					this.Streams[i] = MessageWriter.Get((SendOption) i);
				}
			}
			for (int j = 0; j < this.Streams.Length; j++)
			{
				MessageWriter messageWriter = this.Streams[j];
				messageWriter.Clear((SendOption) j);
				messageWriter.StartMessage(5);
				messageWriter.Write(this.GameId);
			}
			Debug.Log(string.Format("Client joining game: {0}/{1}", this.GameId, GameCode.IntToGameName(this.GameId)));
			MessageWriter messageWriter2 = MessageWriter.Get(SendOption.Reliable);
			messageWriter2.StartMessage(1);
			messageWriter2.Write(this.GameId);
			messageWriter2.EndMessage();
			this.SendOrDisconnect(messageWriter2);
			messageWriter2.Recycle();
		}

		public bool CanBan()
		{
			return this.AmHost && !this.IsGameStarted;
		}

		public bool CanKick()
		{
			return this.IsGameStarted || this.AmHost;
		}

		public void KickPlayer(int clientId, bool ban)
		{
			if (!this.AmHost)
			{
				return;
			}
			MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
			messageWriter.StartMessage(11);
			messageWriter.Write(this.GameId);
			messageWriter.WritePacked(clientId);
			messageWriter.Write(ban);
			messageWriter.EndMessage();
			this.SendOrDisconnect(messageWriter);
			messageWriter.Recycle();
		}

		public MessageWriter StartEndGame()
		{
			MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
			messageWriter.StartMessage(8);
			messageWriter.Write(this.GameId);
			return messageWriter;
		}

		public void FinishEndGame(MessageWriter msg)
		{
			msg.EndMessage();
			this.SendOrDisconnect(msg);
			msg.Recycle();
		}

		protected void SendLateRejection(int targetId, DisconnectReasons reason)
		{
			MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
			messageWriter.StartMessage(4);
			messageWriter.Write(this.GameId);
			messageWriter.WritePacked(targetId);
			messageWriter.Write((byte)reason);
			messageWriter.EndMessage();
			this.SendOrDisconnect(messageWriter);
			messageWriter.Recycle();
		}

		protected void SendClientReady()
		{
			if (!this.AmHost)
			{
				MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
				messageWriter.StartMessage(5);
				messageWriter.Write(this.GameId);
				messageWriter.StartMessage(7);
				messageWriter.WritePacked(this.ClientId);
				messageWriter.EndMessage();
				messageWriter.EndMessage();
				this.SendOrDisconnect(messageWriter);
				messageWriter.Recycle();
				return;
			}
			ClientData clientData = this.FindClientById(this.ClientId);
			if (clientData == null)
			{
				this.HandleDisconnect(DisconnectReasons.Error, "Couldn't find self as host");
				return;
			}
			clientData.IsReady = true;
		}

		protected void SendStartGame()
		{
			MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
			messageWriter.StartMessage(2);
			messageWriter.Write(this.GameId);
			messageWriter.EndMessage();
			this.SendOrDisconnect(messageWriter);
			messageWriter.Recycle();
		}

		public void RequestGameList(bool includePrivate, GameOptionsData settings)
		{
			MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
			messageWriter.StartMessage(16);
			messageWriter.WritePacked(2);
			messageWriter.WriteBytesAndSize(settings.ToBytes(2));
			messageWriter.Write((byte)SaveManager.ChatModeType);
			messageWriter.EndMessage();
			this.SendOrDisconnect(messageWriter);
			messageWriter.Recycle();
		}

		public void ChangeGamePublic(bool isPublic)
		{
			if (this.AmHost)
			{
				MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
				messageWriter.StartMessage(10);
				messageWriter.Write(this.GameId);
				messageWriter.Write(1);
				messageWriter.Write(isPublic);
				messageWriter.EndMessage();
				this.SendOrDisconnect(messageWriter);
				messageWriter.Recycle();
				this.IsGamePublic = isPublic;
			}
		}

		private void OnMessageReceived(DataReceivedEventArgs e)
		{
			MessageReader message = e.Message;
			try
			{
				while (message.Position < message.Length)
				{
					this.HandleMessage(message.ReadMessage(), e.SendOption);
				}
			}
			finally
			{
				message.Recycle();
			}
		}

		private void HandleMessage(MessageReader reader, SendOption sendOption)
		{
			List<Action> obj;
			switch (reader.Tag)
			{
			case 0:
				break;
			case 1:
				goto IL_37B;
			case 2:
				this.GameState = InnerNetClient.GameStates.Started;
				obj = this.Dispatcher;
				lock (obj)
				{
					this.Dispatcher.Add(delegate
					{
						base.StartCoroutine(this.CoStartGame());
					});
					return;
				}
				goto IL_752;
			case 3:
			{
				DisconnectReasons reason3 = DisconnectReasons.ServerRequest;
				if (reader.Position < reader.Length)
				{
					reason3 = (DisconnectReasons)reader.ReadByte();
				}
				this.EnqueueDisconnect(reason3, null);
				return;
			}
			case 4:
				goto IL_12B;
			case 5:
			case 6:
			{
				int num = reader.ReadInt32();
				if (this.GameId != num)
				{
					return;
				}
				if (reader.Tag == 6)
				{
					int num2 = reader.ReadPackedInt32();
					if (this.ClientId != num2)
					{
						Debug.LogError(string.Format("Got data meant for {0}", num2));
						return;
					}
				}
				if (this.InOnlineScene)
				{
					MessageReader subReader = MessageReader.Get(reader);
					obj = this.Dispatcher;
					lock (obj)
					{
						this.Dispatcher.Add(delegate
						{
							this.HandleGameData(subReader);
						});
						return;
					}
				}
				if (sendOption != null)
				{
					MessageReader subReader = MessageReader.Get(reader);
					obj = this.PreSpawnDispatcher;
					lock (obj)
					{
						this.PreSpawnDispatcher.Add(delegate
						{
							this.HandleGameData(subReader);
						});
						return;
					}
					goto IL_599;
				}
				return;
			}
			case 7:
				goto IL_2BB;
			case 8:
			{
				int num3 = reader.ReadInt32();
				if (this.GameId == num3 && this.GameState != InnerNetClient.GameStates.Ended)
				{
					this.GameState = InnerNetClient.GameStates.Ended;
					List<ClientData> obj2 = this.allClients;
					lock (obj2)
					{
						this.allClients.Clear();
					}
					GameOverReason reason = (GameOverReason)reader.ReadByte();
					bool showAd = reader.ReadBoolean();
					obj = this.Dispatcher;
					lock (obj)
					{
						this.Dispatcher.Add(delegate
						{
							this.OnGameEnd(reason, showAd);
						});
						return;
					}
					goto IL_263;
				}
				return;
			}
			case 9:
			case 15:
				goto IL_80D;
			case 10:
				goto IL_69D;
			case 11:
				goto IL_752;
			case 12:
				goto IL_263;
			case 13:
			{
				uint address = reader.ReadUInt32();
				ushort port = reader.ReadUInt16();
				obj = this.Dispatcher;
				lock (obj)
				{
					this.Dispatcher.Add(delegate
					{
						AmongUsClient.Instance.SetEndpoint(InnerNetClient.AddressToString(address), port);
						Debug.Log(string.Format("Redirected to: {0}:{1}", this.networkAddress, this.networkPort));
						this.StopAllCoroutines();
						this.Connect(this.mode);
					});
					return;
				}
				goto IL_80D;
			}
			case 14:
				return;
			case 16:
				goto IL_599;
			case 17:
			{
				int clientId = reader.ReadPackedInt32();
				ReportReasons reason = (ReportReasons)reader.ReadInt32();
				ReportOutcome outcome = (ReportOutcome)reader.ReadByte();
				string playerName = reader.ReadString();
				obj = this.Dispatcher;
				lock (obj)
				{
					this.Dispatcher.Add(delegate
					{
						this.OnReportedPlayer(outcome, clientId, playerName, reason);
					});
					return;
				}
				break;
			}
			default:
				goto IL_80D;
			}
			this.GameId = reader.ReadInt32();
			Debug.Log("Client hosting game: " + GameCode.IntToGameName(this.GameId));
			obj = this.Dispatcher;
			lock (obj)
			{
				this.Dispatcher.Add(delegate
				{
					this.OnGameCreated(GameCode.IntToGameName(this.GameId));
				});
				return;
			}
			IL_12B:
			int num4 = reader.ReadInt32();
			if (this.GameId == num4)
			{
				int playerIdThatLeft = reader.ReadInt32();
				int hostId = reader.ReadInt32();
				DisconnectReasons reason2 = (DisconnectReasons)reader.ReadByte();
				if (!this.AmHost)
				{
					this.HostId = hostId;
					if (this.AmHost)
					{
						obj = this.Dispatcher;
						lock (obj)
						{
							this.Dispatcher.Add(delegate
							{
								this.OnBecomeHost();
							});
						}
					}
				}
				this.RemovePlayer(playerIdThatLeft, reason2);
				return;
			}
			return;
			IL_263:
			int num5 = reader.ReadInt32();
			if (this.GameId != num5)
			{
				return;
			}
			this.ClientId = reader.ReadInt32();
			obj = this.Dispatcher;
			lock (obj)
			{
				this.Dispatcher.Add(delegate
				{
					this.OnWaitForHost(GameCode.IntToGameName(this.GameId));
				});
				return;
			}
			IL_2BB:
			int num6 = reader.ReadInt32();
			if (this.GameId != num6 || this.GameState == InnerNetClient.GameStates.Joined)
			{
				return;
			}
			this.GameState = InnerNetClient.GameStates.Joined;
			this.ClientId = reader.ReadInt32();
			ClientData myClient = this.GetOrCreateClient(this.ClientId);
			this.HostId = reader.ReadInt32();
			int num7 = reader.ReadPackedInt32();
			for (int i = 0; i < num7; i++)
			{
				this.GetOrCreateClient(reader.ReadPackedInt32());
			}
			obj = this.Dispatcher;
			lock (obj)
			{
				this.Dispatcher.Add(delegate
				{
					this.OnGameJoined(GameCode.IntToGameName(this.GameId), myClient);
				});
				return;
			}
			IL_37B:
			int num8 = reader.ReadInt32();
			DisconnectReasons disconnectReasons = (DisconnectReasons)num8;
			if (InnerNetClient.disconnectReasons.Contains(disconnectReasons))
			{
				if (disconnectReasons == DisconnectReasons.Custom)
				{
					this.LastCustomDisconnect = reader.ReadString();
				}
				this.GameId = -1;
				this.EnqueueDisconnect(disconnectReasons, null);
				return;
			}
			if (this.GameId == num8)
			{
				int num9 = reader.ReadInt32();
				bool amHost = this.AmHost;
				this.HostId = reader.ReadInt32();
				ClientData client = this.GetOrCreateClient(num9);
				Debug.Log(string.Format("Player {0} joined", num9));
				obj = this.Dispatcher;
				lock (obj)
				{
					this.Dispatcher.Add(delegate
					{
						this.OnPlayerJoined(client);
					});
				}
				if (!this.AmHost || amHost)
				{
					return;
				}
				obj = this.Dispatcher;
				lock (obj)
				{
					this.Dispatcher.Add(delegate
					{
						this.OnBecomeHost();
					});
					return;
				}
			}
			this.EnqueueDisconnect(DisconnectReasons.IncorrectGame, null);
			return;
			IL_599:
			InnerNetClient.TotalGameData totals = new InnerNetClient.TotalGameData();
			List<GameListing> output = new List<GameListing>();
			while (reader.Position < reader.Length)
			{
				MessageReader messageReader = reader.ReadMessage();
				byte tag = messageReader.Tag;
				if (tag != 0)
				{
					if (tag == 1)
					{
						totals.PerMapTotals = new int[3];
						for (int j = 0; j < totals.PerMapTotals.Length; j++)
						{
							totals.PerMapTotals[j] = messageReader.ReadInt32();
						}
					}
				}
				else
				{
					while (messageReader.Position < messageReader.Length)
					{
						GameListing item = GameListing.DeserializeV2(messageReader.ReadMessage());
						output.Add(item);
					}
				}
			}
			obj = this.Dispatcher;
			lock (obj)
			{
				this.Dispatcher.Add(delegate
				{
					this.OnGetGameList(totals, output);
				});
				return;
			}
			IL_69D:
			int num10 = reader.ReadInt32();
			if (this.GameId != num10)
			{
				return;
			}
			if (reader.ReadByte() == 1)
			{
				this.IsGamePublic = reader.ReadBoolean();
				string str = "Alter Public = ";
				bool flag = this.IsGamePublic;
				Debug.Log(str + flag.ToString());
				return;
			}
			Debug.Log("Alter unknown");
			return;
			IL_752:
			int num11 = reader.ReadInt32();
			if (this.GameId != num11 || reader.ReadPackedInt32() != this.ClientId)
			{
				return;
			}
			bool flag2 = reader.ReadBoolean();
			if (reader.Position < reader.Length)
			{
				this.EnqueueDisconnect((DisconnectReasons)reader.ReadByte(), null);
				return;
			}
			this.EnqueueDisconnect(flag2 ? DisconnectReasons.Banned : DisconnectReasons.Kicked, null);
			return;
			IL_80D:
			Debug.Log(string.Format("Bad tag {0} at {1}+{2}={3}:  ", new object[]
			{
				reader.Tag,
				reader.Offset,
				reader.Position,
				reader.Length
			}) + string.Join<byte>(" ", reader.Buffer.Skip(reader.Offset).Take(reader.Length)));
		}

		public static string AddressToString(uint address)
		{
			return string.Format("{0}.{1}.{2}.{3}", new object[]
			{
				(byte)address,
				(byte)(address >> 8),
				(byte)(address >> 16),
				(byte)(address >> 24)
			});
		}

		private ClientData GetOrCreateClient(int clientId)
		{
			List<ClientData> obj = this.allClients;
			ClientData clientData;
			lock (obj)
			{
				clientData = this.allClients.FirstOrDefault((ClientData c) => c.Id == clientId);
				if (clientData == null)
				{
					clientData = new ClientData(clientId);
					if (clientId == this.ClientId)
					{
						clientData.platformID = Application.platform;
					}
					this.allClients.Add(clientData);
				}
			}
			CircleBuffer<ClientData> obj2 = this.recentClients;
			lock (obj2)
			{
				if (this.recentClients.FirstOrDefault((ClientData c) => c.Id == clientId) == null)
				{
					this.recentClients.Add(clientData);
				}
			}
			return clientData;
		}

		public ClientData GetClient(int clientId)
		{
			List<ClientData> obj = this.allClients;
			ClientData result;
			lock (obj)
			{
				result = this.allClients.FirstOrDefault((ClientData c) => c.Id == clientId);
			}
			return result;
		}

		public void GetRecentClients(List<ClientData> buffer)
		{
			CircleBuffer<ClientData> obj = this.recentClients;
			lock (obj)
			{
				buffer.Clear();
				buffer.AddRange(this.recentClients);
			}
		}

		public ClientData GetRecentClient(int clientId)
		{
			CircleBuffer<ClientData> obj = this.recentClients;
			ClientData result;
			lock (obj)
			{
				result = this.recentClients.FirstOrDefault((ClientData c) => c.Id == clientId);
			}
			return result;
		}

		private void RemovePlayer(int playerIdThatLeft, DisconnectReasons reason)
		{
			ClientData client = null;
			List<ClientData> obj = this.allClients;
			lock (obj)
			{
				for (int i = 0; i < this.allClients.Count; i++)
				{
					ClientData clientData = this.allClients[i];
					if (clientData.Id == playerIdThatLeft)
					{
						client = clientData;
						this.allClients.RemoveAt(i);
						break;
					}
				}
			}
			if (client != null)
			{
				List<Action> dispatcher = this.Dispatcher;
				lock (dispatcher)
				{
					this.Dispatcher.Add(delegate
					{
						this.OnPlayerLeft(client, reason);
					});
				}
			}
		}

		protected virtual void OnApplicationPause(bool pause)
		{
			this.appPaused = pause;
			if (!pause)
			{
				InnerNetClient.SecondsSuspendedBeforeDisconnect = 30;
				Debug.Log("Resumed Game");
				if (this.AmHost)
				{
					this.RemoveUnownedObjects();
					return;
				}
			}
			else if (this.GameState != InnerNetClient.GameStates.Ended && this.AmConnected)
			{
				Debug.Log("Lost focus during game");
				ThreadPool.QueueUserWorkItem(new WaitCallback(this.WaitToDisconnect));
			}
		}

		private void WaitToDisconnect(object state)
		{
			int num = 0;
			while (num < InnerNetClient.SecondsSuspendedBeforeDisconnect && this.appPaused)
			{
				Thread.Sleep(1000);
				num++;
			}
			if (this.appPaused && this.GameState != InnerNetClient.GameStates.Ended && this.AmConnected)
			{
				this.DisconnectInternal(DisconnectReasons.FocusLostBackground, null);
				this.EnqueueDisconnect(DisconnectReasons.FocusLost, null);
			}
		}

		protected void SendInitialData(int clientId)
		{
			MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
			messageWriter.StartMessage(6);
			messageWriter.Write(this.GameId);
			messageWriter.WritePacked(clientId);
			List<InnerNetObject> obj = this.allObjects;
			lock (obj)
			{
				HashSet<GameObject> hashSet = new HashSet<GameObject>();
				for (int i = 0; i < this.allObjects.Count; i++)
				{
					InnerNetObject innerNetObject = this.allObjects[i];
					if (innerNetObject && hashSet.Add(innerNetObject.gameObject))
					{
						this.WriteSpawnMessage(innerNetObject, innerNetObject.OwnerId, innerNetObject.SpawnFlags, messageWriter);
					}
				}
			}
			messageWriter.EndMessage();
			this.SendOrDisconnect(messageWriter);
			messageWriter.Recycle();
		}

		protected abstract void OnGameCreated(string gameIdString);

		protected abstract void OnGameJoined(string gameIdString, ClientData client);

		protected abstract void OnWaitForHost(string gameIdString);

		protected abstract IEnumerator CoStartGame();

		protected abstract void OnGameEnd(GameOverReason reason, bool showAd);

		protected abstract void OnBecomeHost();

		protected abstract void OnPlayerJoined(ClientData client);

		protected abstract IEnumerator CoOnPlayerChangedScene(ClientData client, string targetScene);

		protected abstract void OnPlayerLeft(ClientData client, DisconnectReasons reason);

		protected abstract void OnReportedPlayer(ReportOutcome outcome, int clientId, string playerName, ReportReasons reason);

		protected abstract void OnDisconnected();

		protected abstract void OnGetGameList(InnerNetClient.TotalGameData totalGames, List<GameListing> availableGames);

		protected abstract byte[] GetConnectionData();

		protected ClientData FindClientById(int id)
		{
			if (id < 0)
			{
				return null;
			}
			List<ClientData> obj = this.allClients;
			ClientData result;
			lock (obj)
			{
				for (int i = 0; i < this.allClients.Count; i++)
				{
					ClientData clientData = this.allClients[i];
					if (clientData.Id == id)
					{
						return clientData;
					}
				}
				result = null;
			}
			return result;
		}

		public enum GameStates
		{
			NotJoined,
			Joined,
			Started,
			Ended
		}

		public class TotalGameData
		{
			public int[] PerMapTotals;
		}
	}
}
