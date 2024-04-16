using System;
using TMPro;
using UnityEngine;

public class DialogueBox : MonoBehaviour
{
	public TextMeshPro target;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	private void OnEnable()
	{
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton);
	}

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	public void Show(string dialogue)
	{
		this.target.text = dialogue;
		if (Minigame.Instance)
		{
			Minigame.Instance.Close();
		}
		if (Minigame.Instance)
		{
			Minigame.Instance.Close();
		}
		PlayerControl.LocalPlayer.moveable = false;
		PlayerControl.LocalPlayer.NetTransform.Halt();
		base.gameObject.SetActive(true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(false);
		if (!PlayerControl.LocalPlayer.inVent)
		{
			PlayerControl.LocalPlayer.moveable = true;
		}
		Camera.main.GetComponent<FollowerCamera>().Locked = false;
	}
}
