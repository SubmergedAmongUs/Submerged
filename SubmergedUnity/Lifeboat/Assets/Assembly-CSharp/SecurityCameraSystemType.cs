using System;
using System.Collections.Generic;
using Hazel;

public class SecurityCameraSystemType : ISystemType
{
	public const byte IncrementOp = 1;

	public const byte DecrementOp = 2;

	private HashSet<byte> PlayersUsing = new HashSet<byte>();

	private HashSet<byte> ToRemove = new HashSet<byte>();

	public bool InUse
	{
		get
		{
			return this.PlayersUsing.Count > 0;
		}
	}

	public bool IsDirty { get; private set; }

	public void Detoriorate(float deltaTime)
	{
		if (GameData.Instance)
		{
			foreach (byte b in this.PlayersUsing)
			{
				GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(b);
				if (playerById == null || playerById.Disconnected)
				{
					this.ToRemove.Add(b);
				}
			}
			if (this.ToRemove.Count > 0)
			{
				this.PlayersUsing.ExceptWith(this.ToRemove);
				this.ToRemove.Clear();
				this.UpdateCameras();
				this.IsDirty = true;
			}
		}
	}

	public void RepairDamage(PlayerControl player, byte amount)
	{
		if (amount == 1)
		{
			this.PlayersUsing.Add(player.PlayerId);
		}
		else
		{
			this.PlayersUsing.Remove(player.PlayerId);
		}
		this.IsDirty = true;
		this.UpdateCameras();
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
	}

	private void UpdateCameras()
	{
		for (int i = 0; i < ShipStatus.Instance.AllCameras.Length; i++)
		{
			ShipStatus.Instance.AllCameras[i].SetAnimation(this.InUse);
		}
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.WritePacked(this.PlayersUsing.Count);
		foreach (byte b in this.PlayersUsing)
		{
			writer.Write(b);
		}
		this.IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		this.PlayersUsing.Clear();
		int num = reader.ReadPackedInt32();
		for (int i = 0; i < num; i++)
		{
			this.PlayersUsing.Add(reader.ReadByte());
		}
		this.UpdateCameras();
	}
}
