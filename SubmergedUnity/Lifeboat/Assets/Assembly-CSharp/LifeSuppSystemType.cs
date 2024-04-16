using System;
using System.Collections.Generic;
using Hazel;

public class LifeSuppSystemType : ISystemType, IActivatable
{
	private const float SyncRate = 2f;

	private float timer;

	public const byte StartCountdown = 128;

	public const byte AddUserOp = 64;

	public const byte ClearCountdown = 16;

	public const float CountdownStopped = 10000f;

	public readonly float LifeSuppDuration = 45f;

	public const byte ConsoleIdMask = 3;

	public const byte RequiredUserCount = 2;

	public float Countdown = 10000f;

	private HashSet<int> CompletedConsoles = new HashSet<int>();

	public bool IsDirty { get; private set; }

	public LifeSuppSystemType(float duration)
	{
		this.LifeSuppDuration = duration;
	}

	public int UserCount
	{
		get
		{
			return this.CompletedConsoles.Count;
		}
	}

	public bool GetConsoleComplete(int consoleId)
	{
		return this.CompletedConsoles.Contains(consoleId);
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
		int item = (int)(opCode & 3);
		if (opCode == 128 && !this.IsActive)
		{
			this.Countdown = this.LifeSuppDuration;
			this.CompletedConsoles.Clear();
		}
		else if (opCode == 16)
		{
			this.Countdown = 10000f;
		}
		else if (opCode.HasAnyBit(64))
		{
			this.CompletedConsoles.Add(item);
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
			if (DestroyableSingleton<HudManager>.Instance.OxyFlash == null)
			{
				PlayerControl.LocalPlayer.AddSystemTask(SystemTypes.LifeSupp);
			}
			this.Countdown -= deltaTime;
			if (this.UserCount >= 2)
			{
				this.Countdown = 10000f;
				this.IsDirty = true;
				return;
			}
			this.timer += deltaTime;
			if (this.timer > 2f)
			{
				this.timer = 0f;
				this.IsDirty = true;
				return;
			}
		}
		else if (DestroyableSingleton<HudManager>.Instance.OxyFlash != null)
		{
			DestroyableSingleton<HudManager>.Instance.StopOxyFlash();
		}
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.Write(this.Countdown);
		writer.WritePacked(this.CompletedConsoles.Count);
		foreach (int num in this.CompletedConsoles)
		{
			writer.WritePacked(num);
		}
		this.IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		this.Countdown = reader.ReadSingle();
		if (reader.Position < reader.Length)
		{
			this.CompletedConsoles.Clear();
			int num = reader.ReadPackedInt32();
			for (int i = 0; i < num; i++)
			{
				this.CompletedConsoles.Add(reader.ReadPackedInt32());
			}
		}
	}
}
