using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(QuickChatSubmenu))]
public class QuickChatLocationSubmenu : MonoBehaviour
{
	private QuickChatSubmenu menu;

	public StringNames[] lobbyLocations;

	private string currentMap;

	private void Awake()
	{
		this.Rebuild();
		this.menu = base.GetComponent<QuickChatSubmenu>();
		QuickChatSubmenu quickChatSubmenu = this.menu;
		quickChatSubmenu.OnWillDisplay = (Action)Delegate.Combine(quickChatSubmenu.OnWillDisplay, new Action(this.Rebuild));
	}

	private void Rebuild()
	{
		QuickChatSubmenu menu = base.GetComponent<QuickChatSubmenu>();
		if (!ShipStatus.Instance)
		{
			if (LobbyBehaviour.Instance)
			{
				if (LobbyBehaviour.Instance.name == this.currentMap)
				{
					return;
				}
				this.currentMap = LobbyBehaviour.Instance.name;
				menu.menuItems.Clear();
				StringNames[] array = this.lobbyLocations;
				for (int i = 0; i < array.Length; i++)
				{
					StringNames locStringKey = array[i];
					QuickChatMenuItem menuItem = new QuickChatMenuItem();
					menuItem.locStringKey = locStringKey;
					menuItem.InitLocKeys();
					menuItem.OnClick.AddListener(delegate()
					{
						menu.parentChatMenu.QuickChat(menuItem);
					});
					menuItem.initialized = true;
					menu.menuItems.Add(menuItem);
				}
			}
			return;
		}
		if (ShipStatus.Instance.name == this.currentMap)
		{
			return;
		}
		this.currentMap = ShipStatus.Instance.name;
		menu.menuItems.Clear();
		List<ValueTuple<string, StringNames>> list = (from s in (from r in ShipStatus.Instance.AllRooms
		select new ValueTuple<string, StringNames>(DestroyableSingleton<TranslationController>.Instance.GetString(r.RoomId), DestroyableSingleton<TranslationController>.Instance.GetSystemName(r.RoomId))).Distinct<ValueTuple<string, StringNames>>()
		orderby s.Item1
		select s).ToList<ValueTuple<string, StringNames>>();
		int num = list.Count;
		if (list.Count > 8)
		{
			menu.hasAlternateSet = true;
			menu.alternateSetName = StringNames.QCMore;
			menu.primarySetName = StringNames.QCMore;
			num = Mathf.CeilToInt((float)list.Count / 2f);
		}
		else
		{
			menu.hasAlternateSet = false;
		}
		for (int j = 0; j < num; j++)
		{
			int index = j;
			int num2 = j + num;
			QuickChatMenuItem menuItem = new QuickChatMenuItem();
			menuItem.text = list[index].Item1;
			menuItem.locStringKey = list[index].Item2;
			if (num2 < list.Count)
			{
				menuItem.alternateText = list[num2].Item1;
				menuItem.locStringAltKey = list[num2].Item2;
			}
			menuItem.OnClick.AddListener(delegate()
			{
				menu.parentChatMenu.QuickChat(menuItem);
			});
			menuItem.initialized = true;
			menu.menuItems.Add(menuItem);
		}
	}
}
