using System;

public class AutoOpenDoor : PlainDoor
{
	private const float ClosedDuration = 10f;

	public const float CooldownDuration = 30f;

	public float ClosedTimer;

	public float CooldownTimer;

	public override void SetDoorway(bool open)
	{
		if (!open)
		{
			this.ClosedTimer = 10f;
			this.CooldownTimer = 30f;
		}
		base.SetDoorway(open);
	}

	public override bool DoUpdate(float dt)
	{
		this.CooldownTimer = Math.Max(this.CooldownTimer - dt, 0f);
		if (this.ClosedTimer > 0f)
		{
			this.ClosedTimer = Math.Max(this.ClosedTimer - dt, 0f);
			if (this.ClosedTimer == 0f)
			{
				this.SetDoorway(true);
				return true;
			}
		}
		return false;
	}
}
