using System;
using System.Collections;
using Rewired;
using UnityEngine;

public class ToiletMinigame : Minigame
{
	public SpriteRenderer Needle;

	public SpriteRenderer Pipes;

	public const float StickDown = -0.75f;

	public FloatRange StickRange = new FloatRange(-0.85f, -0.4f);

	public Collider2D Stick;

	public SpriteRenderer Plunger;

	public Sprite PlungerUp;

	public Sprite PlungerDown;

	private float pressure;

	public Controller controller = new Controller();

	public float lastY;

	public float plungeScale = 0.5f;

	public AudioClip flushSound;

	public AudioClip[] plungeSounds;

	private AudioSource plungerSource;

	private float controllerStickPos = 1f;

	private const float controllerPlungeSpeed = 30f;

	public override void Begin(PlayerTask task)
	{
		this.plungerSource = SoundManager.Instance.GetNamedAudioSource("plungerSource");
		base.Begin(task);
		base.SetupInput(true);
	}

	public void Update()
	{
		if (this.amClosing != Minigame.CloseState.None || this.pressure >= 1f)
		{
			return;
		}
		this.pressure -= Time.deltaTime * this.plungeScale / 2f;
		if (this.pressure < 0f)
		{
			this.pressure = 0f;
		}
		this.controller.Update();
		Vector3 localPosition = this.Stick.transform.localPosition;
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			float axisRaw = ReInput.players.GetPlayer(0).GetAxisRaw(14);
			if (axisRaw < 0f)
			{
				if (this.controllerStickPos > 1f + axisRaw)
				{
					this.controllerStickPos = Mathf.Lerp(this.controllerStickPos, 1f + axisRaw, Time.deltaTime * 30f);
				}
			}
			else if (axisRaw > 0f)
			{
				if (this.controllerStickPos < axisRaw)
				{
					this.controllerStickPos = Mathf.Lerp(this.controllerStickPos, axisRaw, Time.deltaTime * 30f);
				}
			}
			else
			{
				this.controllerStickPos = Mathf.Lerp(this.controllerStickPos, 1f, Time.deltaTime);
			}
			localPosition.y = this.StickRange.Lerp(this.controllerStickPos);
			this.Stick.transform.localPosition = localPosition;
			if (this.lastY > localPosition.y)
			{
				this.pressure += (this.lastY - localPosition.y) * this.plungeScale;
				if (Constants.ShouldPlaySfx() && !this.plungerSource.isPlaying)
				{
					this.plungerSource.clip = this.plungeSounds.Random<AudioClip>();
					this.plungerSource.Play();
					VibrationManager.Vibrate(0.3f, 0.3f, 0.2f, VibrationManager.VibrationFalloff.Linear, null, false);
				}
			}
			this.lastY = localPosition.y;
		}
		else
		{
			switch (this.controller.CheckDrag(this.Stick))
			{
			case DragState.TouchStart:
				this.lastY = localPosition.y;
				break;
			case DragState.Dragging:
				localPosition.y = this.StickRange.Clamp(this.StickRange.max + (this.controller.DragPosition.y - this.controller.DragStartPosition.y));
				this.Stick.transform.localPosition = localPosition;
				if (this.lastY > localPosition.y)
				{
					this.pressure += (this.lastY - localPosition.y) * this.plungeScale;
					if (Constants.ShouldPlaySfx() && !this.plungerSource.isPlaying)
					{
						this.plungerSource.clip = this.plungeSounds.Random<AudioClip>();
						this.plungerSource.Play();
					}
				}
				this.lastY = localPosition.y;
				break;
			case DragState.Released:
				localPosition.y = Mathf.Lerp(localPosition.y, this.StickRange.max, Time.deltaTime);
				this.Stick.transform.localPosition = localPosition;
				break;
			}
		}
		this.Plunger.sprite = ((localPosition.y < -0.75f) ? this.PlungerDown : this.PlungerUp);
		this.Needle.transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(0f, -230f, this.pressure));
		if (this.pressure >= 1f)
		{
			base.StartCoroutine(this.Finish());
		}
	}

	private IEnumerator Finish()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.flushSound, false, 1f);
		}
		VibrationManager.Vibrate(2.5f, 2.5f, 0f, VibrationManager.VibrationFalloff.None, this.flushSound, false);
		this.MyNormTask.NextStep();
		yield return Effects.All(new IEnumerator[]
		{
			Effects.Shake(this.Pipes.transform, 0.65f, 0.05f, true),
			Effects.Rotate2D(this.Needle.transform, 230f, 0f, 0.6f)
		});
		this.Close();
		yield break;
	}

	public override void Close()
	{
		SoundManager.Instance.StopNamedSound("plungerSource");
		base.Close();
	}
}
