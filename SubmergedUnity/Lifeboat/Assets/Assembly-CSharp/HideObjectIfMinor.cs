using System;
using UnityEngine;

public class HideObjectIfMinor : MonoBehaviour
{
	public bool hideIfGuest = true;

	private void Awake()
	{
		AccountManager instance = DestroyableSingleton<AccountManager>.Instance;
		instance.OnLoggedInStatusChange = (Action)Delegate.Combine(instance.OnLoggedInStatusChange, new Action(this.UpdateVisual));
	}

	private void OnDestroy()
	{
		AccountManager instance = DestroyableSingleton<AccountManager>.Instance;
		instance.OnLoggedInStatusChange = (Action)Delegate.Remove(instance.OnLoggedInStatusChange, new Action(this.UpdateVisual));
	}

	private void Start()
	{
		this.UpdateVisual();
	}

	private void OnEnable()
	{
		this.UpdateVisual();
	}

	private bool IsGuestAndHide()
	{
		return this.hideIfGuest && SaveManager.AccountLoginStatus == EOSManager.AccountLoginStatus.Guest;
	}

	public void UpdateVisual()
	{
		if (DestroyableSingleton<EOSManager>.Instance.IsMinor() || this.IsGuestAndHide() || SaveManager.AccountLoginStatus == EOSManager.AccountLoginStatus.Offline)
		{
			base.gameObject.SetActive(false);
			return;
		}
		base.gameObject.SetActive(true);
	}
}
