using System;
using System.Collections.Generic;
using UnityEngine;

public class MultistageMinigame : Minigame
{
	public Minigame[] Stages;

	private Minigame stage;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public List<UiElement> ControllerSelectable;

	public override void Begin(PlayerTask task)
	{
		NormalPlayerTask normalPlayerTask = task as NormalPlayerTask;
		if (normalPlayerTask.TaskType == TaskTypes.FuelEngines)
		{
			this.stage = this.Stages[(int)normalPlayerTask.Data[1]];
		}
		else
		{
			this.stage = this.Stages[normalPlayerTask.taskStep];
		}
		this.stage.gameObject.SetActive(true);
		this.stage.Begin(task);
		Minigame.Instance = this;
		UiElement defaultSelection = null;
		foreach (UiElement uiElement in this.ControllerSelectable)
		{
			if (uiElement.isActiveAndEnabled)
			{
				defaultSelection = uiElement;
				break;
			}
		}
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton, defaultSelection, this.ControllerSelectable, false);
	}

	public override void Close()
	{
		Minigame.Instance = null;
		this.stage.Close();
		base.Close();
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}
}
