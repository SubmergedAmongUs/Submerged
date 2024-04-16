using System;
using UnityEngine;

[RequireComponent(typeof(QuickChatSubmenu))]
public class QuickChatFavoritesSubmenu : MonoBehaviour
{
	private void Awake()
	{
		QuickChatSubmenu menu = base.GetComponent<QuickChatSubmenu>();
		menu.menuItems.Clear();
		int num = 10;
		string[] quickChatFavorites = SaveManager.QuickChatFavorites;
		for (int i = 0; i < num; i++)
		{
			int num2 = i;
			int num3 = num + i;
			QuickChatMenuItem menuItem = new QuickChatMenuItem();
			menuItem.text = quickChatFavorites[num2];
			menuItem.alternateText = quickChatFavorites[num3];
			menuItem.OnClick.AddListener(delegate()
			{
				menu.parentChatMenu.QuickChat(menuItem);
			});
			menuItem.initialized = true;
			menu.menuItems.Add(menuItem);
		}
	}
}
