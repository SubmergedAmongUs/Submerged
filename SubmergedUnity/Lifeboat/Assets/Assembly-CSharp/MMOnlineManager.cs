using System;
using System.Collections.Generic;
using UnityEngine;

public class MMOnlineManager : DestroyableSingleton<MMOnlineManager>
{
	public GameObject HelpMenu;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	public bool IsControllerManagerSceneInit;

	public void Start()
	{
		ControllerManager.Instance.NewScene(base.name, this.BackButton, this.DefaultButtonSelected, this.ControllerSelectable, false);
		this.IsControllerManagerSceneInit = true;
		if (VirtualCursor.instance)
		{
			VirtualCursor.instance.gameObject.SetActive(false);
		}
		if (this.HelpMenu)
		{
			if (SaveManager.ShowOnlineHelp)
			{
				SaveManager.ShowOnlineHelp = false;
				this.HelpMenu.gameObject.SetActive(true);
				return;
			}
			this.HelpMenu.gameObject.SetActive(false);
		}
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			SceneChanger.ChangeScene("MainMenu");
		}
	}
}
