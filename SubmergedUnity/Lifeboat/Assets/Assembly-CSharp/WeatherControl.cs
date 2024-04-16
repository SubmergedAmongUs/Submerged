using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class WeatherControl : MonoBehaviour
{
	public Sprite backgroundLight;

	public Sprite backgroundDark;

	public Sprite lightOff;

	public Sprite lightOn;

	public SpriteRenderer Background;

	public SpriteRenderer Switch;

	public SpriteRenderer Light;

	public TextMeshPro Label;

	internal void SetInactive()
	{
		this.Light.sprite = this.lightOff;
		this.Label.color = new Color32(146, 135, 163, byte.MaxValue);
		base.StartCoroutine(this.Run());
		this.Switch.flipX = true;
	}

	public void SetActive()
	{
		base.StopAllCoroutines();
		this.Label.color = new Color32(81, 53, 115, byte.MaxValue);
		this.Background.sprite = this.backgroundLight;
		this.Light.sprite = this.lightOn;
		this.Switch.flipX = false;
	}

	private IEnumerator Run()
	{
		for (;;)
		{
			this.Background.sprite = this.backgroundDark;
			yield return Effects.Wait(0.5f);
			this.Background.sprite = this.backgroundLight;
			yield return Effects.Wait(0.5f);
		}
	}
}
