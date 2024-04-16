using System;
using Hazel;

public class SwitchSystem : ISystemType, IActivatable
{
	public const byte MaxValue = 255;

	public const int NumSwitches = 5;

	public const byte DamageSystem = 128;

	public const byte SwitchesMask = 31;

	public float DetoriorationTime = 0.03f;

	public byte Value = byte.MaxValue;

	private float timer;

	public byte ExpectedSwitches;

	public byte ActualSwitches;

	public bool IsDirty { get; private set; }

	public float Level
	{
		get
		{
			return (float)this.Value / 255f;
		}
	}

	public bool IsActive
	{
		get
		{
			return this.ExpectedSwitches != this.ActualSwitches;
		}
	}

	public SwitchSystem()
	{
		Random random = new Random();
		this.ExpectedSwitches = (byte)(random.Next() & 31);
		this.ActualSwitches = this.ExpectedSwitches;
	}

	public void Detoriorate(float deltaTime)
	{
		this.timer += deltaTime;
		if (this.timer >= this.DetoriorationTime)
		{
			this.timer = 0f;
			if (this.ExpectedSwitches != this.ActualSwitches)
			{
				if (this.Value > 0)
				{
					this.Value = (byte)Math.Max((int)(this.Value - 3), 0);
				}
				if (!SwitchSystem.HasTask<ElectricTask>())
				{
					PlayerControl.LocalPlayer.AddSystemTask(SystemTypes.Electrical);
					return;
				}
			}
			else if (this.Value < 255)
			{
				this.Value = (byte)Math.Min((int)(this.Value + 3), 255);
			}
		}
	}

	public void RepairDamage(PlayerControl player, byte amount)
	{
		if (amount.HasBit(128))
		{
			this.ActualSwitches ^= (byte) (amount & 31);
		}
		else
		{
			this.ActualSwitches ^= (byte)(1 << (int)amount);
		}
		this.IsDirty = true;
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.Write(this.ExpectedSwitches);
		writer.Write(this.ActualSwitches);
		writer.Write(this.Value);
		this.IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		this.ExpectedSwitches = reader.ReadByte();
		this.ActualSwitches = reader.ReadByte();
		this.Value = reader.ReadByte();
	}

	protected static bool HasTask<T>()
	{
		for (int i = PlayerControl.LocalPlayer.myTasks.Count - 1; i > 0; i--)
		{
			if (PlayerControl.LocalPlayer.myTasks[i] is T)
			{
				return true;
			}
		}
		return false;
	}
}
