using System;
using System.Collections;
using UnityEngine;

public class UpdateGuardianEmail : MonoBehaviour
{
	public EmailTextBehaviour emailText;

	public EmailTextBehaviour emailConfirmText;

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

	public void SendUpdateGuardianEmail()
	{
		if (!this.emailText.ShakeIfInvalid() && !this.emailConfirmText.ShakeIfInvalid() && !this.emailConfirmText.ShakeIfDoesntMatch(this.emailText.nameSource.text, this.emailConfirmText.nameSource.text))
		{
			SaveManager.GuardianEmail = this.emailText.GetEmailValidEmail();
			DestroyableSingleton<EOSManager>.Instance.UpdateGuardianEmail();
			DestroyableSingleton<AccountManager>.Instance.ShowGuardianEmailSentConfirm();
			this.Close();
		}
	}
}
