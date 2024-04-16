using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class EmailTextBehaviour : MonoBehaviour
{
	public static readonly HashSet<char> SymbolChars = new HashSet<char>
	{
		'?',
		'!',
		',',
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

	public TextBoxTMP nameSource;

	public string GetEmailValidEmail()
	{
		if (this.ShakeIfInvalid())
		{
			return "";
		}
		return this.nameSource.text;
	}

	private bool IsValidEmail(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return false;
		}
		if (!new Regex("^([+\\w\\.\\-]+)@([\\w\\-]+)((\\.(\\w){2,63})+)$").Match(text).Success)
		{
			return false;
		}
		foreach (char c in text)
		{
			if (c < ' ')
			{
				return false;
			}
			if (EmailTextBehaviour.SymbolChars.Contains(c))
			{
				return false;
			}
		}
		return true;
	}

	public bool ShakeIfInvalid()
	{
		if (!this.IsValidEmail(this.nameSource.text))
		{
			base.StartCoroutine(Effects.SwayX(this.nameSource.transform, 0.75f, 0.25f));
			return true;
		}
		return false;
	}

	public bool ShakeIfDoesntMatch(string email1, string email2)
	{
		if (email1 != email2)
		{
			base.StartCoroutine(Effects.SwayX(this.nameSource.transform, 0.75f, 0.25f));
			return true;
		}
		return false;
	}
}
