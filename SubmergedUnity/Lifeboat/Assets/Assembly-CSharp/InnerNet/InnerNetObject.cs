using System;
using Hazel;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InnerNet
{
	public abstract class InnerNetObject : MonoBehaviour, IComparable<InnerNetObject>
	{
		public uint SpawnId;

		public uint NetId;

		protected uint DirtyBits;

		public SpawnFlags SpawnFlags;

		public SendOption sendMode = SendOption.Reliable;

		public int OwnerId;

		protected bool DespawnOnDestroy = true;

		public virtual bool IsDirty
		{
			get
			{
				return this.DirtyBits > 0U;
			}
		}

		public bool AmOwner
		{
			get
			{
				return this.OwnerId == AmongUsClient.Instance.ClientId;
			}
		}

		public void Despawn()
		{
			AmongUsClient.Instance.Despawn(this);
			 UnityEngine.Object.Destroy(base.gameObject);
		}

		public virtual void OnDestroy()
		{
			if (AmongUsClient.Instance && this.NetId != 4294967295U)
			{
				if (this.DespawnOnDestroy && this.AmOwner)
				{
					AmongUsClient.Instance.Despawn(this);
					return;
				}
				AmongUsClient.Instance.RemoveNetObject(this);
			}
		}

		public abstract void HandleRpc(byte callId, MessageReader reader);

		public abstract bool Serialize(MessageWriter writer, bool initialState);

		public abstract void Deserialize(MessageReader reader, bool initialState);

		public int CompareTo(InnerNetObject other)
		{
			if (this.NetId > other.NetId)
			{
				return 1;
			}
			if (this.NetId < other.NetId)
			{
				return -1;
			}
			return 0;
		}

		protected bool IsDirtyBitSet(int idx)
		{
			return (this.DirtyBits & 1U << idx) > 0U;
		}

		protected void ClearDirtyBits()
		{
			this.DirtyBits = 0U;
		}

		protected void SetDirtyBit(uint val)
		{
			this.DirtyBits |= val;
		}
	}
}
