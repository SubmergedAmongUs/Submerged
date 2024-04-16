using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

public class RewiredCategoryEnabler : MonoBehaviour
{
	public bool applyDefaultMapState;

	public RewiredCategoryEnabler.RewiredCategoryState[] defaultStates;

	private void OnEnable()
	{
		ReInput.ControllerConnectedEvent += this.ReInput_ControllerConnectedEvent;
	}

	private void OnDisable()
	{
		ReInput.ControllerConnectedEvent -= this.ReInput_ControllerConnectedEvent;
	}

	private void ReInput_ControllerConnectedEvent(ControllerStatusChangedEventArgs obj)
	{
		if (this.applyDefaultMapState)
		{
			this.ApplyDefaultMapState();
		}
	}

	public void ApplyDefaultMapState()
	{
		IEnumerable<Player> allPlayers = ReInput.players.AllPlayers;
		Debug.Log("RewiredCategoryEnabler - Applying default map state to all players");
		int num = 0;
		foreach (Player player in allPlayers)
		{
			foreach (Joystick joystick in player.controllers.Joysticks)
			{
				num++;
				foreach (RewiredCategoryEnabler.RewiredCategoryState rewiredCategoryState in this.defaultStates)
				{
					player.controllers.maps.GetMap(joystick, rewiredCategoryState.name, "Default").enabled = rewiredCategoryState.enabled;
				}
			}
		}
		Debug.Log("Applied default map state to " + num.ToString() + " joysticks");
		if (ConsoleJoystick.inputState != ConsoleJoystick.ConsoleInputState.Menu)
		{
			ConsoleJoystick.ReapplyCurrentInputState();
		}
	}

	[Serializable]
	public class RewiredCategoryState
	{
		public string name;

		public bool enabled;
	}
}
