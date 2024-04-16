using System;
using System.Collections.Generic;
using Hazel;
using UnityEngine;

public class DoorsSystemType : ISystemType, IActivatable, RunTimer, IDoorSystem
{
	public const byte CloseDoors = 128;

	public const byte OpenDoor = 64;

	private const byte ActionMask = 192;

	private const byte IdMask = 31;

	private Dictionary<SystemTypes, float> timers = new Dictionary<SystemTypes, float>();

	public bool IsActive
	{
		get
		{
			return false;
		}
	}

	public bool IsDirty { get; private set; }

	public void Detoriorate(float deltaTime)
	{
		for (int i = 0; i < SystemTypeHelpers.AllTypes.Length; i++)
		{
			SystemTypes key = SystemTypeHelpers.AllTypes[i];
			float num;
			if (this.timers.TryGetValue(key, out num))
			{
				this.timers[key] = Mathf.Clamp(num - deltaTime, 0f, 30f);
			}
		}
	}

	public void RepairDamage(PlayerControl player, byte amount)
	{
		int num = (int)(amount & 31);
		int num2 = (int)(amount & 192);
		if (num2 != 64)
		{
			if (num2 != 128)
			{
			}
		}
		else if (num < ShipStatus.Instance.AllDoors.Length)
		{
			ShipStatus.Instance.AllDoors[num].SetDoorway(true);
		}
		else
		{
			Debug.LogWarning(string.Format("Couldn't find door {0}", num));
		}
		this.IsDirty = true;
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.Write((byte)this.timers.Count);
		foreach (KeyValuePair<SystemTypes, float> keyValuePair in this.timers)
		{
			writer.Write((byte)keyValuePair.Key);
			writer.Write(keyValuePair.Value);
		}
		for (int i = 0; i < ShipStatus.Instance.AllDoors.Length; i++)
		{
			ShipStatus.Instance.AllDoors[i].Serialize(writer);
		}
		this.IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		int num = (int)reader.ReadByte();
		for (int i = 0; i < num; i++)
		{
			SystemTypes key = (SystemTypes)reader.ReadByte();
			float value = reader.ReadSingle();
			this.timers[key] = value;
		}
		for (int j = 0; j < ShipStatus.Instance.AllDoors.Length; j++)
		{
			ShipStatus.Instance.AllDoors[j].Deserialize(reader);
		}
	}

	public void CloseDoorsOfType(SystemTypes room)
	{
		this.timers[room] = 30f;
		for (int i = 0; i < ShipStatus.Instance.AllDoors.Length; i++)
		{
			PlainDoor plainDoor = ShipStatus.Instance.AllDoors[i];
			if (plainDoor.Room == room)
			{
				plainDoor.SetDoorway(false);
			}
		}
		this.IsDirty = true;
	}

	public virtual float GetTimer(SystemTypes room)
	{
		float result;
		if (this.timers.TryGetValue(room, out result))
		{
			return result;
		}
		return 0f;
	}
}
