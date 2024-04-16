using System;
using System.Collections;
using System.Net.Sockets;
using Hazel.Udp;
using UnityEngine;

namespace InnerNet
{
	public class InnerDiscover : DestroyableSingleton<InnerDiscover>
	{
		private UdpBroadcastListener listener;

		private UdpBroadcaster sender;

		public int Port = 47777;

		public float Interval = 1f;

		public event Action<BroadcastPacket> OnPacketGet;

		public void StartAsServer(string data)
		{
			bool flag = this.sender == null;
			if (flag)
			{
				this.sender = new UdpBroadcaster(this.Port, new Action<string>(Debug.LogError));
			}
			this.sender.SetData(data);
			if (flag)
			{
				base.StartCoroutine(this.RunServer());
			}
		}

		private IEnumerator RunServer()
		{
			while (this.sender != null)
			{
				this.sender.Broadcast();
				for (float timer = 0f; timer < this.Interval; timer += Time.deltaTime)
				{
					yield return null;
				}
			}
			yield break;
		}

		public void StopServer()
		{
			if (this.sender != null)
			{
				this.sender.Dispose();
				this.sender = null;
			}
		}

		public void StartAsClient()
		{
			if (this.listener == null)
			{
				try
				{
					this.listener = new UdpBroadcastListener(this.Port, new Action<string>(Debug.LogError));
					this.listener.StartListen();
					base.StartCoroutine(this.RunClient());
				}
				catch (SocketException ex)
				{
					Debug.LogError("InnerDiscover::StartAsClient SocketException");
					Debug.LogException(ex, this);
					AmongUsClient.Instance.LastDisconnectReason = DisconnectReasons.Custom;
					AmongUsClient.Instance.LastCustomDisconnect = "Couldn't start local network listener. You may need to restart Among Us.";
					DestroyableSingleton<DisconnectPopup>.Instance.Show();
				}
			}
		}

		private IEnumerator RunClient()
		{
			while (this.listener != null)
			{
				this.listener.StartListen();
				BroadcastPacket[] packets = this.listener.GetPackets();
				for (int i = 0; i < packets.Length; i++)
				{
					Action<BroadcastPacket> onPacketGet = this.OnPacketGet;
					if (onPacketGet != null)
					{
						onPacketGet(packets[i]);
					}
				}
				yield return null;
			}
			yield break;
		}

		public void StopClient()
		{
			if (this.listener != null)
			{
				this.listener.Dispose();
				this.listener = null;
			}
		}

		public override void OnDestroy()
		{
			this.StopServer();
			this.StopClient();
			base.OnDestroy();
		}
	}
}
