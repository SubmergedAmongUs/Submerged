using System;
using System.Collections.Generic;
using UnityEngine;

public class FreeplayPopover : MonoBehaviour
{
	public GameObject Content;

	public SpriteRenderer[] MapButtons;

	public HostGameButton HostGame;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	public void Show()
	{
		MainMenuManager mainMenuManager = UnityEngine.Object.FindObjectOfType<MainMenuManager>();
		if (mainMenuManager && mainMenuManager.googlePlayAssetHandler.RejectedDownload())
		{
			base.StartCoroutine(mainMenuManager.googlePlayAssetHandler.PromptUser());
			return;
		}
		int num = 0;
		for (int i = 0; i < this.MapButtons.Length; i++)
		{
			num++;
		}
		int num2 = 0;
		for (int j = 0; j < this.MapButtons.Length; j++)
		{
			SpriteRenderer spriteRenderer = this.MapButtons[j];
			if (spriteRenderer.gameObject.activeSelf)
			{
				Vector3 localPosition = spriteRenderer.transform.localPosition;
				localPosition.y = -0.65f * ((float)num2 - (float)(num - 1) / 2f) + 0.15f;
				spriteRenderer.transform.localPosition = localPosition;
				num2++;
			}
		}
		if (num == 1)
		{
			this.HostGame.OnClick();
			return;
		}
		this.Content.SetActive(true);
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton, this.DefaultButtonSelected, this.ControllerSelectable, false);
	}

	public void Close()
	{
		this.Content.SetActive(false);
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	public void PlayMap(int i)
	{
		AmongUsClient.Instance.TutorialMapId = i;
		this.HostGame.OnClick();
	}
}
