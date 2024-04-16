using System;
using UnityEngine;

public class SetInputModeOnAwake : MonoBehaviour
{
	public ConsoleJoystick.ConsoleInputState inputMode;

	private void Awake()
	{
		switch (this.inputMode)
		{
		case ConsoleJoystick.ConsoleInputState.Gameplay:
			ConsoleJoystick.SetMode_Gameplay();
			return;
		case ConsoleJoystick.ConsoleInputState.Menu:
			ConsoleJoystick.SetMode_Menu();
			return;
		case ConsoleJoystick.ConsoleInputState.Task:
			ConsoleJoystick.SetMode_Task();
			return;
		case ConsoleJoystick.ConsoleInputState.Sabotage:
			ConsoleJoystick.SetMode_Sabotage();
			return;
		case ConsoleJoystick.ConsoleInputState.Vent:
			ConsoleJoystick.SetMode_Vent();
			return;
		case ConsoleJoystick.ConsoleInputState.QuickChat:
			ConsoleJoystick.SetMode_QuickChat();
			return;
		default:
			return;
		}
	}
}
