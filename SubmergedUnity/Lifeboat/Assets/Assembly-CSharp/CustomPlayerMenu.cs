using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using Object = UnityEngine.Object;

public class CustomPlayerMenu : MonoBehaviour
{
	public static CustomPlayerMenu Instance;

	public TabButton[] Tabs;

	private int selectedTab;

	public GameObject PreviewArea;

	public Sprite NormalColor;

	public Sprite SelectedColor;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	public GameObject glyphL;

	public GameObject glyphR;

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	public void Start()
	{
		if (CustomPlayerMenu.Instance && CustomPlayerMenu.Instance != this)
		{
			 UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			CustomPlayerMenu.Instance = this;
		}
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton, this.DefaultButtonSelected, this.ControllerSelectable, true);
		if (this.Tabs.Length != 0 && this.Tabs[0].Tab != null)
		{
			PlayerTab component = this.Tabs[0].Tab.GetComponent<PlayerTab>();
			if (component != null)
			{
				foreach (ColorChip colorChip in component.ColorChips)
				{
					ControllerManager.Instance.AddSelectableUiElement(colorChip.Button, false);
				}
				ColorChip defaultSelectable = component.GetDefaultSelectable();
				if (defaultSelectable)
				{
					ControllerManager.Instance.SetCurrentSelected(defaultSelectable.Button);
					return;
				}
				if (component.ColorChips.Count > 0)
				{
					ControllerManager.Instance.SetCurrentSelected(component.ColorChips[0].Button);
				}
			}
		}
	}

	public void OpenTab(GameObject tab)
	{
		for (int i = 0; i < this.Tabs.Length; i++)
		{
			TabButton tabButton = this.Tabs[i];
			if (tabButton.Tab == tab)
			{
				this.selectedTab = i;
				tabButton.Tab.SetActive(true);
				tabButton.Button.sprite = this.SelectedColor;
				List<ColorChip> list = new List<ColorChip>();
				ColorChip colorChip = null;
				this.PreviewArea.SetActive(false);
				PlayerTab component = tabButton.Tab.GetComponent<PlayerTab>();
				if (component != null)
				{
					this.PreviewArea.SetActive(true);
					list = component.ColorChips;
					colorChip = component.GetDefaultSelectable();
				}
				HatsTab component2 = tabButton.Tab.GetComponent<HatsTab>();
				if (component2 != null)
				{
					this.PreviewArea.SetActive(true);
					list = component2.ColorChips;
					colorChip = component2.GetDefaultSelectable();
				}
				PetsTab component3 = tabButton.Tab.GetComponent<PetsTab>();
				if (component3 != null)
				{
					this.PreviewArea.SetActive(true);
					list = component3.ColorChips;
					colorChip = component3.GetDefaultSelectable();
				}
				SkinsTab component4 = tabButton.Tab.GetComponent<SkinsTab>();
				if (component4 != null)
				{
					this.PreviewArea.SetActive(true);
					list = component4.ColorChips;
					colorChip = component4.GetDefaultSelectable();
				}
				foreach (ColorChip colorChip2 in list)
				{
					ControllerManager.Instance.AddSelectableUiElement(colorChip2.Button, false);
				}
				if (colorChip)
				{
					ControllerManager.Instance.SetCurrentSelected(colorChip.Button);
				}
				else if (list.Count > 0)
				{
					ControllerManager.Instance.SetCurrentSelected(list[0].Button);
				}
				else
				{
					ControllerManager.Instance.PickTopSelectable();
				}
			}
			else
			{
				tabButton.Tab.SetActive(false);
				tabButton.Button.sprite = this.NormalColor;
			}
		}
		this.glyphL.SetActive(this.selectedTab != 0);
		this.glyphR.SetActive(this.selectedTab != this.Tabs.Length - 1);
		ControllerManager.Instance.ClearDestroyedSelectableUiElements();
	}

	public void Close(bool canMove)
	{
		 UnityEngine.Object.Destroy(base.gameObject);
	}

	private void Update()
	{
		Player player = ReInput.players.GetPlayer(0);
		if (this.selectedTab > 0 && player.GetButtonDown(35))
		{
			this.selectedTab--;
			this.OpenTab(this.Tabs[this.selectedTab].Tab);
		}
		if (this.selectedTab < this.Tabs.Length - 1 && player.GetButtonDown(34))
		{
			this.selectedTab++;
			this.OpenTab(this.Tabs[this.selectedTab].Tab);
		}
	}
}
