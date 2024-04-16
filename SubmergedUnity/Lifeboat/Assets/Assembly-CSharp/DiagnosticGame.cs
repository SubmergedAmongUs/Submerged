using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DiagnosticGame : Minigame
{
	public VerticalGauge Gauge;

	public SpriteRenderer StartButton;

	public float TimePerStep = 90f;

	public TextMeshPro Text;

	private int TargetNum = -1;

	public SpriteRenderer[] Targets;

	private Color goodBarColor = new Color32(100, 193, byte.MaxValue, byte.MaxValue);

	public AudioClip StartSound;

	public AudioClip CorrectSound;

	public AudioClip TickSound;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	private int lastPercent;

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	public override void Begin(PlayerTask task)
	{
		this.Text.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.BeginDiagnostics, Array.Empty<object>());
		base.Begin(task);
		if (this.MyNormTask.TimerStarted == NormalPlayerTask.TimerState.NotStarted)
		{
			base.StartCoroutine(this.BlinkButton());
		}
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton, this.DefaultButtonSelected, this.ControllerSelectable, false);
	}

	private IEnumerator BlinkButton()
	{
		for (;;)
		{
			this.StartButton.color = Color.red;
			yield return Effects.Wait(0.5f);
			this.StartButton.color = Color.white;
			yield return Effects.Wait(0.5f);
		}
		yield break;
	}

	public void PickAnomaly(int num)
	{
		if (this.amClosing != Minigame.CloseState.None)
		{
			return;
		}
		if (this.MyNormTask.TimerStarted != NormalPlayerTask.TimerState.Finished)
		{
			return;
		}
		if (num == this.TargetNum)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.CorrectSound, false, 1f);
			}
			this.Targets[this.TargetNum].color = this.goodBarColor;
			this.MyNormTask.NextStep();
			base.StartCoroutine(base.CoStartClose(0.75f));
		}
	}

	public void StartDiagnostic()
	{
		if (this.MyNormTask.TimerStarted != NormalPlayerTask.TimerState.NotStarted)
		{
			return;
		}
		this.StartButton.GetComponent<PassiveButton>().enabled = false;
		base.StopAllCoroutines();
		this.StartButton.color = Color.white;
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.StartSound, false, 1f);
		}
		this.MyNormTask.TaskTimer = this.TimePerStep;
		this.MyNormTask.TimerStarted = NormalPlayerTask.TimerState.Started;
	}

	public void Update()
	{
		switch (this.MyNormTask.TimerStarted)
		{
		case NormalPlayerTask.TimerState.NotStarted:
			this.Gauge.gameObject.SetActive(false);
			this.Targets.ForEach(delegate(SpriteRenderer f)
			{
				f.gameObject.SetActive(false);
			});
			return;
		case NormalPlayerTask.TimerState.Started:
		{
			this.Gauge.gameObject.SetActive(true);
			this.Gauge.MaxValue = this.TimePerStep;
			this.Gauge.value = this.MyNormTask.TaskTimer;
			int num = (int)(100f * this.MyNormTask.TaskTimer / this.TimePerStep);
			if (num != this.lastPercent && Constants.ShouldPlaySfx())
			{
				this.lastPercent = num;
				SoundManager.Instance.PlaySound(this.TickSound, false, 0.8f);
			}
			this.Text.text = num.ToString() + "%";
			this.Targets.ForEach(delegate(SpriteRenderer f)
			{
				f.gameObject.SetActive(false);
			});
			return;
		}
		case NormalPlayerTask.TimerState.Finished:
			this.Gauge.gameObject.SetActive(true);
			this.Gauge.MaxValue = 1f;
			this.Gauge.value = 1f;
			if (this.TargetNum == -1)
			{
				this.Text.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.PickAnomaly, Array.Empty<object>());
				this.Targets.ForEach(delegate(SpriteRenderer f)
				{
					f.gameObject.SetActive(true);
					f.color = this.goodBarColor;
				});
				this.TargetNum = this.Targets.RandomIdx<SpriteRenderer>();
				this.Targets[this.TargetNum].color = Color.red;
			}
			return;
		default:
			return;
		}
	}
}
