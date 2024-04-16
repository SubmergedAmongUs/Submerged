using System;
using System.Collections.Generic;
using UnityEngine;

public class NameTextBehaviour : MonoBehaviour
{
	public static readonly HashSet<char> SymbolChars = new HashSet<char>
	{
		'?',
		'!',
		',',
		'.',
		'\'',
		':',
		';',
		'(',
		')',
		'/',
		'\\',
		'%',
		'^',
		'&',
		'-',
		'=',
		'\r',
		'\n',
		'[',
		']'
	};

	public static NameTextBehaviour Instance;

	public TextBoxTMP nameSource;

	public void Start()
	{
		NameTextBehaviour.Instance = this;
		if (!NameTextBehaviour.IsValidName(SaveManager.PlayerName))
		{
			this.nameSource.SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.EnterName, Array.Empty<object>()), "");
			return;
		}
		this.nameSource.SetText(SaveManager.PlayerName, "");
	}

	public void UpdateName()
	{
		if (this.ShakeIfInvalid())
		{
			return;
		}
		SaveManager.PlayerName = this.nameSource.text;
	}

	public static bool IsValidName(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return false;
		}
		if (text.Equals(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.EnterName, Array.Empty<object>()), StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		foreach (char c in text)
		{
			if (c < ' ')
			{
				return false;
			}
			if (NameTextBehaviour.SymbolChars.Contains(c))
			{
				return false;
			}
		}
		return !(BlockedWords.CensorWords(text) != text);
	}

	public bool ShakeIfInvalid()
	{
		if (!NameTextBehaviour.IsValidName(this.nameSource.text))
		{
			base.StartCoroutine(Effects.SwayX(this.nameSource.transform, 0.75f, 0.25f));
			return true;
		}
		return false;
	}
}
