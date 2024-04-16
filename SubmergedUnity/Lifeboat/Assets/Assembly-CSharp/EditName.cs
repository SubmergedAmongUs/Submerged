using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditName : MonoBehaviour
{
	public NameTextBehaviour nameText;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultSelection;

	public List<UiElement> selectableObjects;

	private void OnEnable()
	{
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton, this.DefaultSelection, this.selectableObjects, false);
	}

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	public IEnumerator Show()
	{
		base.gameObject.SetActive(true);
		while (base.gameObject.activeSelf)
		{
			yield return null;
		}
		yield break;
	}

	public void Close()
	{
		base.GetComponent<TransitionOpen>().Close();
	}

	public void UpdateName()
	{
		if (!this.nameText.ShakeIfInvalid())
		{
			this.nameText.UpdateName();
			DestroyableSingleton<AccountManager>.Instance.UpdateAccountInfoDisplays();
			this.Close();
		}
	}

	public void RandomizeName()
	{
		this.nameText.nameSource.SetText(DestroyableSingleton<AccountManager>.Instance.GetRandomName(), "");
	}
}
