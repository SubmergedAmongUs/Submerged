using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NumberMenu : MonoBehaviour
{
	public TextMeshPro text;

	public NumberSetter numberSetter;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	private UiElement defaultButtonSelected;

	private List<UiElement> controllerSelectable;

	private void Awake()
	{
		this.defaultButtonSelected = null;
		this.controllerSelectable = new List<UiElement>();
	}

	public void Open()
	{
		base.gameObject.SetActive(true);
	}

	public void SetValue(string val)
	{
		this.text.text = val;
		this.Close();
	}

	public void Close()
	{
		base.gameObject.SetActive(false);
	}
}
