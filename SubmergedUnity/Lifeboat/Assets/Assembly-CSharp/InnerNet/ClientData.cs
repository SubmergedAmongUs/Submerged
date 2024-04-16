using System;
using UnityEngine;

namespace InnerNet
{
	[Serializable]
	public class ClientData
	{
		public int Id;

		public bool InScene;

		public bool IsReady;

		public bool HasBeenReported;

		public PlayerControl Character;

		public RuntimePlatform platformID =  (RuntimePlatform) (-1);

		public string PlayerName = "???";

		public int ColorId;

		public ClientData(int id)
		{
			this.Id = id;
		}
	}
}
