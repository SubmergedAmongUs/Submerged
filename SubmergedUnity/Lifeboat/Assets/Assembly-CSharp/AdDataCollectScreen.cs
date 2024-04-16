using System;
using System.Collections;
using UnityEngine;

public class AdDataCollectScreen : MonoBehaviour
{
	public void ForceShow()
	{
		SaveManager.ShowAdsScreen = ShowAdsState.NotAccepted;
		base.gameObject.SetActive(true);
	}

	public IEnumerator Show()
	{
		if (SaveManager.ShowAdsScreen == ShowAdsState.NotAccepted && !SaveManager.BoughtNoAds)
		{
			base.gameObject.SetActive(true);
			while (base.gameObject.activeSelf)
			{
				yield return null;
			}
		}
		yield break;
	}

	public void Update()
	{
		if (SaveManager.BoughtNoAds)
		{
			SaveManager.ShowAdsScreen = ShowAdsState.Purchased;
			this.Close();
		}
	}

	public void Close()
	{
		base.GetComponent<TransitionOpen>().Close();
	}

	public void SetPersonalized()
	{
		if (SaveManager.ShowAdsScreen != ShowAdsState.Purchased)
		{
			SaveManager.ShowAdsScreen = ShowAdsState.Personalized;
		}
		this.Close();
	}

	public void SetNonPersonalized()
	{
		if (SaveManager.ShowAdsScreen != ShowAdsState.Purchased)
		{
			SaveManager.ShowAdsScreen = ShowAdsState.NonPersonalized;
		}
		this.Close();
	}
}
