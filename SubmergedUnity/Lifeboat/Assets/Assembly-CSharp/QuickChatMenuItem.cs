using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class QuickChatMenuItem
{
	public Sprite icon;

	public string text;

	public byte playerID = byte.MaxValue;

	public byte alternatePlayerID = byte.MaxValue;

	public QuickChatMenuItem.QuickChatMenuItemDisplayType displayType;

	public QuickChatMenuItem.QuickChatMenuItemType itemType;

	public StringNames locStringKey;

	public StringNames locStringAltKey;

	public string alternateText;

	public QuickChatSubmenu targetSubmenu;

	public string fillInText;

	public string alternateFillInText;

	public QuickChatSubmenu[] fillBlankSelectionsInOder;

	public QuickChatSubmenu[] alternateFillBlankSelectionsInOder;

	public Button.ButtonClickedEvent OnClick = new Button.ButtonClickedEvent();

	[HideInInspector]
	public bool initialized;

	public bool ShouldDisplay
	{
		get
		{
			switch (this.displayType)
			{
			case QuickChatMenuItem.QuickChatMenuItemDisplayType.Always:
				return true;
			case QuickChatMenuItem.QuickChatMenuItemDisplayType.OnlyLobby:
				return LobbyBehaviour.Instance;
			case QuickChatMenuItem.QuickChatMenuItemDisplayType.OnlyInGame:
				return ShipStatus.Instance;
			case QuickChatMenuItem.QuickChatMenuItemDisplayType.OnlyAliveInGame:
				return ShipStatus.Instance && PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead;
			case QuickChatMenuItem.QuickChatMenuItemDisplayType.OnlyDeadInGame:
				return ShipStatus.Instance && PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data.IsDead;
			default:
				return true;
			}
		}
	}

	public void CreateFillInFromRegularText()
	{
		string text = this.text;
		string text2 = this.alternateText;
		if (!string.IsNullOrEmpty(text))
		{
			int num = 0;
			for (int i = 0; i < 26; i++)
			{
				string text3 = "(" + ((char)(65 + i)).ToString() + ")";
				string newValue = "{" + i.ToString() + "}";
				if (text.Contains(text3))
				{
					num++;
				}
				text = text.Replace(text3, newValue);
			}
			if (text != this.text)
			{
				this.fillInText = text;
				if (this.fillBlankSelectionsInOder == null || this.fillBlankSelectionsInOder.Length != num)
				{
					this.fillBlankSelectionsInOder = new QuickChatSubmenu[num];
				}
			}
			else
			{
				this.fillBlankSelectionsInOder = new QuickChatSubmenu[0];
			}
		}
		if (!string.IsNullOrEmpty(text2))
		{
			int num2 = 0;
			for (int j = 0; j < 26; j++)
			{
				string text4 = "(" + ((char)(65 + j)).ToString() + ")";
				string newValue2 = "{" + j.ToString() + "}";
				if (text2.Contains(text4))
				{
					num2++;
				}
				text2 = text2.Replace(text4, newValue2);
			}
			if (text2 != this.alternateText)
			{
				this.alternateFillInText = text2;
				if (this.alternateFillBlankSelectionsInOder == null || this.alternateFillBlankSelectionsInOder.Length != num2)
				{
					this.alternateFillBlankSelectionsInOder = new QuickChatSubmenu[num2];
					return;
				}
			}
			else
			{
				this.alternateFillBlankSelectionsInOder = new QuickChatSubmenu[0];
			}
		}
	}

	private string GeneratePreviewText(string formatStr)
	{
		string[] array = new string[]
		{
			"{0}",
			"{1}",
			"{2}",
			"{3}"
		};
		string[] array2 = new string[]
		{
			"(A)",
			"(B)",
			"(C)",
			"(D)"
		};
		List<QuickChatMenuItem.TempReplaceBit> list = new List<QuickChatMenuItem.TempReplaceBit>();
		for (int i = 0; i < array.Length; i++)
		{
			int num = formatStr.IndexOf(array[i]);
			if (num != -1)
			{
				list.Add(new QuickChatMenuItem.TempReplaceBit
				{
					location = num,
					originalStrIndex = i
				});
			}
		}
		list.Sort();
		string[] array3 = new string[list.Count];
		for (int j = 0; j < list.Count; j++)
		{
			array3[j] = array2[list[j].originalStrIndex];
		}
		object[] args = array3;
		return string.Format(formatStr, args);
	}

	public void InitLocKeys()
	{
		if (this.locStringKey != StringNames.ExitButton)
		{
			if (this.itemType == QuickChatMenuItem.QuickChatMenuItemType.FillInBlank)
			{
				this.fillInText = DestroyableSingleton<TranslationController>.Instance.GetString(this.locStringKey, Array.Empty<object>());
				this.text = this.GeneratePreviewText(this.fillInText);
			}
			else
			{
				this.text = DestroyableSingleton<TranslationController>.Instance.GetString(this.locStringKey, Array.Empty<object>());
			}
		}
		if (this.locStringAltKey != StringNames.ExitButton)
		{
			if (this.itemType == QuickChatMenuItem.QuickChatMenuItemType.FillInBlank)
			{
				this.alternateFillInText = DestroyableSingleton<TranslationController>.Instance.GetString(this.locStringAltKey, Array.Empty<object>());
				this.alternateText = this.GeneratePreviewText(this.alternateFillInText);
				return;
			}
			this.alternateText = DestroyableSingleton<TranslationController>.Instance.GetString(this.locStringAltKey, Array.Empty<object>());
		}
	}

	public enum QuickChatMenuItemType
	{
		Text,
		GoToSubmenu,
		FillInBlank,
		CustomButton
	}

	public enum QuickChatMenuItemDisplayType
	{
		Always,
		OnlyLobby,
		OnlyInGame,
		OnlyAliveInGame,
		OnlyDeadInGame
	}

	private class TempReplaceBit : IComparable
	{
		public int location;

		public int originalStrIndex;

		public int CompareTo(object obj)
		{
			return this.location.CompareTo(((QuickChatMenuItem.TempReplaceBit)obj).location);
		}
	}
}
