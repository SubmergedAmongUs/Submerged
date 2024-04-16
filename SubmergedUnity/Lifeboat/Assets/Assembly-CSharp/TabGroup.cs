using System;
using UnityEngine;

public class TabGroup : MonoBehaviour
{
	public SpriteRenderer Button;

	public ButtonRolloverHandler Rollover;

	public GameObject Content;

	internal void Close()
	{
		Color color = new Color(0.3f, 0.3f, 0.3f);
		this.Button.color = color;
		this.Rollover.OutColor = color;
		this.Content.SetActive(false);
	}

	internal void Open()
	{
		if (ActiveInputManager.currentControlType != ActiveInputManager.InputType.Joystick && ControllerManager.Instance.CurrentUiState.CurrentSelection != null && ControllerManager.Instance.CurrentUiState.CurrentSelection.gameObject == this.Rollover.gameObject)
		{
			this.Button.color = Color.white;
		}
		this.Rollover.OutColor = Color.white;
		this.Content.SetActive(true);
	}
}
