using System;
using System.Collections.Generic;

[Serializable]
public class ControllerUiElementsState
{
	public string MenuName = string.Empty;

	public UiElement CurrentSelection;

	public List<UiElement> SelectableUiElements = new List<UiElement>();

	public UiElement BackButton;

	public bool EnforceGridNavigation;

	public float zPos;

	public bool IsScene;

	public void Reset()
	{
		this.CurrentSelection = null;
		this.SelectableUiElements = new List<UiElement>();
		this.BackButton = null;
		this.EnforceGridNavigation = false;
		this.MenuName = string.Empty;
		this.IsScene = false;
	}
}
