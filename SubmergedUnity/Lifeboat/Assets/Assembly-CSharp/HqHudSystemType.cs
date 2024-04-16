using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;

internal class HqHudSystemType : ISystemType, IActivatable
{
	public const byte TagMask = 240;

	public const byte IdMask = 15;

	private HashSet<Tuple<byte, byte>> ActiveConsoles = new HashSet<Tuple<byte, byte>>();

	private HashSet<byte> CompletedConsoles = new HashSet<byte>();

	private const float ActiveTime = 10f;

	private float Timer;

	public int TargetNumber;

	public bool IsActive
	{
		get
		{
			return this.CompletedConsoles.Count < 2;
		}
	}

	public float NumComplete
	{
		get
		{
			return (float)this.CompletedConsoles.Count;
		}
	}

	public float PercentActive
	{
		get
		{
			return this.Timer / 10f;
		}
	}

	public bool IsDirty { get; private set; }

	public HqHudSystemType()
	{
		this.CompletedConsoles.Add(0);
		this.CompletedConsoles.Add(1);
	}

	public void Detoriorate(float deltaTime)
	{
		if (this.IsActive)
		{
			this.Timer -= deltaTime;
			if (this.Timer <= 0f)
			{
				this.TargetNumber = IntRange.Next(0, 99999);
				this.Timer = 10f;
				this.CompletedConsoles.Clear();
			}
			if (!PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
			{
				PlayerControl.LocalPlayer.AddSystemTask(SystemTypes.Comms);
			}
		}
	}

	internal bool IsConsoleActive(int consoleId)
	{
		return this.ActiveConsoles.Any((Tuple<byte, byte> s) => s.Item2 == (byte)consoleId);
	}

	internal bool IsConsoleOkay(int consoleId)
	{
		return this.CompletedConsoles.Contains((byte)consoleId);
	}

	public void RepairDamage(PlayerControl player, byte amount)
	{
		byte b = (byte) (amount & 15);
		HqHudSystemType.Tags tags = (HqHudSystemType.Tags)(amount & 240);
		if (tags <= HqHudSystemType.Tags.DeactiveBit)
		{
			if (tags != HqHudSystemType.Tags.FixBit)
			{
				if (tags == HqHudSystemType.Tags.DeactiveBit)
				{
					this.ActiveConsoles.Remove(new Tuple<byte, byte>(player.PlayerId, b));
				}
			}
			else
			{
				this.Timer = 10f;
				this.CompletedConsoles.Add(b);
			}
		}
		else if (tags != HqHudSystemType.Tags.ActiveBit)
		{
			if (tags == HqHudSystemType.Tags.DamageBit)
			{
				this.Timer = -1f;
				this.CompletedConsoles.Clear();
				this.ActiveConsoles.Clear();
			}
		}
		else
		{
			this.ActiveConsoles.Add(new Tuple<byte, byte>(player.PlayerId, b));
		}
		this.IsDirty = true;
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.WritePacked(this.ActiveConsoles.Count);
		foreach (Tuple<byte, byte> tuple in this.ActiveConsoles)
		{
			writer.Write(tuple.Item1);
			writer.Write(tuple.Item2);
		}
		writer.WritePacked(this.CompletedConsoles.Count);
		foreach (byte b in this.CompletedConsoles)
		{
			writer.Write(b);
		}
		this.IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		int num = reader.ReadPackedInt32();
		this.ActiveConsoles.Clear();
		for (int i = 0; i < num; i++)
		{
			this.ActiveConsoles.Add(new Tuple<byte, byte>(reader.ReadByte(), reader.ReadByte()));
		}
		int num2 = reader.ReadPackedInt32();
		this.CompletedConsoles.Clear();
		for (int j = 0; j < num2; j++)
		{
			this.CompletedConsoles.Add(reader.ReadByte());
		}
	}

	public enum Tags
	{
		DamageBit = 128,
		ActiveBit = 64,
		DeactiveBit = 32,
		FixBit = 16
	}
}
