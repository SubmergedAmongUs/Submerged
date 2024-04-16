using System;

public class AutoCloseDoor : PlainDoor
{
	private const float OpenDuration = 10f;

	private float OpenTime;

	public override void SetDoorway(bool open)
	{
		if (open)
		{
			this.OpenTime = 10f;
		}
		base.SetDoorway(open);
	}

	public override bool DoUpdate(float dt)
	{
		if (this.OpenTime > 0f)
		{
			this.OpenTime = Math.Max(this.OpenTime - dt, 0f);
			if (this.OpenTime == 0f)
			{
				this.SetDoorway(false);
				return true;
			}
		}
		return false;
	}
}
