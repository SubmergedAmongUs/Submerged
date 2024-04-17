using Hazel;
using UnityEngine;

namespace InnerNet
{
	public abstract class InnerNetObject : MonoBehaviour
	{
		public uint SpawnId;
		public uint NetId;
		public SpawnFlags SpawnFlags;
		public SendOption sendMode = (SendOption)1;
		public int OwnerId;
	}
}
