using System;
using System.Collections.Generic;
using System.Linq;
using Beebyte.Obfuscator;
using Hazel;
using InnerNet;
using UnityEngine;
using Random = System.Random;

[SkipRename]
public class GameData : InnerNetObject, IDisconnectHandler
{
	public static GameData Instance;

	public List<GameData.PlayerInfo> AllPlayers = new List<GameData.PlayerInfo>();

	public int TotalTasks;

	public int CompletedTasks;

	public const byte InvalidPlayerId = 255;

	public const byte DisconnectedPlayerId = 254;

	public static Random randy = new Random(0);

	public void RpcSetTasks(byte playerId, byte[] taskTypeIds)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			this.SetTasks(playerId, taskTypeIds);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(this.NetId, 29, SendOption.Reliable);
		messageWriter.Write(playerId);
		messageWriter.WriteBytesAndSize(taskTypeIds);
		messageWriter.EndMessage();
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		if (initialState)
		{
			writer.WritePacked(this.AllPlayers.Count);
			for (int i = 0; i < this.AllPlayers.Count; i++)
			{
				GameData.PlayerInfo playerInfo = this.AllPlayers[i];
				writer.Write(playerInfo.PlayerId);
				playerInfo.Serialize(writer);
			}
		}
		else
		{
			for (int j = 0; j < this.AllPlayers.Count; j++)
			{
				GameData.PlayerInfo playerInfo2 = this.AllPlayers[j];
				if (base.IsDirtyBitSet((int)playerInfo2.PlayerId))
				{
					writer.StartMessage(playerInfo2.PlayerId);
					playerInfo2.Serialize(writer);
					writer.EndMessage();
				}
			}
			base.ClearDirtyBits();
		}
		return true;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
		if (initialState)
		{
			int num = reader.ReadPackedInt32();
			for (int i = 0; i < num; i++)
			{
				GameData.PlayerInfo playerInfo = new GameData.PlayerInfo(reader.ReadByte());
				playerInfo.Deserialize(reader);
				this.AllPlayers.Add(playerInfo);
			}
		}
		else
		{
			while (reader.Position < reader.Length)
			{
				MessageReader messageReader = reader.ReadMessage();
				GameData.PlayerInfo playerInfo2 = this.GetPlayerById(messageReader.Tag);
				if (playerInfo2 != null)
				{
					playerInfo2.Deserialize(messageReader);
				}
				else
				{
					playerInfo2 = new GameData.PlayerInfo(messageReader.Tag);
					playerInfo2.Deserialize(messageReader);
					this.AllPlayers.Add(playerInfo2);
				}
			}
		}
		this.RecomputeTaskCounts();
	}

	public override void HandleRpc(byte callId, MessageReader reader)
	{
		if (callId == 29)
		{
			this.SetTasks(reader.ReadByte(), reader.ReadBytesAndSize());
		}
	}

	public int PlayerCount
	{
		get
		{
			return this.AllPlayers.Count;
		}
	}

	public void Awake()
	{
		if (GameData.Instance && GameData.Instance != this)
		{
			Debug.Log("Destroying dupe GameData");
			 UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		GameData.Instance = this;
		if (AmongUsClient.Instance)
		{
			AmongUsClient.Instance.DisconnectHandlers.AddUnique(this);
		}
	}

	internal void SetDirty()
	{
		base.SetDirtyBit(uint.MaxValue);
	}

	public GameData.PlayerInfo GetHost()
	{
		ClientData host = AmongUsClient.Instance.GetHost();
		if (host != null && host.Character)
		{
			return host.Character.Data;
		}
		return null;
	}

	public sbyte GetAvailableId()
	{
		sbyte result = -1;
		do
		{
			result += 1;
		} while (this.AllPlayers.Any(p => p.PlayerId == result));

		return result;
	}

	public GameData.PlayerInfo GetPlayerById(byte id)
	{
		if (id == 255)
		{
			return null;
		}
		for (int i = 0; i < this.AllPlayers.Count; i++)
		{
			if (this.AllPlayers[i].PlayerId == id)
			{
				return this.AllPlayers[i];
			}
		}
		return null;
	}

	public void UpdateName(byte playerId, string name, bool dontCensor = false)
	{
		GameData.PlayerInfo playerById = this.GetPlayerById(playerId);
		if (playerById != null)
		{
			playerById.dontCensorName = dontCensor;
			playerById.PlayerName = name;
			ClientData clientFromCharacter = AmongUsClient.Instance.GetClientFromCharacter(playerById.Object);
			if (clientFromCharacter != null)
			{
				clientFromCharacter.PlayerName = name;
			}
		}
		base.SetDirtyBit(1U << (int)playerId);
	}

	public void UpdateColor(byte playerId, int color)
	{
		GameData.PlayerInfo playerById = this.GetPlayerById(playerId);
		if (playerById != null)
		{
			playerById.ColorId = color;
			ClientData clientFromCharacter = AmongUsClient.Instance.GetClientFromCharacter(playerById.Object);
			if (clientFromCharacter != null)
			{
				clientFromCharacter.ColorId = color;
			}
		}
		base.SetDirtyBit(1U << (int)playerId);
	}

	public void UpdateHat(byte playerId, uint hat)
	{
		GameData.PlayerInfo playerById = this.GetPlayerById(playerId);
		if (playerById != null)
		{
			playerById.HatId = hat;
		}
		base.SetDirtyBit(1U << (int)playerId);
	}

	public void UpdatePet(byte playerId, uint petId)
	{
		GameData.PlayerInfo playerById = this.GetPlayerById(playerId);
		if (playerById != null)
		{
			playerById.PetId = petId;
		}
		base.SetDirtyBit(1U << (int)playerId);
	}

	public void UpdateSkin(byte playerId, uint skin)
	{
		GameData.PlayerInfo playerById = this.GetPlayerById(playerId);
		if (playerById != null)
		{
			playerById.SkinId = skin;
		}
		base.SetDirtyBit(1U << (int)playerId);
	}

	public PlayerInfo AddPlayer(PlayerControl pc)
	{
		GameData.PlayerInfo playerInfo = new GameData.PlayerInfo(pc);
		this.AllPlayers.Add(playerInfo);
		base.SetDirtyBit(1U << (int)pc.PlayerId);
		return playerInfo;
	}

	public bool RemovePlayer(byte playerId)
	{
		for (int i = 0; i < this.AllPlayers.Count; i++)
		{
			if (this.AllPlayers[i].PlayerId == playerId)
			{
				this.SetDirty();
				this.AllPlayers.RemoveAt(i);
				return true;
			}
		}
		return false;
	}

	public void RecomputeTaskCounts()
	{
		this.TotalTasks = 0;
		this.CompletedTasks = 0;
		for (int i = 0; i < this.AllPlayers.Count; i++)
		{
			GameData.PlayerInfo playerInfo = this.AllPlayers[i];
			if (!playerInfo.Disconnected && playerInfo.Tasks != null && playerInfo.Object && (PlayerControl.GameOptions.GhostsDoTasks || !playerInfo.IsDead) && !playerInfo.IsImpostor)
			{
				for (int j = 0; j < playerInfo.Tasks.Count; j++)
				{
					this.TotalTasks++;
					if (playerInfo.Tasks[j].Complete)
					{
						this.CompletedTasks++;
					}
				}
			}
		}
	}

	public void TutOnlyRemoveTask(byte playerId, uint taskId)
	{
		GameData.PlayerInfo playerById = this.GetPlayerById(playerId);
		GameData.TaskInfo item = playerById.FindTaskById(taskId);
		playerById.Tasks.Remove(item);
		this.RecomputeTaskCounts();
	}

	public uint TutOnlyAddTask(byte playerId)
	{
		GameData.PlayerInfo playerById = this.GetPlayerById(playerId);
		uint num = (from d in playerById.Tasks
		select d.Id).Max<uint>() + 1U;
		playerById.Tasks.Add(new GameData.TaskInfo
		{
			Id = num
		});
		this.TotalTasks++;
		return num;
	}

	private void SetTasks(byte playerId, byte[] taskTypeIds)
	{
		GameData.PlayerInfo playerById = this.GetPlayerById(playerId);
		if (playerById == null)
		{
			Debug.Log("Could not set tasks for player id: " + playerId.ToString());
			return;
		}
		if (playerById.Disconnected)
		{
			return;
		}
		if (!playerById.Object)
		{
			Debug.Log("Could not set tasks for player (" + playerById.PlayerName + "): " + playerId.ToString());
			return;
		}
		playerById.Tasks = new List<GameData.TaskInfo>(taskTypeIds.Length);
		for (int i = 0; i < taskTypeIds.Length; i++)
		{
			playerById.Tasks.Add(new GameData.TaskInfo(taskTypeIds[i], (uint)i));
			playerById.Tasks[i].Id = (uint)i;
		}
		playerById.Object.SetTasks(playerById.Tasks);
		base.SetDirtyBit(1U << (int)playerById.PlayerId);
	}

	public void CompleteTask(PlayerControl pc, uint taskId)
	{
		GameData.TaskInfo taskInfo = pc.Data.FindTaskById(taskId);
		if (taskInfo == null)
		{
			Debug.LogWarning("Couldn't find task: " + taskId.ToString());
			return;
		}
		if (!taskInfo.Complete)
		{
			taskInfo.Complete = true;
			this.CompletedTasks++;
			return;
		}
		Debug.LogWarning("Double complete task: " + taskId.ToString());
	}

	public void HandleDisconnect(PlayerControl player, DisconnectReasons reason)
	{
		if (!player)
		{
			return;
		}
		GameData.PlayerInfo playerById = this.GetPlayerById(player.PlayerId);
		if (playerById == null)
		{
			return;
		}
		if (AmongUsClient.Instance.IsGameStarted)
		{
			if (!playerById.Disconnected)
			{
				playerById.Disconnected = true;
				TempData.LastDeathReason = DeathReason.Disconnect;
				this.ShowNotification(playerById.PlayerName, reason);
			}
		}
		else if (this.RemovePlayer(player.PlayerId))
		{
			this.ShowNotification(playerById.PlayerName, reason);
		}
		this.RecomputeTaskCounts();
	}

	private void ShowNotification(string playerName, DisconnectReasons reason)
	{
		if (string.IsNullOrEmpty(playerName))
		{
			return;
		}
		if (reason <= DisconnectReasons.Banned)
		{
			if (reason == DisconnectReasons.ExitGame)
			{
				DestroyableSingleton<HudManager>.Instance.Notifier.AddItem(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.UserLeftGame, new object[]
				{
					playerName
				}));
				return;
			}
			if (reason == DisconnectReasons.Banned)
			{
				GameData.PlayerInfo data = AmongUsClient.Instance.GetHost().Character.Data;
				DestroyableSingleton<HudManager>.Instance.Notifier.AddItem(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.PlayerWasBannedBy, new object[]
				{
					playerName,
					data.PlayerName
				}));
				return;
			}
		}
		else if (reason == DisconnectReasons.Kicked)
		{
			GameData.PlayerInfo data2 = AmongUsClient.Instance.GetHost().Character.Data;
			DestroyableSingleton<HudManager>.Instance.Notifier.AddItem(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.PlayerWasKickedBy, new object[]
			{
				playerName,
				data2.PlayerName
			}));
			return;
		}
		DestroyableSingleton<HudManager>.Instance.Notifier.AddItem(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.LeftGameError, new object[]
		{
			playerName
		}));
	}

	public void HandleDisconnect()
	{
		if (!AmongUsClient.Instance.IsGameStarted)
		{
			for (int i = this.AllPlayers.Count - 1; i >= 0; i--)
			{
				if (!this.AllPlayers[i].Object)
				{
					this.AllPlayers.RemoveAt(i);
				}
			}
		}
	}

	public class TaskInfo
	{
		public uint Id;

		public byte TypeId;

		public bool Complete;

		public TaskInfo()
		{
		}

		public TaskInfo(byte typeId, uint id)
		{
			this.Id = id;
			this.TypeId = typeId;
		}

		public void Serialize(MessageWriter writer)
		{
			writer.WritePacked(this.Id);
			writer.Write(this.Complete);
		}

		public void Deserialize(MessageReader reader)
		{
			this.Id = reader.ReadPackedUInt32();
			this.Complete = reader.ReadBoolean();
		}
	}

	public class PlayerInfo
	{
		public readonly byte PlayerId;

		private string _playerName = string.Empty;

		public bool dontCensorName;

		public int ColorId = -1;

		public uint HatId = uint.MaxValue;

		public uint PetId = uint.MaxValue;

		public uint SkinId = uint.MaxValue;

		public bool Disconnected;

		public List<GameData.TaskInfo> Tasks;

		public bool IsImpostor;

		public bool IsDead;

		private PlayerControl _object;

		public string PlayerName
		{
			get
			{
				return this._playerName;
			}
			set
			{
				this._playerName = value;
			}
		}

		public bool IsIncomplete
		{
			get
			{
				return string.IsNullOrEmpty(this.PlayerName) || this.ColorId == -1 || this.HatId == uint.MaxValue || this.PetId == uint.MaxValue || this.SkinId == uint.MaxValue;
			}
		}

		public PlayerControl Object
		{
			get
			{
				if (!this._object)
				{
					this._object = PlayerControl.AllPlayerControls.FirstOrDefault((PlayerControl p) => p.PlayerId == this.PlayerId);
				}
				return this._object;
			}
		}

		public PlayerInfo(byte playerId)
		{
			this.PlayerId = playerId;
		}

		public PlayerInfo(PlayerControl pc) : this(pc.PlayerId)
		{
			this._object = pc;
		}

		public void Serialize(MessageWriter writer)
		{
			writer.Write(this.PlayerName);
			writer.WritePacked(this.ColorId);
			writer.WritePacked(this.HatId);
			writer.WritePacked(this.PetId);
			writer.WritePacked(this.SkinId);
			byte b = 0;
			if (this.Disconnected)
			{
				b |= 1;
			}
			if (this.IsImpostor)
			{
				b |= 2;
			}
			if (this.IsDead)
			{
				b |= 4;
			}
			writer.Write(b);
			if (this.Tasks != null)
			{
				writer.Write((byte)this.Tasks.Count);
				for (int i = 0; i < this.Tasks.Count; i++)
				{
					this.Tasks[i].Serialize(writer);
				}
				return;
			}
			writer.Write(0);
		}

		public void Deserialize(MessageReader reader)
		{
			this.PlayerName = reader.ReadString();
			this.ColorId = reader.ReadPackedInt32();
			this.HatId = reader.ReadPackedUInt32();
			this.PetId = reader.ReadPackedUInt32();
			this.SkinId = reader.ReadPackedUInt32();
			byte b = reader.ReadByte();
			this.Disconnected = ((b & 1) > 0);
			this.IsImpostor = ((b & 2) > 0);
			this.IsDead = ((b & 4) > 0);
			byte b2 = reader.ReadByte();
			this.Tasks = new List<GameData.TaskInfo>((int)b2);
			for (int i = 0; i < (int)b2; i++)
			{
				this.Tasks.Add(new GameData.TaskInfo());
				this.Tasks[i].Deserialize(reader);
			}
		}

		public GameData.TaskInfo FindTaskById(uint taskId)
		{
			for (int i = 0; i < this.Tasks.Count; i++)
			{
				if (this.Tasks[i].Id == taskId)
				{
					return this.Tasks[i];
				}
			}
			return null;
		}
	}
}
