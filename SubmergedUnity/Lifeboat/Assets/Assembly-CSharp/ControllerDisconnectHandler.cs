using System;
using UnityEngine;

public class ControllerDisconnectHandler : MonoBehaviour
{
	private bool isConnected = true;

	public GameObject ContinueBackground;

	public GameObject ContinueText;

	public GameObject obj;

	[Header("Console Controller Navigation")]
	public UiElement ContinueButton;

	private void Update()
	{
	}

	public void Close()
	{
		Debug.LogError("Controller reconnected");
		this.isConnected = true;
		if (this.ContinueBackground)
		{
			this.ContinueBackground.SetActive(false);
		}
		if (this.ContinueText)
		{
			this.ContinueText.SetActive(false);
		}
		if (this.obj)
		{
			this.obj.SetActive(false);
		}
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	private void OnStateChange(uint index, bool newIsConnected)
	{
		if (!newIsConnected && this.isConnected)
		{
			Debug.LogError("Controller disconnected");
			this.isConnected = false;
			if (this.ContinueBackground)
			{
				this.ContinueBackground.SetActive(true);
			}
			if (this.ContinueText)
			{
				this.ContinueText.SetActive(true);
			}
			if (this.obj)
			{
				this.obj.SetActive(true);
			}
			ControllerManager.Instance.OpenOverlayMenu(base.name, this.ContinueButton, this.ContinueButton);
		}
	}
}
