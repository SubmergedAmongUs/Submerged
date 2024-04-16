using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using UnityEngine;

public class ElectricalDoors : MonoBehaviour, ISystemType
{
	public StaticDoor[] Doors;

	public ElectricalDoors.DoorSet[] Rooms;

	public ElectricalDoors.DoorSet LeftExits;

	public bool IsDirty { get; private set; }

	[ContextMenu("Shuffle Doors")]
	public void Initialize()
	{
		HashSet<ElectricalDoors.DoorSet> hashSet = new HashSet<ElectricalDoors.DoorSet>();
		StaticDoor[] doors = this.Doors;
		for (int i = 0; i < doors.Length; i++)
		{
			doors[i].SetOpen(false);
		}
		ElectricalDoors.DoorSet room = this.Rooms[0];
		int num = 0;
		while (hashSet.Count < this.Rooms.Length && num++ < 10000)
		{
			StaticDoor door = room.Doors.Random<StaticDoor>();
			ElectricalDoors.DoorSet doorSet = this.Rooms.First((ElectricalDoors.DoorSet r) => r != room && r.Doors.Contains(door));
			if (hashSet.Add(doorSet))
			{
				door.SetOpen(true);
			}
			if (BoolRange.Next(0.5f))
			{
				hashSet.Add(room);
				room = doorSet;
			}
		}
		bool flag = BoolRange.Next(0.5f);
		this.LeftExits.Doors[0].SetOpen(flag);
		this.LeftExits.Doors[1].SetOpen(!flag);
		this.IsDirty = true;
	}

	public void Detoriorate(float deltaTime)
	{
	}

	public void RepairDamage(PlayerControl player, byte amount)
	{
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		uint num = 0U;
		for (int i = 0; i < this.Doors.Length; i++)
		{
			num |= (this.Doors[i].IsOpen ? 1U : 0U) << i;
		}
		writer.Write(num);
		this.IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		uint num = reader.ReadUInt32();
		for (int i = 0; i < this.Doors.Length; i++)
		{
			this.Doors[i].SetOpen(((ulong)num & (ulong)(1L << (i & 31))) > 0UL);
		}
	}

	[Serializable]
	public class DoorSet
	{
		public string Name;

		public StaticDoor[] Doors;

		public override string ToString()
		{
			return this.Name;
		}
	}
}
