using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class QuickChatSystemsSubmenu : MonoBehaviour
{
	private QuickChatSubmenu menu;

	public StringNames[] lobbySystems;

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
		if (ShipStatus.Instance)
		{
			if (ShipStatus.Instance.name == this.currentMap)
			{
				return;
			}
			this.currentMap = ShipStatus.Instance.name;
			menu.menuItems.Clear();
			using (IEnumerator<ValueTuple<string, StringNames>> enumerator = (from s in ShipStatus.Instance.SystemNames
			select new ValueTuple<string, StringNames>(DestroyableSingleton<TranslationController>.Instance.GetString(s, Array.Empty<object>()), s) into s
			orderby s.Item1
			select s).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ValueTuple<string, StringNames> valueTuple = enumerator.Current;
					QuickChatMenuItem menuItem = new QuickChatMenuItem();
					menuItem.text = valueTuple.Item1;
					menuItem.locStringKey = valueTuple.Item2;
					menuItem.OnClick.AddListener(delegate()
					{
						menu.parentChatMenu.QuickChat(menuItem);
					});
					menuItem.initialized = true;
					menu.menuItems.Add(menuItem);
				}
				return;
			}
		}
		if (LobbyBehaviour.Instance)
		{
			if (LobbyBehaviour.Instance.name == this.currentMap)
			{
				return;
			}
			this.currentMap = LobbyBehaviour.Instance.name;
			menu.menuItems.Clear();
			StringNames[] array = this.lobbySystems;
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
	}
}
