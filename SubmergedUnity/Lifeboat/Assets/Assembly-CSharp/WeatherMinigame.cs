using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class WeatherMinigame : Minigame
{
	public float Duration = 5f;

	public HorizontalGauge destGauge1;

	public HorizontalGauge destGauge2;

	public HorizontalGauge destGauge3;

	public PassiveButton StartButton;

	public TextMeshPro EtaText;

	public AudioClip StartSound;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton, this.DefaultButtonSelected);
	}

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	public void StartStopFill()
	{
		this.StartButton.enabled = false;
		base.StartCoroutine(this.CoDoAnimation());
	}

	private IEnumerator CoDoAnimation()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.StartSound, false, 1f);
		}
		yield return Effects.ScaleIn(this.StartButton.transform, 1f, 0f, 0.15f);
		this.EtaText.gameObject.SetActive(true);
		yield return Effects.ScaleIn(this.EtaText.transform, 0f, 1f, 0.15f);
		for (float timer = 0f; timer < this.Duration; timer += Time.deltaTime)
		{
			int num = Mathf.CeilToInt(this.Duration - timer);
			this.EtaText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.WeatherEta, new object[]
			{
				num
			});
			this.destGauge1.Value = Mathf.Lerp(0f, 1f, timer / this.Duration * 5f);
			this.destGauge2.Value = Mathf.Lerp(0f, 1f, timer / this.Duration * 3f);
			this.destGauge3.Value = Mathf.Lerp(0f, 1f, timer / this.Duration);
			yield return null;
		}
		this.EtaText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.WeatherComplete, Array.Empty<object>());
		this.MyNormTask.NextStep();
		yield return base.CoStartClose(0.75f);
		yield break;
	}

	public override void Close()
	{
		SoundManager.Instance.StopSound(this.StartSound);
		base.Close();
	}
}
