using System;
using TMPro;
using UnityEngine;

public class ShowerMinigame : Minigame
{
	public VerticalGauge Gauge;

	public TextMeshPro PercentText;

	private float timer;

	public float MaxTime = 12f;

	public AudioClip washSound;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		this.timer = this.MaxTime * (float)this.MyNormTask.Data[0] / 100f;
		this.PercentText.text = ((int)(100 - this.MyNormTask.Data[0])).ToString() + "%";
		AirshipStatus airshipStatus = ShipStatus.Instance as AirshipStatus;
		if (airshipStatus && airshipStatus.ShowerParticles)
		{
			airshipStatus.ShowerParticles.Play();
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.washSound, true, 1f);
		}
		base.SetupInput(true);
		VibrationManager.Vibrate(2f, 2f, 0f, VibrationManager.VibrationFalloff.None, this.washSound, true);
	}

	public void Update()
	{
		if (this.amClosing != Minigame.CloseState.None)
		{
			return;
		}
		this.timer += Time.deltaTime;
		this.MyNormTask.Data[0] = (byte)(this.timer / this.MaxTime * 100f);
		this.Gauge.value = 1f - this.timer / this.MaxTime;
		this.PercentText.text = ((int)(100 - this.MyNormTask.Data[0])).ToString() + "%";
		if (this.MyNormTask.Data[0] >= 100)
		{
			this.MyNormTask.NextStep();
			base.StartCoroutine(base.CoStartClose(0.5f));
		}
	}

	public override void Close()
	{
		SoundManager.Instance.StopSound(this.washSound);
		AirshipStatus airshipStatus = ShipStatus.Instance as AirshipStatus;
		if (airshipStatus != null)
		{
			airshipStatus.ShowerParticles.Stop();
		}
		VibrationManager.CancelVibration(this.washSound);
		base.Close();
	}
}
