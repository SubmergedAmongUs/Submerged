using System;
using UnityEngine;

public class DemoKeyboardStick : VirtualJoystick
{
	public SpriteRenderer UpKey;

	public SpriteRenderer DownKey;

	public SpriteRenderer LeftKey;

	public SpriteRenderer RightKey;

	protected override void FixedUpdate()
	{
	}

	public override void UpdateJoystick(FingerBehaviour finger, Vector2 velocity, bool syncFinger)
	{
		this.UpKey.enabled = (velocity.y > 0.1f);
		this.DownKey.enabled = (velocity.y < -0.1f);
		this.RightKey.enabled = (velocity.x > 0.1f);
		this.LeftKey.enabled = (velocity.x < -0.1f);
	}
}
