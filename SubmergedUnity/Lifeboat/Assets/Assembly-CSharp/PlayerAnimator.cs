using System;
using System.Collections;
using PowerTools;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
	public float Speed = 2.5f;

	public VirtualJoystick joystick;

	public SpriteRenderer UseButton;

	public FingerBehaviour finger;

	public AnimationClip RunAnim;

	public AnimationClip IdleAnim;

	private Vector2 velocity;

	[HideInInspector]
	private SpriteAnim Animator;

	[HideInInspector]
	private SpriteRenderer rend;

	public int NearbyConsoles;

	private void Start()
	{
		this.Animator = base.GetComponent<SpriteAnim>();
		this.rend = base.GetComponent<SpriteRenderer>();
		this.rend.material.SetColor("_BackColor", Palette.ShadowColors[0]);
		this.rend.material.SetColor("_BodyColor", Palette.PlayerColors[0]);
		this.rend.material.SetColor("_VisorColor", Palette.VisorColor);
	}

	public void FixedUpdate()
	{
		base.transform.Translate(this.velocity * Time.fixedDeltaTime);
		this.UseButton.enabled = (this.NearbyConsoles > 0);
	}

	public void LateUpdate()
	{
		if (this.velocity.sqrMagnitude >= 0.1f)
		{
			if (this.Animator.GetCurrentAnimation() != this.RunAnim)
			{
				this.Animator.Play(this.RunAnim, 1f);
			}
			this.rend.flipX = (this.velocity.x < 0f);
			return;
		}
		if (this.Animator.GetCurrentAnimation() == this.RunAnim)
		{
			this.Animator.Play(this.IdleAnim, 1f);
		}
	}

	public IEnumerator WalkPlayerTo(Vector2 worldPos, bool relax, float tolerance = 0.01f)
	{
		worldPos.y += 0.3636f;
		if (!(this.joystick is DemoKeyboardStick))
		{
			this.finger.ClickOn();
		}
		for (;;)
		{
			Vector2 vector2;
			Vector2 vector = vector2 = worldPos - (Vector2) base.transform.position;
			if (vector2.sqrMagnitude <= tolerance)
			{
				break;
			}
			float num = Mathf.Clamp(vector.magnitude * 2f, 0.01f, 1f);
			this.velocity = vector.normalized * this.Speed * num;
			this.joystick.UpdateJoystick(this.finger, this.velocity, true);
			yield return null;
		}
		if (relax)
		{
			this.finger.ClickOff();
			this.velocity = Vector2.zero;
			this.joystick.UpdateJoystick(this.finger, this.velocity, false);
		}
		yield break;
	}
}
