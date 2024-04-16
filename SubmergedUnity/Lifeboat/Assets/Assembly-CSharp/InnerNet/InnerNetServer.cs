using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Hazel;
using Hazel.Udp;
using UnityEngine;

namespace InnerNet
{
	public class InnerNetServer : DestroyableSingleton<InnerNetServer>
	{
		public const int LocalGameId = 32;

		private const int InvalidHost = -1;

		private int HostId = -1;

		public HashSet<string> ipBans = new HashSet<string>();

		public int Port = 22023;

		[SerializeField]
		private GameStates GameState;

		private UdpConnectionListener listener;

		private List<InnerNetServer.Player> Clients = new List<InnerNetServer.Player>();

		public override void OnDestroy()
		{
			this.StopServer();
			base.OnDestroy();
		}

		public void StartAsServer()
		{
			if (this.listener != null)
			{
				this.StopServer();
			}
			this.GameState = GameStates.NotStarted;
			this.listener = new UdpConnectionListener(new IPEndPoint(IPAddress.Any, this.Port), 0, delegate(string s)
			{
				Debug.LogError(s);
			});
			this.listener.NewConnection += this.OnServerConnect;
			this.listener.Start();
		}

		public void StartAsLocalServer()
		{
			if (this.listener != null)
			{
				this.StopServer();
			}
			this.GameState = GameStates.NotStarted;
			this.listener = new UdpConnectionListener(new IPEndPoint(IPAddress.Parse("127.0.0.1"), this.Port), 0, delegate(string s)
			{
				Debug.LogError(s);
			});
			this.listener.NewConnection += this.OnServerConnect;
			this.listener.Start();
		}

		private void DebugString(string obj)
		{
			if (!string.IsNullOrWhiteSpace(obj))
			{
				Debug.LogError(obj);
			}
		}

		public void StopServer()
		{
			this.HostId = -1;
			this.GameState = GameStates.Destroyed;
			if (this.listener != null)
			{
				this.listener.Dispose();
				this.listener = null;
			}
			List<InnerNetServer.Player> clients = this.Clients;
			lock (clients)
			{
				this.Clients.Clear();
			}
		}

		public static bool IsCompatibleVersion(int version)
		{
			return Constants.CompatVersions.Contains(version);
		}

		private void OnServerConnect(NewConnectionEventArgs evt)
		{
			MessageReader handshakeData = evt.HandshakeData;
			try
			{
				if (evt.HandshakeData.Length < 5)
				{
					InnerNetServer.SendIncorrectVersion(evt.Connection);
					return;
				}
				if (!InnerNetServer.IsCompatibleVersion(handshakeData.ReadInt32()))
				{
					InnerNetServer.SendIncorrectVersion(evt.Connection);
					return;
				}
			}
			finally
			{
				handshakeData.Recycle();
			}
			InnerNetServer.Player client = new InnerNetServer.Player(evt.Connection);
			Debug.Log(string.Format("Client {0} added: {1}", client.Id, evt.Connection.EndPoint));
			UdpConnection udpConnection = (UdpConnection)evt.Connection;
			udpConnection.KeepAliveInterval = 1500;
			udpConnection.DisconnectTimeout = 6000;
			udpConnection.ResendPingMultiplier = 1.5f;
			udpConnection.DataReceived += delegate(DataReceivedEventArgs e)
			{
				this.OnDataReceived(client, e);
			};
			udpConnection.Disconnected += delegate(object o, DisconnectedEventArgs e)
			{
				this.ClientDisconnect(client);
			};
		}

		private static void SendIncorrectVersion(Connection connection)
		{
			MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
			messageWriter.StartMessage(1);
			messageWriter.Write(5);
			messageWriter.EndMessage();
			connection.Send(messageWriter);
			messageWriter.Recycle();
		}

		private void Connection_DataRecievedRaw(byte[] data, int length)
		{
			Debug.Log("Server Got: " + string.Join(" ", (from b in data
			select b.ToString()).Take(length)));
		}

		private void OnDataReceived(InnerNetServer.Player client, DataReceivedEventArgs evt)
		{
			MessageReader message = evt.Message;
			try
			{
				while (message.Position < message.Length)
				{
					this.HandleMessage(client, message.ReadMessage(), evt.SendOption);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("InnerNetServer::OnDataReceived Exception");
				Debug.LogException(ex, this);
			}
			finally
			{
				message.Recycle();
			}
		}

		private void HandleMessage(InnerNetServer.Player client, MessageReader reader, SendOption sendOption)
		{
			switch (reader.Tag)
			{
			case 0:
			{
				Debug.Log("Server got host game");
				MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
				messageWriter.StartMessage(0);
				messageWriter.Write(32);
				messageWriter.EndMessage();
				client.Connection.Send(messageWriter);
				messageWriter.Recycle();
				return;
			}
			case 1:
			{
				Debug.Log("Server got join game");
				if (reader.ReadInt32() == 32)
				{
					this.JoinGame(client);
					return;
				}
				MessageWriter messageWriter2 = MessageWriter.Get(SendOption.Reliable);
				messageWriter2.StartMessage(1);
				messageWriter2.Write(3);
				messageWriter2.EndMessage();
				client.Connection.Send(messageWriter2);
				messageWriter2.Recycle();
				return;
			}
			case 2:
				if (reader.ReadInt32() == 32)
				{
					this.StartGame(reader, client);
					return;
				}
				break;
			case 3:
				if (reader.ReadInt32() == 32)
				{
					this.ClientDisconnect(client);
					return;
				}
				break;
			case 4:
			case 7:
			case 9:
			case 10:
				break;
			case 5:
				if (this.Clients.Contains(client))
				{
					if (reader.ReadInt32() == 32)
					{
						MessageWriter messageWriter3 = MessageWriter.Get(sendOption);
						messageWriter3.CopyFrom(reader);
						this.Broadcast(messageWriter3, client);
						messageWriter3.Recycle();
						return;
					}
				}
				else if (this.GameState == GameStates.Started)
				{
					Debug.Log("GameDataTo: Server didn't have client");
					client.Connection.Dispose();
					return;
				}
				break;
			case 6:
				if (this.Clients.Contains(client))
				{
					if (reader.ReadInt32() == 32)
					{
						int targetId = reader.ReadPackedInt32();
						MessageWriter messageWriter4 = MessageWriter.Get(sendOption);
						messageWriter4.CopyFrom(reader);
						this.SendTo(messageWriter4, targetId);
						messageWriter4.Recycle();
						return;
					}
				}
				else if (this.GameState == GameStates.Started)
				{
					Debug.Log("GameDataTo: Server didn't have client");
					client.Connection.Dispose();
					return;
				}
				break;
			case 8:
				if (reader.ReadInt32() == 32)
				{
					this.EndGame(reader, client);
					return;
				}
				break;
			case 11:
				if (reader.ReadInt32() == 32)
				{
					this.KickPlayer(reader.ReadPackedInt32(), reader.ReadBoolean());
				}
				break;
			default:
				return;
			}
		}

		private void KickPlayer(int targetId, bool ban)
		{
			List<InnerNetServer.Player> clients = this.Clients;
			lock (clients)
			{
				InnerNetServer.Player player = null;
				for (int i = 0; i < this.Clients.Count; i++)
				{
					if (this.Clients[i].Id == targetId)
					{
						player = this.Clients[i];
						break;
					}
				}
				if (player != null)
				{
					if (ban)
					{
						HashSet<string> obj = this.ipBans;
						lock (obj)
						{
							IPEndPoint endPoint = player.Connection.EndPoint;
							this.ipBans.Add(endPoint.Address.ToString());
						}
					}
					MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
					messageWriter.StartMessage(11);
					messageWriter.Write(32);
					messageWriter.WritePacked(targetId);
					messageWriter.Write(ban);
					messageWriter.EndMessage();
					this.Broadcast(messageWriter, null);
					messageWriter.Recycle();
				}
			}
		}

		protected void JoinGame(InnerNetServer.Player client)
		{
			HashSet<string> obj = this.ipBans;
			lock (obj)
			{
				IPEndPoint endPoint = client.Connection.EndPoint;
				if (this.ipBans.Contains(endPoint.Address.ToString()))
				{
					MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
					messageWriter.StartMessage(1);
					messageWriter.Write(6);
					messageWriter.EndMessage();
					client.Connection.Send(messageWriter);
					messageWriter.Recycle();
					return;
				}
			}
			List<InnerNetServer.Player> clients = this.Clients;
			lock (clients)
			{
				switch (this.GameState)
				{
				case GameStates.NotStarted:
					this.HandleNewGameJoin(client);
					break;
				default:
				{
					MessageWriter messageWriter2 = MessageWriter.Get(SendOption.Reliable);
					messageWriter2.StartMessage(1);
					messageWriter2.Write(2);
					messageWriter2.EndMessage();
					client.Connection.Send(messageWriter2);
					messageWriter2.Recycle();
					break;
				}
				case GameStates.Ended:
					this.HandleRejoin(client);
					break;
				}
			}
		}

		private void HandleRejoin(InnerNetServer.Player client)
		{
			if (client.Id == this.HostId)
			{
				this.GameState = GameStates.NotStarted;
				this.HandleNewGameJoin(client);
				MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
				for (int i = 0; i < this.Clients.Count; i++)
				{
					InnerNetServer.Player player = this.Clients[i];
					if (player != client)
					{
						try
						{
							this.WriteJoinedMessage(player, messageWriter, true);
							player.Connection.Send(messageWriter);
						}
						catch (Exception ex)
						{
							Debug.LogError("InnerNetServer::HandleRejoin Exception: " + ex.Message);
							Debug.LogException(ex, this);
						}
					}
				}
				messageWriter.Recycle();
				return;
			}
			if (this.Clients.Count >= 14)
			{
				MessageWriter messageWriter2 = MessageWriter.Get(SendOption.Reliable);
				messageWriter2.StartMessage(1);
				messageWriter2.Write(1);
				messageWriter2.EndMessage();
				client.Connection.Send(messageWriter2);
				messageWriter2.Recycle();
				return;
			}
			this.Clients.Add(client);
			client.LimboState = LimboStates.WaitingForHost;
			MessageWriter messageWriter3 = MessageWriter.Get(SendOption.Reliable);
			try
			{
				messageWriter3.StartMessage(12);
				messageWriter3.Write(32);
				messageWriter3.Write(client.Id);
				messageWriter3.EndMessage();
				client.Connection.Send(messageWriter3);
				this.BroadcastJoinMessage(client, messageWriter3);
			}
			catch (Exception ex2)
			{
				Debug.LogError("InnerNetServer::HandleRejoin MessageWriter Exception: " + ex2.Message);
				Debug.LogException(ex2, this);
			}
			finally
			{
				messageWriter3.Recycle();
			}
		}

		private void HandleNewGameJoin(InnerNetServer.Player client)
		{
			if (this.Clients.Count >= 15)
			{
				MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
				try
				{
					messageWriter.StartMessage(1);
					messageWriter.Write(1);
					messageWriter.EndMessage();
					client.Connection.Send(messageWriter);
				}
				catch (Exception ex)
				{
					Debug.LogError("InnerNetServer::HandleNewGameJoin MessageWriter 1 Exception: " + ex.Message);
					Debug.LogException(ex, this);
				}
				finally
				{
					messageWriter.Recycle();
				}
				return;
			}
			this.Clients.Add(client);
			client.LimboState = LimboStates.PreSpawn;
			if (this.HostId == -1)
			{
				this.HostId = this.Clients[0].Id;
			}
			if (this.HostId == client.Id)
			{
				client.LimboState = LimboStates.NotLimbo;
			}
			MessageWriter messageWriter2 = MessageWriter.Get(SendOption.Reliable);
			try
			{
				this.WriteJoinedMessage(client, messageWriter2, true);
				client.Connection.Send(messageWriter2);
				this.BroadcastJoinMessage(client, messageWriter2);
			}
			catch (Exception ex2)
			{
				Debug.LogError("InnerNetServer::HandleNewGameJoin MessageWriter 2 Exception: " + ex2.Message);
				Debug.LogException(ex2, this);
			}
			finally
			{
				messageWriter2.Recycle();
			}
		}

		private void EndGame(MessageReader message, InnerNetServer.Player source)
		{
			if (source.Id == this.HostId)
			{
				this.GameState = GameStates.Ended;
				MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
				messageWriter.CopyFrom(message);
				this.Broadcast(messageWriter, null);
				messageWriter.Recycle();
				List<InnerNetServer.Player> clients = this.Clients;
				lock (clients)
				{
					this.Clients.Clear();
					return;
				}
			}
			Debug.LogWarning("Reset request rejected from: " + source.Id.ToString());
		}

		private void StartGame(MessageReader message, InnerNetServer.Player source)
		{
			this.GameState = GameStates.Started;
			MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
			messageWriter.CopyFrom(message);
			this.Broadcast(messageWriter, null);
			messageWriter.Recycle();
		}

		private void ClientDisconnect(InnerNetServer.Player client)
		{
			Debug.Log("Server DC client " + client.Id.ToString());
			MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
			messageWriter.StartMessage(4);
			messageWriter.Write(32);
			messageWriter.Write(client.Id);
			messageWriter.Write(this.HostId);
			messageWriter.Write(0);
			messageWriter.EndMessage();
			this.Broadcast(messageWriter, null);
			messageWriter.Recycle();
			List<InnerNetServer.Player> clients = this.Clients;
			lock (clients)
			{
				this.Clients.Remove(client);
				client.Connection.Dispose();
				if (this.Clients.Count > 0)
				{
					this.HostId = this.Clients[0].Id;
				}
			}
		}

		protected void SendTo(MessageWriter msg, int targetId)
		{
			List<InnerNetServer.Player> clients = this.Clients;
			lock (clients)
			{
				for (int i = 0; i < this.Clients.Count; i++)
				{
					InnerNetServer.Player player = this.Clients[i];
					if (player.Id == targetId)
					{
						try
						{
							player.Connection.Send(msg);
							break;
						}
						catch (Exception ex)
						{
							Debug.LogError("InnerNetServer::SendTo Exception");
							Debug.LogException(ex, this);
							break;
						}
					}
				}
			}
		}

		protected void Broadcast(MessageWriter msg, InnerNetServer.Player source)
		{
			List<InnerNetServer.Player> clients = this.Clients;
			lock (clients)
			{
				for (int i = 0; i < this.Clients.Count; i++)
				{
					InnerNetServer.Player player = this.Clients[i];
					if (player != source)
					{
						try
						{
							player.Connection.Send(msg);
						}
						catch (Exception ex)
						{
							Debug.LogError("InnerNetServer::Broadcast Exception");
							Debug.LogException(ex, this);
						}
					}
				}
			}
		}

		private void BroadcastJoinMessage(InnerNetServer.Player client, MessageWriter msg)
		{
			msg.Clear(SendOption.Reliable);
			msg.StartMessage(1);
			msg.Write(32);
			msg.Write(client.Id);
			msg.Write(this.HostId);
			msg.EndMessage();
			this.Broadcast(msg, client);
		}

		private void WriteJoinedMessage(InnerNetServer.Player client, MessageWriter msg, bool clear)
		{
			if (clear)
			{
				msg.Clear(SendOption.Reliable);
			}
			msg.StartMessage(7);
			msg.Write(32);
			msg.Write(client.Id);
			msg.Write(this.HostId);
			msg.WritePacked(this.Clients.Count - 1);
			for (int i = 0; i < this.Clients.Count; i++)
			{
				InnerNetServer.Player player = this.Clients[i];
				if (player != client)
				{
					msg.WritePacked(player.Id);
				}
			}
			msg.EndMessage();
		}

		protected class Player
		{
			private static int IdCount = 1;

			public int Id;

			public Connection Connection;

			public LimboStates LimboState;

			public Player(Connection connection)
			{
				this.Id = Interlocked.Increment(ref InnerNetServer.Player.IdCount);
				this.Connection = connection;
			}
		}
	}
}
