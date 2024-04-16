using System;
using UnityEngine;

public class CreditsScreenPopUp : MonoBehaviour
{
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
}
