using System;
using UnityEngine;

public class PermissionsRequest : MonoBehaviour
{
	public EmailTextBehaviour emailText;

	public EmailTextBehaviour emailConfirmText;

	public void Show()
	{
		base.gameObject.SetActive(true);
	}

	public void Close()
	{
		base.GetComponent<TransitionOpen>().Close();
	}

	public void ContinueWithout()
	{
		DestroyableSingleton<EOSManager>.Instance.LogInToDeviceIDForGuestMode();
		this.Close();
	}

	public void SendEmail()
	{
		if (!this.emailText.ShakeIfInvalid() && !this.emailConfirmText.ShakeIfInvalid() && !this.emailConfirmText.ShakeIfDoesntMatch(this.emailText.nameSource.text, this.emailConfirmText.nameSource.text))
		{
			SaveManager.GuardianEmail = this.emailText.GetEmailValidEmail();
			DestroyableSingleton<AccountManager>.Instance.UpdateGuardianEmailDisplay();
			DestroyableSingleton<EOSManager>.Instance.CreateKWSUer();
			this.Close();
		}
	}
}
