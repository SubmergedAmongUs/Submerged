using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartFansMinigame : Minigame
{
	public TextMeshPro ActionText;

	public SpriteRenderer[] CodeIcons;

	public Sprite[] IconSprites;

	public AudioClip revealSound;

	public AudioClip cycleSound;

	public AudioClip completeSound;

	public PassiveButton mainCodeButton;

	public PassiveButton closeButton;

	public List<UiElement> codeButtons;

	public ControllerButtonBehavior enterCodeHotkey;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		if (base.ConsoleId == 0)
		{
			this.ActionText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.RevealCode, Array.Empty<object>());
		}
		else
		{
			this.ActionText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.EnterCode, Array.Empty<object>());
		}
		byte[] data = this.MyNormTask.Data;
		for (int i = 0; i < this.CodeIcons.Length; i++)
		{
			SpriteRenderer target = this.CodeIcons[i];
			this.CodeIcons[i].transform.parent.gameObject.SetActive(false);
			if (base.ConsoleId == 0)
			{
				target.sprite = this.IconSprites[(int)data[i]];
			}
			else
			{
				this.CodeIcons[i].GetComponent<PassiveButton>().OnClick.AddListener(delegate()
				{
					this.RotateImage(target);
				});
			}
		}
		List<UiElement> list = new List<UiElement>(1);
		list.Add(this.mainCodeButton);
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.closeButton, this.mainCodeButton, list, false);
	}

	public override void Close()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.name);
		base.Close();
	}

	public void RevealCode()
	{
		for (int i = 0; i < this.CodeIcons.Length; i++)
		{
			this.CodeIcons[i].transform.parent.gameObject.SetActive(true);
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.revealSound, false, 1f);
		}
		if (this.MyNormTask.taskStep == 0)
		{
			this.MyNormTask.NextStep();
		}
		if (base.ConsoleId != 0)
		{
			ControllerManager.Instance.CloseOverlayMenu(base.name);
			ControllerManager.Instance.OpenOverlayMenu(base.name, this.closeButton, this.codeButtons[0], this.codeButtons, false);
		}
	}

	public void RotateImage(SpriteRenderer target)
	{
		if (base.ConsoleId == 0)
		{
			return;
		}
		if (this.amClosing != Minigame.CloseState.None)
		{
			return;
		}
		int num = this.IconSprites.IndexOf(target.sprite);
		num = (num + 1) % this.IconSprites.Length;
		target.sprite = this.IconSprites[num];
		for (int i = 0; i < this.CodeIcons.Length; i++)
		{
			SpriteRenderer spriteRenderer = this.CodeIcons[i];
			int num2 = this.IconSprites.IndexOf(spriteRenderer.sprite);
			if ((int)this.MyNormTask.Data[i] != num2)
			{
				if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySound(this.cycleSound, false, 1f);
				}
				return;
			}
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.completeSound, false, 1f);
		}
		this.MyNormTask.NextStep();
		base.StartCoroutine(base.CoStartClose(0.75f));
	}
}
