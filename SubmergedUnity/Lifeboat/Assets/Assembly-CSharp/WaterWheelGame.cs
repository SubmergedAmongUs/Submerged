using System;
using Rewired;
using UnityEngine;

public class WaterWheelGame : Minigame
{
	public SpriteRenderer Wheel;

	public VerticalSpriteGauge WaterLevel;

	public SpriteRenderer Watertop;

	public int WheelScale = 4;

	public AudioClip FillStart;

	public AudioClip FillLoop;

	public AudioClip WheelTurn;

	private TouchpadBehavior touchpad;

	private float Rate = 0.01f;

	private AudioSource fillSound;

	private Vector2 prevStickInput = Vector2.zero;

	private bool hadPrev;

	private bool grabbed;

	private float Water
	{
		get
		{
			return this.WaterLevel.Value;
		}
		set
		{
			float num = Mathf.Clamp(value, 0f, 1f);
			this.WaterLevel.Value = num;
			this.MyNormTask.Data[base.ConsoleId] = (byte)(num * 255f);
		}
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		this.WaterLevel.Value = (float)this.MyNormTask.Data[base.ConsoleId] / 255f;
		if (Constants.ShouldPlaySfx())
		{
			this.fillSound = SoundManager.Instance.PlaySound(this.FillStart, false, 1f);
		}
		this.touchpad = base.GetComponent<TouchpadBehavior>();
		base.SetupInput(true);
	}

	public void Update()
	{
		if (this.amClosing != Minigame.CloseState.None)
		{
			return;
		}
		this.Water += Time.deltaTime * this.Rate;
		if (this.fillSound && !this.fillSound.isPlaying)
		{
			this.fillSound = SoundManager.Instance.PlaySound(this.FillLoop, true, 1f);
		}
		if (this.fillSound)
		{
			this.fillSound.volume = Mathf.Lerp(0f, 1f, this.Rate * 5f);
			this.fillSound.pitch = Mathf.Lerp(0.75f, 1.25f, this.Water);
		}
		Vector2 vector = ReInput.players.GetPlayer(0).GetAxis2DRaw(13, 14);
		if (this.touchpad.IsTouching())
		{
			vector = this.touchpad.GetCenterToTouch().normalized;
		}
		if (vector.magnitude > 0.9f)
		{
			vector = vector.normalized;
			if (this.hadPrev)
			{
				float num = this.prevStickInput.AngleSigned(vector);
				if (num > 180f)
				{
					num -= 360f;
				}
				if (num < -180f)
				{
					num += 360f;
				}
				num /= (float)this.WheelScale;
				Vector3 localEulerAngles = this.Wheel.transform.localEulerAngles;
				float z = localEulerAngles.z;
				float num2 = Mathf.Clamp(localEulerAngles.z + num, 0.0001f, 358.99f);
				if (Mathf.Abs(localEulerAngles.z - num2) > 1f && Constants.ShouldPlaySfx())
				{
					AudioSource audioSource = SoundManager.Instance.PlaySound(this.WheelTurn, false, 1f);
					if (audioSource.timeSamples == 0)
					{
						audioSource.pitch = FloatRange.Next(0.9f, 1.1f);
					}
				}
				localEulerAngles.z = num2;
				this.Wheel.transform.localEulerAngles = localEulerAngles;
				this.Rate += num / (float)(360 * this.WheelScale);
				float num3 = Mathf.Abs(z - num2) * 0.01f;
				VibrationManager.Vibrate(num3, num3, 0.02f, VibrationManager.VibrationFalloff.None, null, false);
			}
			this.prevStickInput = vector;
			this.hadPrev = true;
		}
		else
		{
			this.hadPrev = false;
		}
		if (this.grabbed)
		{
			Controller controller = DestroyableSingleton<PassiveButtonManager>.Instance.controller;
			Vector2 vector2 = this.Wheel.transform.position;
			float num4 = (controller.DragStartPosition - vector2).AngleSigned(controller.DragPosition - vector2);
			if (num4 > 180f)
			{
				num4 -= 360f;
			}
			if (num4 < -180f)
			{
				num4 += 360f;
			}
			num4 /= (float)this.WheelScale;
			Vector3 localEulerAngles2 = this.Wheel.transform.localEulerAngles;
			float num5 = Mathf.Clamp(localEulerAngles2.z + num4, 0.0001f, 358.99f);
			if (Mathf.Abs(localEulerAngles2.z - num5) > 1f && Constants.ShouldPlaySfx())
			{
				AudioSource audioSource2 = SoundManager.Instance.PlaySound(this.WheelTurn, false, 1f);
				if (audioSource2.timeSamples == 0)
				{
					audioSource2.pitch = FloatRange.Next(0.9f, 1.1f);
				}
			}
			localEulerAngles2.z = num5;
			this.Wheel.transform.localEulerAngles = localEulerAngles2;
			this.Rate += num4 / (float)(360 * this.WheelScale);
			controller.ResetDragPosition();
		}
		else if (this.WaterLevel.Value >= 1f)
		{
			this.MyNormTask.NextStep();
			base.StartCoroutine(base.CoStartClose(0.75f));
		}
		Vector3 localPosition = this.Watertop.transform.localPosition;
		localPosition.y = this.WaterLevel.TopY;
		this.Watertop.transform.localPosition = localPosition;
	}

	public void Grab()
	{
		this.grabbed = !this.grabbed;
	}

	public override void Close()
	{
		if (this.fillSound)
		{
			this.fillSound.Stop();
		}
		base.Close();
	}
}
