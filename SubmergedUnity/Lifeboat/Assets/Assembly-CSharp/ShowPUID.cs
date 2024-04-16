using System;
using TMPro;
using UnityEngine;

public class ShowPUID : MonoBehaviour
{
	public int howManyClicks = 5;

	public TextMeshPro tmp;

	public void Awake()
	{
		this.howManyClicks = 5;
		this.ShowPUIDCountdown();
	}

	public void OnDisable()
	{
		this.howManyClicks = 5;
		this.ShowPUIDCountdown();
	}

	public void ShowPUIDCountdown()
	{
		string text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ShowAccountSupportID5, Array.Empty<object>());
		if (this.howManyClicks == 4)
		{
			text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ShowAccountSupportID4, Array.Empty<object>());
		}
		else if (this.howManyClicks == 3)
		{
			text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ShowAccountSupportID3, Array.Empty<object>());
		}
		else if (this.howManyClicks == 2)
		{
			text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ShowAccountSupportID2, Array.Empty<object>());
		}
		else if (this.howManyClicks == 1)
		{
			text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ShowAccountSupportID1, Array.Empty<object>());
		}
		else if (this.howManyClicks == 0)
		{
			if (SaveManager.AccountLoginStatus == EOSManager.AccountLoginStatus.Offline)
			{
				text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.YouAreNotOnline, Array.Empty<object>());
			}
			else
			{
				text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SupportIDLabel, new object[]
				{
					DestroyableSingleton<EOSManager>.Instance.ProductUserId
				});
				if (SaveManager.AccountLoginStatus == EOSManager.AccountLoginStatus.Guest)
				{
					text += "-G";
				}
				else if (SaveManager.AccountLoginStatus == EOSManager.AccountLoginStatus.WaitingForParent)
				{
					text += "-W";
				}
				if (DestroyableSingleton<EOSManager>.Instance.IsMinor())
				{
					text += "-M";
				}
				else
				{
					text += "-A";
				}
			}
		}
		this.tmp.text = text;
		this.howManyClicks--;
		if (this.howManyClicks < 0)
		{
			this.howManyClicks = 5;
		}
	}
}
