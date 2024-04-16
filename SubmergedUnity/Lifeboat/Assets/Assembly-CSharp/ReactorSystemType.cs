using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;

public class ReactorSystemType : ISystemType, IActivatable, ICriticalSabotage
{
	private const float SyncRate = 2f;

	private float timer;

	public const byte StartCountdown = 128;

	public const byte AddUserOp = 64;

	public const byte RemoveUserOp = 32;

	public const byte ClearCountdown = 16;

	public const float CountdownStopped = 10000f;

	public readonly float ReactorDuration = 30f;

	public const byte ConsoleIdMask = 3;

	public const byte RequiredUserCount = 2;

	private HashSet<Tuple<byte, byte>> UserConsolePairs = new HashSet<Tuple<byte, byte>>();

	private SystemTypes system;

	public float Countdown { get; private set; } = 10000f;

	public bool IsDirty { get; private set; }

	public ReactorSystemType(float duration, SystemTypes system)
	{
		this.ReactorDuration = duration;
		this.system = system;
	}

	public int UserCount
	{
		get
		{
			int num = 0;
			int num2 = 0;
			foreach (Tuple<byte, byte> tuple in this.UserConsolePairs)
			{
				int num3 = 1 << (int)tuple.Item2;
				if ((num3 & num2) == 0)
				{
					num++;
					num2 |= num3;
				}
			}
			return num;
		}
	}

	public bool GetConsoleComplete(int consoleId)
	{
		return this.UserConsolePairs.Any((Tuple<byte, byte> kvp) => (int)kvp.Item2 == consoleId);
	}

	public void ClearSabotage()
	{
		this.Countdown = 10000f;
	}

	public bool IsActive
	{
		get
		{
			return this.Countdown < 10000f;
		}
	}

	public void RepairDamage(PlayerControl player, byte opCode)
	{
		int num = (int)(opCode & 3);
		if (opCode == 128 && !this.IsActive)
		{
			this.Countdown = this.ReactorDuration;
			this.UserConsolePairs.Clear();
		}
		else if (opCode == 16)
		{
			this.Countdown = 10000f;
		}
		else if (opCode.HasAnyBit(64))
		{
			this.UserConsolePairs.Add(new Tuple<byte, byte>(player.PlayerId, (byte)num));
			if (this.UserCount >= 2)
			{
				this.Countdown = 10000f;
			}
		}
		else if (opCode.HasAnyBit(32))
		{
			this.UserConsolePairs.Remove(new Tuple<byte, byte>(player.PlayerId, (byte)num));
		}
		this.IsDirty = true;
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
	}

	public void Detoriorate(float deltaTime)
	{
		if (this.IsActive)
		{
			if (!PlayerTask.PlayerHasTaskOfType<ReactorTask>(PlayerControl.LocalPlayer))
			{
				PlayerControl.LocalPlayer.AddSystemTask(this.system);
			}
			this.Countdown -= deltaTime;
			this.timer += deltaTime;
			if (this.timer > 2f)
			{
				this.timer = 0f;
				this.IsDirty = true;
				return;
			}
		}
		else
		{
			DestroyableSingleton<HudManager>.Instance.StopReactorFlash();
		}
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.Write(this.Countdown);
		writer.WritePacked(this.UserConsolePairs.Count);
		foreach (Tuple<byte, byte> tuple in this.UserConsolePairs)
		{
			writer.Write(tuple.Item1);
			writer.Write(tuple.Item2);
		}
		this.IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		this.Countdown = reader.ReadSingle();
		this.UserConsolePairs.Clear();
		int num = reader.ReadPackedInt32();
		for (int i = 0; i < num; i++)
		{
			this.UserConsolePairs.Add(new Tuple<byte, byte>(reader.ReadByte(), reader.ReadByte()));
		}
	}
}
