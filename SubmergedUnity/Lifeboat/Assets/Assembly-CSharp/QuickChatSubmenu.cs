using System;
using System.Collections.Generic;
using UnityEngine;

public class QuickChatSubmenu : MonoBehaviour
{
	[HideInInspector]
	public QuickChatSubmenu parentMenu;

	public QuickChatMenu parentChatMenu;

	public bool allowBackspace = true;

	public bool hasAlternateSet;

	public StringNames primarySetName = StringNames.QCMore;

	public StringNames alternateSetName = StringNames.QCMore;

	public QuickChatSubmenu.QuickChatColorSet alternateColorSet;

	public bool hasCustomColorSet;

	public QuickChatSubmenu.QuickChatColorSet customColorSet;

	public List<QuickChatMenuItem> menuItems = new List<QuickChatMenuItem>();

	[NonSerialized]
	public List<QuickChatMenuItem> activeMenuItems = new List<QuickChatMenuItem>();

	public Action OnWillDisplay;

	private List<string> menuButtonStrings = new List<string>();

	private List<string> altMenuButtonStrings = new List<string>();

	public void UpdateActiveItems(bool doAlternate = false)
	{
		this.activeMenuItems.Clear();
		if (!doAlternate)
		{
			for (int i = 0; i < this.menuItems.Count; i++)
			{
				if (!string.IsNullOrEmpty(this.menuItems[i].text) && this.menuItems[i].ShouldDisplay)
				{
					this.activeMenuItems.Add(this.menuItems[i]);
				}
			}
			return;
		}
		for (int j = 0; j < this.menuItems.Count; j++)
		{
			if (!string.IsNullOrEmpty(this.menuItems[j].alternateText) && this.menuItems[j].ShouldDisplay)
			{
				this.activeMenuItems.Add(this.menuItems[j]);
			}
		}
	}

	public string[] GetMenuButtonStrings(bool doAlternate = false)
	{
		if (!doAlternate)
		{
			this.menuButtonStrings.Clear();
			for (int i = 0; i < this.activeMenuItems.Count; i++)
			{
				this.menuButtonStrings.Add(this.activeMenuItems[i].text);
			}
		}
		else
		{
			this.altMenuButtonStrings.Clear();
			for (int j = 0; j < this.activeMenuItems.Count; j++)
			{
				this.altMenuButtonStrings.Add(this.activeMenuItems[j].alternateText);
			}
		}
		return (doAlternate ? this.altMenuButtonStrings : this.menuButtonStrings).ToArray();
	}

	public void Awake()
	{
		this.parentChatMenu = base.GetComponentInParent<QuickChatMenu>();
		using (List<QuickChatMenuItem>.Enumerator enumerator = this.menuItems.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				QuickChatMenuItem item = enumerator.Current;
				if (!item.initialized)
				{
					item.initialized = true;
					item.InitLocKeys();
					if (item.targetSubmenu)
					{
						item.targetSubmenu.parentMenu = this;
						item.OnClick.AddListener(delegate()
						{
							this.parentChatMenu.DisplayMenu(item.targetSubmenu, false);
						});
					}
					else if (item.itemType == QuickChatMenuItem.QuickChatMenuItemType.Text)
					{
						item.OnClick.AddListener(delegate()
						{
							this.parentChatMenu.QuickChat(item);
						});
					}
					else
					{
						item.OnClick.AddListener(delegate()
						{
							this.parentChatMenu.BeginFillInBlank(item);
						});
					}
				}
			}
		}
	}

	[Serializable]
	public class QuickChatColorSet
	{
		public Color fillColor;

		public Color edgeColor;
	}
}
