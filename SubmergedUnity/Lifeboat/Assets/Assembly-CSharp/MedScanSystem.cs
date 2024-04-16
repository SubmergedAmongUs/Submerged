using System;
using System.Collections.Generic;
using Hazel;
using UnityEngine;

public class MedScanSystem : ISystemType
{
	public const byte Request = 128;

	public const byte Release = 64;

	public const byte NumMask = 31;

	public const byte NoPlayer = 255;

	public List<byte> UsersList = new List<byte>();

	public byte CurrentUser { get; private set; } = byte.MaxValue;

	public bool IsDirty { get; private set; }

	public void Detoriorate(float deltaTime)
	{
		if (this.UsersList.Count > 0)
		{
			if (this.CurrentUser != this.UsersList[0])
			{
				if (this.CurrentUser != 255)
				{
					Debug.Log("Released scanner from: " + this.CurrentUser.ToString());
				}
				this.CurrentUser = this.UsersList[0];
				Debug.Log("Acquired scanner for: " + this.CurrentUser.ToString());
				this.IsDirty = true;
				return;
			}
		}
		else if (this.CurrentUser != 255)
		{
			Debug.Log("Released scanner from: " + this.CurrentUser.ToString());
			this.CurrentUser = byte.MaxValue;
			this.IsDirty = true;
		}
	}

	public void RepairDamage(PlayerControl player, byte data)
	{
		byte playerId = (byte) (data & 31);
		if ((data & 128) != 0)
		{
			if (!this.UsersList.Contains(playerId))
			{
				Debug.Log("Added to queue: " + playerId.ToString());
				this.UsersList.Add(playerId);
			}
		}
		else if ((data & 64) != 0)
		{
			Debug.Log("Removed from queue: " + playerId.ToString());
			this.UsersList.RemoveAll((byte v) => v == playerId);
		}
		this.IsDirty = true;
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.WritePacked(this.UsersList.Count);
		for (int i = 0; i < this.UsersList.Count; i++)
		{
			writer.Write(this.UsersList[i]);
		}
		this.IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		this.UsersList.Clear();
		int num = reader.ReadPackedInt32();
		for (int i = 0; i < num; i++)
		{
			this.UsersList.Add(reader.ReadByte());
		}
	}
}
