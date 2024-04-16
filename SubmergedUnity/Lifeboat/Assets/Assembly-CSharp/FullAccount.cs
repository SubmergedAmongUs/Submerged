using System;
using UnityEngine;

public class FullAccount : MonoBehaviour
{
	[SerializeField]
	private GameObject randomizeNameButton;

	[SerializeField]
	private GameObject editNameButton;

	public void CanSetCustomName(bool canSetName)
	{
		if (!canSetName)
		{
			DestroyableSingleton<AccountManager>.Instance.CheckAndRegenerateName();
		}
		this.randomizeNameButton.SetActive(!canSetName);
		this.editNameButton.SetActive(canSetName);
	}

	public void UpdateLoggedInAccountVisuals()
	{
		if (DestroyableSingleton<EOSManager>.Instance.IsMinor())
		{
			this.CanSetCustomName(DestroyableSingleton<AccountManager>.Instance.CanMinorSetCustomDisplayName());
			return;
		}
		this.CanSetCustomName(true);
	}
}
