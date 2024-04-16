using System;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class ControllerButtonBehaviourComplex : MonoBehaviour
{
	public ControllerButtonBehaviourComplex.ActionTriggerType actionTriggerType;

	public ControllerButtonBehaviourComplex.ActionTrigger[] actionTriggers;

	public GameObject requiredMenuObject;

	private Player player;

	private void Start()
	{
		this.player = ReInput.players.GetPlayer(0);
	}

	private bool ConditionMet()
	{
		if (this.actionTriggerType == ControllerButtonBehaviourComplex.ActionTriggerType.All)
		{
			foreach (ControllerButtonBehaviourComplex.ActionTrigger actionTrigger in this.actionTriggers)
			{
				if (!actionTrigger.ConditionMet(this.player))
				{
					return false;
				}
			}
			return true;
		}
		foreach (ControllerButtonBehaviourComplex.ActionTrigger actionTrigger2 in this.actionTriggers)
		{
			if (actionTrigger2.ConditionMet(this.player))
			{
				return true;
			}
		}
		return false;
	}

	private void Update()
	{
		if (this.ConditionMet())
		{
			if (this.requiredMenuObject && ControllerManager.Instance.CurrentUiState.MenuName != this.requiredMenuObject.name)
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

	public enum ActionTriggerType
	{
		Any,
		All
	}

	[Serializable]
	public struct ActionTrigger
	{
		public RewiredConstsEnum.Action action;

		public ControllerButtonBehaviourComplex.ActionTrigger.ActionType actionType;

		public float axisComparisonValue;

		public bool ConditionMet(Player player)
		{
			switch (this.actionType)
			{
			case ControllerButtonBehaviourComplex.ActionTrigger.ActionType.Axis_GEqual:
				return player.GetAxisRaw((int)this.action) >= this.axisComparisonValue;
			case ControllerButtonBehaviourComplex.ActionTrigger.ActionType.Axis_LEqual:
				return player.GetAxisRaw((int)this.action) <= this.axisComparisonValue;
			case ControllerButtonBehaviourComplex.ActionTrigger.ActionType.Button_Down:
				return player.GetButtonDown((int)this.action);
			default:
				return false;
			}
		}

		public enum ActionType
		{
			Axis_GEqual,
			Axis_LEqual,
			Button_Down
		}
	}
}
