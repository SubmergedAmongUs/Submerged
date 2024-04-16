using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class PlatformTextTranslationTMP : MonoBehaviour, ITranslatedText
{
	public StringNames DefaultTargetText;

	public StringNames SwitchTargetText;

	public void ResetText()
	{
		TextMeshPro component = base.GetComponent<TextMeshPro>();
		component.text = DestroyableSingleton<TranslationController>.Instance.GetString(this.DefaultTargetText, Array.Empty<object>());
		component.ForceMeshUpdate(false, false);
	}

	public void Start()
	{
		DestroyableSingleton<TranslationController>.Instance.ActiveTexts.Add(this);
		this.ResetText();
	}

	public void OnDestroy()
	{
		if (DestroyableSingleton<TranslationController>.InstanceExists)
		{
			try
			{
				DestroyableSingleton<TranslationController>.Instance.ActiveTexts.Remove(this);
			}
			catch
			{
			}
		}
	}
}
