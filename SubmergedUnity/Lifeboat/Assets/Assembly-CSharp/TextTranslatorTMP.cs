using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(TextMeshPro))]
public class TextTranslatorTMP : MonoBehaviour, ITranslatedText
{
	public StringNames TargetText;

	public string defaultStr;

	public bool ToUpper;

	public bool ResetOnlyWhenNoDefault;

	public UnityEvent OnTranslate;

	public void ResetText()
	{
		if (this.ResetOnlyWhenNoDefault && (this.defaultStr != null || this.defaultStr != ""))
		{
			return;
		}
		TextMeshPro component = base.GetComponent<TextMeshPro>();
		string text = DestroyableSingleton<TranslationController>.Instance.GetStringWithDefault(this.TargetText, this.defaultStr, Array.Empty<object>());
		if (this.ToUpper)
		{
			text = text.ToUpperInvariant();
		}
		component.text = text;
		component.ForceMeshUpdate(false, false);
		if (this.OnTranslate != null)
		{
			this.OnTranslate.Invoke();
		}
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
