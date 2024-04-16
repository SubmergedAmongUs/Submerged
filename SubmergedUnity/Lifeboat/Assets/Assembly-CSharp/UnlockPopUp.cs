using System;
using System.Collections;
using UnityEngine;

public class UnlockPopUp : MonoBehaviour
{
	public IEnumerator Show()
	{
		DateTime utcNow = DateTime.UtcNow;
		if ((utcNow.DayOfYear < 350 && utcNow.DayOfYear > 4) || SaveManager.GetPurchase("hats_newyears2018"))
		{
			yield break;
		}
		base.gameObject.SetActive(true);
		SaveManager.SetPurchased("hats_newyears2018");
		while (base.isActiveAndEnabled)
		{
			yield return null;
		}
		yield break;
	}
}
