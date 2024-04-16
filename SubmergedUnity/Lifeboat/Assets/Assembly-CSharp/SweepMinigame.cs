using System;
using System.Collections.Generic;
using UnityEngine;

public class SweepMinigame : Minigame
{
	public SpriteRenderer[] Spinners;

	public SpriteRenderer[] Shadows;

	public SpriteRenderer[] Lights;

	public HorizontalGauge[] Gauges;

	private int spinnerIdx;

	private float timer;

	public float SpinRate = 45f;

	private float initialTimer;

	public AudioClip SpinningSound;

	public AudioClip AcceptSound;

	public AudioClip RejectSound;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		this.ResetGauges();
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.SpinningSound, true, 1f);
		}
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton, this.DefaultButtonSelected, this.ControllerSelectable, true);
	}

	public override void Close()
	{
		SoundManager.Instance.StopSound(this.SpinningSound);
		base.Close();
	}

	public void FixedUpdate()
	{
		float num = Mathf.Clamp(2f - this.timer / 30f, 1f, 2f);
		this.timer += Time.fixedDeltaTime * num;
		if (this.spinnerIdx < this.Spinners.Length)
		{
			float num2 = this.CalcXPerc();
			this.Gauges[this.spinnerIdx].Value = ((num2 < 13f) ? 0.9f : 0.1f);
			Quaternion localRotation = Quaternion.Euler(0f, 0f, this.timer * this.SpinRate);
			this.Spinners[this.spinnerIdx].transform.localRotation = localRotation;
			this.Shadows[this.spinnerIdx].transform.localRotation = localRotation;
			this.Lights[this.spinnerIdx].enabled = (num2 < 13f);
		}
		for (int i = 0; i < this.Gauges.Length; i++)
		{
			HorizontalGauge horizontalGauge = this.Gauges[i];
			if (i < this.spinnerIdx)
			{
				horizontalGauge.Value = 0.95f;
			}
			if (i > this.spinnerIdx)
			{
				horizontalGauge.Value = 0.05f;
			}
			horizontalGauge.Value += (Mathf.PerlinNoise((float)i, Time.time * 51f) - 0.5f) * 0.025f;
		}
	}

	private float CalcXPerc()
	{
		int num = (int)(this.timer * this.SpinRate) % 360;
		return (float)Mathf.Min(360 - num, num);
	}

	public void HitButton(int i)
	{
		if (i != this.spinnerIdx)
		{
			return;
		}
		if (this.CalcXPerc() < 13f)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.AcceptSound, false, 1f);
			}
			this.Spinners[this.spinnerIdx].transform.localRotation = Quaternion.identity;
			this.Shadows[this.spinnerIdx].transform.localRotation = Quaternion.identity;
			this.spinnerIdx++;
			this.timer = this.initialTimer;
			if (this.spinnerIdx >= this.Gauges.Length)
			{
				this.MyNormTask.NextStep();
				base.StartCoroutine(base.CoStartClose(0.75f));
				return;
			}
		}
		else
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.RejectSound, false, 1f);
			}
			this.ResetGauges();
		}
	}

	private void ResetGauges()
	{
		this.spinnerIdx = 0;
		this.timer = FloatRange.Next(1f, 3f);
		this.initialTimer = this.timer;
		for (int i = 0; i < this.Gauges.Length; i++)
		{
			this.Lights[i].enabled = false;
			this.Spinners[i].transform.localRotation = Quaternion.Euler(0f, 0f, this.timer * this.SpinRate);
			this.Shadows[i].transform.localRotation = Quaternion.Euler(0f, 0f, this.timer * this.SpinRate);
		}
	}
}
