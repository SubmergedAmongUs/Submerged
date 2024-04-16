using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class RegionTextMonitor : MonoBehaviour
{
	private void Start()
	{
		base.StartCoroutine(this.GetRegionText());
	}

	private IEnumerator GetRegionText()
	{
		while (DestroyableSingleton<ServerManager>.Instance.CurrentRegion == null)
		{
			yield return null;
		}
		base.GetComponent<TextMeshPro>().text = DestroyableSingleton<TranslationController>.Instance.GetStringWithDefault(DestroyableSingleton<ServerManager>.Instance.CurrentRegion.TranslateName, DestroyableSingleton<ServerManager>.Instance.CurrentRegion.Name, Array.Empty<object>());
		yield break;
	}
}
