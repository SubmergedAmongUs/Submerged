using System;
using System.Collections.Generic;
using UnityEngine;

public class QuickChatTasksSubmenu : MonoBehaviour
{
	private QuickChatSubmenu menu;

	public StringNames[] lobbyTasks;

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
			List<TaskTypes> list = new List<TaskTypes>();
			foreach (NormalPlayerTask normalPlayerTask in ShipStatus.Instance.CommonTasks)
			{
				if (!list.Contains(normalPlayerTask.TaskType))
				{
					list.Add(normalPlayerTask.TaskType);
				}
			}
			foreach (NormalPlayerTask normalPlayerTask2 in ShipStatus.Instance.NormalTasks)
			{
				if (!list.Contains(normalPlayerTask2.TaskType))
				{
					list.Add(normalPlayerTask2.TaskType);
				}
			}
			foreach (NormalPlayerTask normalPlayerTask3 in ShipStatus.Instance.LongTasks)
			{
				if (!list.Contains(normalPlayerTask3.TaskType))
				{
					list.Add(normalPlayerTask3.TaskType);
				}
			}
			if (list.Count > 8)
			{
				menu.hasAlternateSet = true;
				menu.alternateSetName = StringNames.QCMore;
				menu.primarySetName = StringNames.QCMore;
				int num = list.Count / 2 + ((list.Count % 2 == 1) ? 1 : 0);
				for (int j = 0; j < num; j++)
				{
					int index = j;
					int num2 = j + num;
					QuickChatMenuItem menuItem = new QuickChatMenuItem();
					menuItem.text = DestroyableSingleton<TranslationController>.Instance.GetString(list[index]);
					menuItem.locStringKey = DestroyableSingleton<TranslationController>.Instance.GetTaskName(list[index]);
					if (num2 < list.Count)
					{
						menuItem.alternateText = DestroyableSingleton<TranslationController>.Instance.GetString(list[num2]);
						menuItem.locStringAltKey = DestroyableSingleton<TranslationController>.Instance.GetTaskName(list[num2]);
					}
					menuItem.OnClick.AddListener(delegate()
					{
						menu.parentChatMenu.QuickChat(menuItem);
					});
					menuItem.initialized = true;
					menu.menuItems.Add(menuItem);
				}
				return;
			}
			menu.hasAlternateSet = false;
			using (List<TaskTypes>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					TaskTypes task = enumerator.Current;
					QuickChatMenuItem menuItem = new QuickChatMenuItem();
					menuItem.text = DestroyableSingleton<TranslationController>.Instance.GetString(task);
					menuItem.locStringKey = DestroyableSingleton<TranslationController>.Instance.GetTaskName(task);
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
			StringNames[] array2 = this.lobbyTasks;
			for (int i = 0; i < array2.Length; i++)
			{
				StringNames locStringKey = array2[i];
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
