using System;
using TMPro;
using UnityEngine;

public class GenericPopup : MonoBehaviour
{
	public TextMeshPro TextAreaTMP;

	public bool destroyOnClose;

	public void Show(string text = "")
	{
		if (this.TextAreaTMP)
		{
			this.TextAreaTMP.text = text;
		}
		base.gameObject.SetActive(true);
	}

	public void Close()
	{
		if (this.destroyOnClose)
		{
			 UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		base.gameObject.SetActive(false);
	}
}
