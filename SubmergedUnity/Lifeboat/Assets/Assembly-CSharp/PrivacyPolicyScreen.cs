using System;
using System.Collections;
using UnityEngine;

public class PrivacyPolicyScreen : MonoBehaviour
{
	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	public IEnumerator Show()
	{
		if (SaveManager.AcceptedPrivacyPolicy != 2)
		{
			base.gameObject.SetActive(true);
			ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton, this.BackButton);
			while (base.gameObject.activeSelf)
			{
				yield return null;
			}
		}
		yield break;
	}

	public void Close()
	{
		SaveManager.AcceptedPrivacyPolicy = 2;
		base.GetComponent<TransitionOpen>().Close();
	}
}
