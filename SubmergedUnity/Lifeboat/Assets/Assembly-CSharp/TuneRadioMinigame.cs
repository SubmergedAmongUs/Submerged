using System;
using Rewired;
using UnityEngine;

public class TuneRadioMinigame : Minigame
{
	public RadioWaveBehaviour actualSignal;

	public DialBehaviour dial;

	public SpriteRenderer redLight;

	public SpriteRenderer greenLight;

	public float Tolerance = 0.1f;

	public float targetAngle;

	public bool finished;

	private float steadyTimer;

	public AudioClip StaticSound;

	public AudioClip RadioSound;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		this.targetAngle = this.dial.DialRange.Next();
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlayDynamicSound("CommsRadio", this.RadioSound, true, new DynamicSound.GetDynamicsFunction(this.GetRadioVolume), true);
			SoundManager.Instance.PlayDynamicSound("RadioStatic", this.StaticSound, true, new DynamicSound.GetDynamicsFunction(this.GetStaticVolume), true);
		}
		base.SetupInput(true);
	}

	private void GetRadioVolume(AudioSource player, float dt)
	{
		player.volume = 1f - this.actualSignal.NoiseLevel;
	}

	private void GetStaticVolume(AudioSource player, float dt)
	{
		player.volume = this.actualSignal.NoiseLevel;
	}

	public void Update()
	{
		if (this.finished)
		{
			return;
		}
		Vector2 axis2DRaw = ReInput.players.GetPlayer(0).GetAxis2DRaw(16, 17);
		if (axis2DRaw.sqrMagnitude > 0.9f)
		{
			Vector2 normalized = axis2DRaw.normalized;
			float value = Vector2.SignedAngle(Vector2.up, normalized);
			value = this.dial.DialRange.Clamp(value);
			this.dial.SetValue(value);
		}
		float num = Mathf.Abs((this.targetAngle - this.dial.Value) / this.dial.DialRange.Width) * 2f;
		this.actualSignal.NoiseLevel = Mathf.Clamp(Mathf.Sqrt(num), 0f, 1f);
		if (this.actualSignal.NoiseLevel <= this.Tolerance)
		{
			if (!this.dial.Engaged)
			{
				this.FinishGame();
				return;
			}
			this.redLight.color = new Color(Mathf.Lerp(1f, 0.35f, this.steadyTimer), 0f, 0f);
			this.steadyTimer += Time.deltaTime;
			if (this.steadyTimer > 1f)
			{
				this.FinishGame();
				return;
			}
		}
		else
		{
			this.redLight.color = new Color(1f, 0f, 0f);
			this.steadyTimer = 0f;
		}
	}

	private void FinishGame()
	{
		this.greenLight.color = Color.green;
		this.finished = true;
		this.dial.enabled = false;
		this.dial.SetValue(this.targetAngle);
		this.actualSignal.NoiseLevel = 0f;
		if (PlayerControl.LocalPlayer)
		{
			ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 0);
		}
		base.StartCoroutine(base.CoStartClose(0.75f));
		try
		{
			((SabotageTask)this.MyTask).MarkContributed();
		}
		catch
		{
		}
	}

	public override void Close()
	{
		SoundManager.Instance.StopSound(this.StaticSound);
		SoundManager.Instance.StopSound(this.RadioSound);
		base.Close();
	}
}
