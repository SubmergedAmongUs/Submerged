using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerNavMenu : MonoBehaviour
{
	public bool openInOnEnable;

	public bool gridNavigation;

	public bool trySelectAny;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	private bool isOpen;

	private void Start()
	{
		if (this.openInOnEnable)
		{
			this.OpenMenu(false);
		}
	}

	private void OnEnable()
	{
		if (this.openInOnEnable)
		{
			this.OpenMenu(false);
		}
	}

	private IEnumerator OpenCoroutine()
	{
		yield return null;
		this.OpenMenu(false);
		yield break;
	}

	private void OnDisable()
	{
		base.StopAllCoroutines();
		this.CloseMenu();
	}

	public void OpenMenu(bool force = false)
	{
		if ((!this.isOpen || force) && ControllerManager.Instance)
		{
			UiElement uiElement = this.DefaultButtonSelected;
			if ((!uiElement || !uiElement.isActiveAndEnabled) && this.trySelectAny)
			{
				foreach (UiElement uiElement2 in this.ControllerSelectable)
				{
					if (uiElement2 && uiElement2.isActiveAndEnabled)
					{
						uiElement = uiElement2;
					}
				}
			}
			ControllerManager.Instance.OpenOverlayMenu(base.gameObject.name, this.BackButton, uiElement, this.ControllerSelectable, this.gridNavigation);
			this.isOpen = true;
		}
	}

	public void CloseMenu()
	{
		if (this.isOpen)
		{
			ControllerManager.Instance.CloseOverlayMenu(base.gameObject.name);
			this.isOpen = false;
		}
	}
}
