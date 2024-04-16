using System;
using System.Linq;
using Hazel;

public class AutoDoorsSystemType : ISystemType, IActivatable, RunTimer, IDoorSystem
{
	private uint dirtyBits;

	public bool IsActive
	{
		get
		{
			return ShipStatus.Instance.AllDoors.Any((PlainDoor b) => !b.Open);
		}
	}

	public bool IsDirty
	{
		get
		{
			return this.dirtyBits > 0U;
		}
	}

	public void Detoriorate(float deltaTime)
	{
		for (int i = 0; i < ShipStatus.Instance.AllDoors.Length; i++)
		{
			if (ShipStatus.Instance.AllDoors[i].DoUpdate(deltaTime))
			{
				this.dirtyBits |= 1U << i;
			}
		}
	}

	public void RepairDamage(PlayerControl player, byte amount)
	{
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		if (initialState)
		{
			for (int i = 0; i < ShipStatus.Instance.AllDoors.Length; i++)
			{
				ShipStatus.Instance.AllDoors[i].Serialize(writer);
			}
			return;
		}
		writer.WritePacked(this.dirtyBits);
		for (int j = 0; j < ShipStatus.Instance.AllDoors.Length; j++)
		{
			if ((this.dirtyBits & 1U << j) != 0U)
			{
				ShipStatus.Instance.AllDoors[j].Serialize(writer);
			}
		}
		this.dirtyBits = 0U;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		if (initialState)
		{
			for (int i = 0; i < ShipStatus.Instance.AllDoors.Length; i++)
			{
				ShipStatus.Instance.AllDoors[i].Deserialize(reader);
			}
			return;
		}
		uint num = reader.ReadPackedUInt32();
		for (int j = 0; j < ShipStatus.Instance.AllDoors.Length; j++)
		{
			if ((num & 1U << j) != 0U)
			{
				ShipStatus.Instance.AllDoors[j].Deserialize(reader);
			}
		}
	}

	public void SetDoor(AutoOpenDoor door, bool open)
	{
		door.SetDoorway(open);
		this.dirtyBits |= 1U << ShipStatus.Instance.AllDoors.IndexOf(door);
	}

	public void CloseDoorsOfType(SystemTypes room)
	{
		for (int i = 0; i < ShipStatus.Instance.AllDoors.Length; i++)
		{
			PlainDoor plainDoor = ShipStatus.Instance.AllDoors[i];
			if (plainDoor.Room == room)
			{
				plainDoor.SetDoorway(false);
				this.dirtyBits |= 1U << i;
			}
		}
	}

	public float GetTimer(SystemTypes room)
	{
		for (int i = 0; i < ShipStatus.Instance.AllDoors.Length; i++)
		{
			PlainDoor plainDoor = ShipStatus.Instance.AllDoors[i];
			if (plainDoor.Room == room)
			{
				return ((AutoOpenDoor)plainDoor).CooldownTimer;
			}
		}
		return 0f;
	}
}
