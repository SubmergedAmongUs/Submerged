using System;
using TMPro;
using UnityEngine;

public class SettingsLanguageMenu : MonoBehaviour
{
	public TextMeshPro selectedLangText;

	public void Awake()
	{
		TranslatedImageSet[] languages = DestroyableSingleton<TranslationController>.Instance.Languages;
		for (int i = 0; i < languages.Length; i++)
		{
			if ((long)i == (long)((ulong)SaveManager.LastLanguage))
			{
				this.selectedLangText.text = languages[i].Name;
			}
		}
	}
}
