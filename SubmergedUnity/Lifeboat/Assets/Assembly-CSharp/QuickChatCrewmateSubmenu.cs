using System;
using UnityEngine;

[RequireComponent(typeof(QuickChatSubmenu))]
public class QuickChatCrewmateSubmenu : MonoBehaviour
{
	public StringNames[] alwaysDisplayedOptions;

	private QuickChatSubmenu menu;

	private void Awake()
	{
		this.menu = base.GetComponent<QuickChatSubmenu>();
		QuickChatSubmenu quickChatSubmenu = this.menu;
		quickChatSubmenu.OnWillDisplay = (Action)Delegate.Combine(quickChatSubmenu.OnWillDisplay, new Action(this.Rebuild));
	}

	private void Rebuild()
	{
		this.menu.menuItems.Clear();
		int num = Mathf.CeilToInt(7.5f);
		int num2 = PlayerControl.AllPlayerControls.Count;
		if (PlayerControl.AllPlayerControls.Count > num)
		{
			this.menu.hasAlternateSet = true;
			this.menu.alternateSetName = StringNames.QCMore;
			this.menu.primarySetName = StringNames.QCMore;
			num2 = num;
		}
		else
		{
			this.menu.hasAlternateSet = false;
		}
		for (int i = 0; i < num2; i++)
		{
			PlayerControl playerControl = PlayerControl.AllPlayerControls[i];
			QuickChatMenuItem menuItem = new QuickChatMenuItem();
			menuItem.text = playerControl.Data.PlayerName;
			menuItem.playerID = playerControl.Data.PlayerId;
			if (this.menu.hasAlternateSet && i + num < PlayerControl.AllPlayerControls.Count)
			{
				PlayerControl playerControl2 = PlayerControl.AllPlayerControls[i + num];
				menuItem.alternateText = playerControl2.Data.PlayerName;
				menuItem.alternatePlayerID = playerControl2.Data.PlayerId;
			}
			menuItem.OnClick.AddListener(delegate()
			{
				this.menu.parentChatMenu.QuickChat(menuItem);
			});
			menuItem.initialized = true;
			this.menu.menuItems.Add(menuItem);
		}
		StringNames[] array = this.alwaysDisplayedOptions;
		for (int j = 0; j < array.Length; j++)
		{
			StringNames locStringKey = array[j];
			QuickChatMenuItem menuItem = new QuickChatMenuItem();
			menuItem.locStringKey = locStringKey;
			menuItem.InitLocKeys();
			menuItem.OnClick.AddListener(delegate()
			{
				this.menu.parentChatMenu.QuickChat(menuItem);
			});
			menuItem.initialized = true;
			this.menu.menuItems.Add(menuItem);
		}
	}
}
