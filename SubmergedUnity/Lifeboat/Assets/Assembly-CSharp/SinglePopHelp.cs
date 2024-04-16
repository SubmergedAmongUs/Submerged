using System;
using System.Collections.Generic;
using UnityEngine;

public class SinglePopHelp : MonoBehaviour
{
	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	public void OnEnable()
	{
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton, this.DefaultButtonSelected, this.ControllerSelectable, false);
		ControllerManager.Instance.CurrentUiState.CurrentSelection = this.DefaultButtonSelected;
	}

	public void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}
}
