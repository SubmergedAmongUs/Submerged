using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class ControllerButtonBehavior : MonoBehaviour
{
	public RewiredConstsEnum.Action Action;

	public GameObject requiredMenuObject;

	public List<string> requiredMenuNames = new List<string>();

	private Player player;

	private void Start()
	{
		if (this.requiredMenuObject)
		{
			this.requiredMenuNames.Add(this.requiredMenuObject.name);
		}
		this.player = ReInput.players.GetPlayer(0);
	}

	private void Update()
	{
		if (RadialMenu.instances > 0)
		{
			return;
		}
		if (this.player.GetButtonDown((int)this.Action))
		{
			if (this.requiredMenuNames.Count > 0 && !this.requiredMenuNames.Contains(ControllerManager.Instance.CurrentUiState.MenuName))
			{
				return;
			}
			ButtonBehavior component = base.GetComponent<ButtonBehavior>();
			if (component)
			{
				Button.ButtonClickedEvent onClick = component.OnClick;
				if (onClick == null)
				{
					return;
				}
				onClick.Invoke();
				return;
			}
			else
			{
				PassiveButton component2 = base.GetComponent<PassiveButton>();
				if (component2)
				{
					Button.ButtonClickedEvent onClick2 = component2.OnClick;
					if (onClick2 == null)
					{
						return;
					}
					onClick2.Invoke();
				}
			}
		}
	}
}
