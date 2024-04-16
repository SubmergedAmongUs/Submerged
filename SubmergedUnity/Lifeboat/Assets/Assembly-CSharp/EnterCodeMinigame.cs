using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnterCodeMinigame : Minigame
{
	public TextMeshPro NumberText;

	public TextMeshPro TargetText;

	public SpriteRenderer Card;

	public int number;

	public string numString = string.Empty;

	private bool animating;

	private bool cardOut;

	private bool done;

	private int targetNumber;

	public AudioClip WalletOut;

	public AudioClip NumberSound;

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

	public void ShowCard()
	{
		base.StartCoroutine(this.CoShowCard());
	}

	private IEnumerator CoShowCard()
	{
		if (this.cardOut)
		{
			yield break;
		}
		this.cardOut = true;
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.WalletOut, false, 1f);
		}
		Vector3 pos = this.Card.transform.localPosition;
		Vector3 targ = new Vector3(pos.x, 0.84f, pos.z);
		float time = 0f;
		for (;;)
		{
			float num = Mathf.Min(1f, time / 0.6f);
			this.Card.transform.localPosition = Vector3.Lerp(pos, targ, num);
			this.Card.transform.localScale = Vector3.Lerp(Vector3.one * 0.75f, Vector3.one, num);
			if (time > 0.6f)
			{
				break;
			}
			yield return null;
			time += Time.deltaTime;
		}
		yield break;
	}

	public void EnterDigit(int i)
	{
		if (this.animating)
		{
			return;
		}
		if (this.done)
		{
			return;
		}
		if (this.NumberText.text.Length >= 5)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.RejectSound, false, 1f);
			}
			return;
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.NumberSound, false, 1f).pitch = Mathf.Lerp(0.8f, 1.2f, (float)i / 9f);
		}
		this.numString += i.ToString();
		this.number = this.number * 10 + i;
		this.NumberText.text = this.numString;
	}

	public void ClearDigits()
	{
		if (this.animating)
		{
			return;
		}
		this.number = 0;
		this.numString = string.Empty;
		this.NumberText.text = string.Empty;
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.NumberSound, false, 1f);
		}
	}

	public void AcceptDigits()
	{
		if (this.animating)
		{
			return;
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.NumberSound, false, 1f);
		}
		base.StartCoroutine(this.Animate());
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		this.targetNumber = BitConverter.ToInt32(this.MyNormTask.Data, 0);
		this.NumberText.text = string.Empty;
		this.TargetText.text = this.targetNumber.ToString("D5");
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton, this.DefaultButtonSelected, this.ControllerSelectable, false);
	}

	private IEnumerator Animate()
	{
		this.animating = true;
		WaitForSeconds wait = new WaitForSeconds(0.1f);
		yield return wait;
		this.NumberText.text = string.Empty;
		yield return wait;
		if (this.targetNumber == this.number)
		{
			this.done = true;
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.AcceptSound, false, 1f);
			}
			this.MyNormTask.NextStep();
			string okStr = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.OK, Array.Empty<object>());
			this.NumberText.text = okStr;
			yield return wait;
			this.NumberText.text = string.Empty;
			yield return wait;
			this.NumberText.text = okStr;
			yield return wait;
			this.NumberText.text = string.Empty;
			yield return base.CoStartClose(0.5f);
			okStr = null;
		}
		else
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.RejectSound, false, 1f);
			}
			string okStr = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Bad, Array.Empty<object>());
			this.NumberText.text = okStr;
			yield return wait;
			this.NumberText.text = string.Empty;
			yield return wait;
			this.NumberText.text = okStr;
			yield return wait;
			this.numString = string.Empty;
			this.number = 0;
			this.NumberText.text = this.numString;
			okStr = null;
		}
		this.animating = false;
		yield break;
	}
}
