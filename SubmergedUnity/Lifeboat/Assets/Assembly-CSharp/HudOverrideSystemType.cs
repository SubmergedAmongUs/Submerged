using System;
using Hazel;

internal class HudOverrideSystemType : ISystemType, IActivatable
{
	public const byte DamageBit = 128;

	public const byte TaskMask = 127;

	public bool IsActive { get; private set; }

	public bool IsDirty { get; private set; }

	public void Detoriorate(float deltaTime)
	{
		if (this.IsActive && !PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			PlayerControl.LocalPlayer.AddSystemTask(SystemTypes.Comms);
		}
	}

	public void RepairDamage(PlayerControl player, byte amount)
	{
		if ((amount & 128) > 0)
		{
			this.IsActive = true;
		}
		else
		{
			this.IsActive = false;
		}
		this.IsDirty = true;
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.Write(this.IsActive);
		this.IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		this.IsActive = reader.ReadBoolean();
	}
}
