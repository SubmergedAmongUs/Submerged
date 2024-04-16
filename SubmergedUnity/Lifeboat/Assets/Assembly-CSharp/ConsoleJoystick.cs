using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

public class ConsoleJoystick : MonoBehaviour, IVirtualJoystick
{
	private Vector2 delta;

	private static Player player;

	private static Rewired.Controller controller;

	private static Rewired.Controller prevController;

	public static ConsoleJoystick.ConsoleInputState inputState = ConsoleJoystick.ConsoleInputState.Menu;

	private static ConsoleJoystick.ConsoleInputState oldInputState = ConsoleJoystick.ConsoleInputState.Sabotage;

	private static int highlightedVentIndex = 0;

	public Vector2 Delta
	{
		get
		{
			return this.delta;
		}
	}

	private void OnEnable()
	{
		RewiredControllerSupport.onPostControllersAssigned = (Action)Delegate.Combine(RewiredControllerSupport.onPostControllersAssigned, new Action(ConsoleJoystick.ReapplyCurrentInputState));
		if (ConsoleJoystick.inputState == ConsoleJoystick.ConsoleInputState.Menu)
		{
			ConsoleJoystick.SetMode_Gameplay();
			return;
		}
		ConsoleJoystick.ReapplyCurrentInputState();
	}

	private void OnDisable()
	{
		RewiredControllerSupport.onPostControllersAssigned = (Action)Delegate.Remove(RewiredControllerSupport.onPostControllersAssigned, new Action(ConsoleJoystick.ReapplyCurrentInputState));
	}

	public static void ReapplyCurrentInputState()
	{
		Debug.LogError("New controller connected, updating current input state");
		switch (ConsoleJoystick.inputState)
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

	public static void SetMode_MenuAdditive()
	{
		ConsoleJoystick.player = ReInput.players.GetPlayer(0);
		IList<Joystick> joysticks = ConsoleJoystick.player.controllers.Joysticks;
		for (int i = 0; i < ConsoleJoystick.player.controllers.joystickCount; i++)
		{
			ConsoleJoystick.controller = joysticks[i];
			ConsoleJoystick.SetMapEnabled(2, true);
		}
	}

	public static void SetMode_Gameplay()
	{
		ConsoleJoystick.inputState = ConsoleJoystick.ConsoleInputState.Gameplay;
		ConsoleJoystick.player = ReInput.players.GetPlayer(0);
		if (VirtualCursor.instance)
		{
			VirtualCursor.instance.gameObject.SetActive(false);
		}
		IList<Joystick> joysticks = ConsoleJoystick.player.controllers.Joysticks;
		for (int i = 0; i < ConsoleJoystick.player.controllers.joystickCount; i++)
		{
			ConsoleJoystick.controller = joysticks[i];
			ConsoleJoystick.SetMapEnabled(1, true);
			ConsoleJoystick.SetMapEnabled(2, false);
			ConsoleJoystick.SetMapEnabled(3, false);
			ConsoleJoystick.SetMapEnabled(4, false);
		}
	}

	public static void SetMode_Menu()
	{
		ConsoleJoystick.inputState = ConsoleJoystick.ConsoleInputState.Menu;
		ConsoleJoystick.player = ReInput.players.GetPlayer(0);
		if (VirtualCursor.instance && ControllerManager.Instance && !ControllerManager.Instance.IsUiControllerActive)
		{
			VirtualCursor.instance.gameObject.SetActive(true);
		}
		IList<Joystick> joysticks = ConsoleJoystick.player.controllers.Joysticks;
		for (int i = 0; i < ConsoleJoystick.player.controllers.joystickCount; i++)
		{
			ConsoleJoystick.controller = joysticks[i];
			ConsoleJoystick.SetMapEnabled(1, false);
			ConsoleJoystick.SetMapEnabled(2, true);
			ConsoleJoystick.SetMapEnabled(3, false);
			ConsoleJoystick.SetMapEnabled(4, false);
		}
		VirtualCursor.horizontalAxis = 9;
		VirtualCursor.verticalAxis = 10;
	}

	public static void SetMode_Task()
	{
		ConsoleJoystick.inputState = ConsoleJoystick.ConsoleInputState.Task;
		ConsoleJoystick.player = ReInput.players.GetPlayer(0);
		if (VirtualCursor.instance && !ControllerManager.Instance.IsUiControllerActive)
		{
			VirtualCursor.instance.gameObject.SetActive(true);
		}
		IList<Joystick> joysticks = ConsoleJoystick.player.controllers.Joysticks;
		for (int i = 0; i < ConsoleJoystick.player.controllers.joystickCount; i++)
		{
			ConsoleJoystick.controller = joysticks[i];
			ConsoleJoystick.SetMapEnabled(1, false);
			ConsoleJoystick.SetMapEnabled(2, false);
			ConsoleJoystick.SetMapEnabled(3, true);
			ConsoleJoystick.SetMapEnabled(4, false);
		}
		VirtualCursor.horizontalAxis = 13;
		VirtualCursor.verticalAxis = 14;
	}

	public static void SetMode_Sabotage()
	{
		ConsoleJoystick.inputState = ConsoleJoystick.ConsoleInputState.Sabotage;
		ConsoleJoystick.player = ReInput.players.GetPlayer(0);
		if (VirtualCursor.instance)
		{
			VirtualCursor.instance.gameObject.SetActive(false);
		}
		IList<Joystick> joysticks = ConsoleJoystick.player.controllers.Joysticks;
		for (int i = 0; i < ConsoleJoystick.player.controllers.joystickCount; i++)
		{
			ConsoleJoystick.controller = joysticks[i];
			ConsoleJoystick.SetMapEnabled(1, true);
			ConsoleJoystick.SetMapEnabled(2, false);
			ConsoleJoystick.SetMapEnabled(3, true);
			ConsoleJoystick.SetMapEnabled(4, false);
		}
		VirtualCursor.horizontalAxis = 16;
		VirtualCursor.verticalAxis = 17;
	}

	public static void SetMode_Vent()
	{
		ConsoleJoystick.inputState = ConsoleJoystick.ConsoleInputState.Vent;
		ConsoleJoystick.player = ReInput.players.GetPlayer(0);
		if (VirtualCursor.instance && !ControllerManager.Instance.IsUiControllerActive)
		{
			VirtualCursor.instance.gameObject.SetActive(false);
		}
		IList<Joystick> joysticks = ConsoleJoystick.player.controllers.Joysticks;
		for (int i = 0; i < ConsoleJoystick.player.controllers.joystickCount; i++)
		{
			ConsoleJoystick.controller = joysticks[i];
			ConsoleJoystick.SetMapEnabled(1, true);
			ConsoleJoystick.SetMapEnabled(2, false);
			ConsoleJoystick.SetMapEnabled(3, false);
			ConsoleJoystick.SetMapEnabled(4, false);
		}
		ConsoleJoystick.highlightedVentIndex = -1;
		VirtualCursor.horizontalAxis = 16;
		VirtualCursor.verticalAxis = 17;
	}

	public static void SetMode_QuickChat()
	{
		ConsoleJoystick.inputState = ConsoleJoystick.ConsoleInputState.QuickChat;
		ConsoleJoystick.player = ReInput.players.GetPlayer(0);
		if (VirtualCursor.instance && !ControllerManager.Instance.IsUiControllerActive)
		{
			VirtualCursor.instance.gameObject.SetActive(true);
		}
		IList<Joystick> joysticks = ConsoleJoystick.player.controllers.Joysticks;
		for (int i = 0; i < ConsoleJoystick.player.controllers.joystickCount; i++)
		{
			ConsoleJoystick.controller = joysticks[i];
			ConsoleJoystick.SetMapEnabled(1, false);
			ConsoleJoystick.SetMapEnabled(2, false);
			ConsoleJoystick.SetMapEnabled(3, false);
			ConsoleJoystick.SetMapEnabled(4, true);
		}
		VirtualCursor.horizontalAxis = 13;
		VirtualCursor.verticalAxis = 14;
	}

	private static void SetMapEnabled(int rewiredCategoryIndex, bool enableMap)
	{
		if (ConsoleJoystick.controller == null)
		{
			return;
		}
		ConsoleJoystick.player.controllers.maps.GetMap(ConsoleJoystick.controller, rewiredCategoryIndex, 0).enabled = enableMap;
	}

	private void Update()
	{
		if (!PlayerControl.LocalPlayer)
		{
			return;
		}
		if (ConsoleJoystick.oldInputState != ConsoleJoystick.inputState)
		{
			if (DestroyableSingleton<HudManager>.Instance.consoleUIObjects[(int)ConsoleJoystick.oldInputState])
			{
				DestroyableSingleton<HudManager>.Instance.consoleUIObjects[(int)ConsoleJoystick.oldInputState].SetActive(false);
			}
			if (DestroyableSingleton<HudManager>.Instance.consoleUIObjects[(int)ConsoleJoystick.inputState])
			{
				DestroyableSingleton<HudManager>.Instance.consoleUIObjects[(int)ConsoleJoystick.inputState].SetActive(true);
			}
			ConsoleJoystick.oldInputState = ConsoleJoystick.inputState;
		}
		if (ConsoleJoystick.inputState == ConsoleJoystick.ConsoleInputState.Menu && ControllerManager.Instance.IsUiControllerActive != DestroyableSingleton<HudManager>.Instance.consoleUIObjects[(int)ConsoleJoystick.inputState].activeSelf)
		{
			DestroyableSingleton<HudManager>.Instance.consoleUIObjects[(int)ConsoleJoystick.inputState].SetActive(ControllerManager.Instance.IsUiControllerActive);
		}
		this.delta.x = ConsoleJoystick.player.GetAxis(2);
		this.delta.y = ConsoleJoystick.player.GetAxis(3);
		if (this.delta.sqrMagnitude > 1f)
		{
			this.delta = this.delta.normalized;
		}
		this.HandleHUD();
	}

	private void HandleHUD()
	{
		if (PlayerControl.LocalPlayer)
		{
			bool canMove = PlayerControl.LocalPlayer.CanMove;
			if (canMove && PlayerControl.LocalPlayer.MyPhysics.inputHandler.enabled)
			{
				PlayerControl.LocalPlayer.MyPhysics.inputHandler.enabled = false;
			}
			if (!canMove && ConsoleJoystick.inputState == ConsoleJoystick.ConsoleInputState.Gameplay)
			{
				ConsoleJoystick.SetMode_Menu();
				return;
			}
			if (canMove && ConsoleJoystick.inputState != ConsoleJoystick.ConsoleInputState.Sabotage && ConsoleJoystick.inputState != ConsoleJoystick.ConsoleInputState.Gameplay)
			{
				ConsoleJoystick.SetMode_Gameplay();
				return;
			}
		}
		if (ConsoleJoystick.inputState == ConsoleJoystick.ConsoleInputState.Vent)
		{
			if (!ControllerManager.Instance.IsUiControllerActive)
			{
				if (!Vent.currentVent)
				{
					ConsoleJoystick.SetMode_Gameplay();
				}
				else
				{
					bool flag = false;
					if (this.delta.sqrMagnitude > 0.25f)
					{
						flag = true;
						Vector2 normalized = this.delta.normalized;
						Vector2 vector = Vector2.zero;
						float num = float.NegativeInfinity;
						int num2 = -1;
						for (int i = 0; i < Vent.currentVent.Buttons.Length; i++)
						{
							if (Vent.currentVent.Buttons[i].isActiveAndEnabled)
							{
								vector = Vent.currentVent.Buttons[i].transform.localPosition.normalized;
								float num3 = Vector2.Dot(normalized, vector);
								if (num2 == -1 || num3 > num)
								{
									num = num3;
									num2 = i;
								}
							}
						}
						if (num > 0.7f)
						{
							if (ConsoleJoystick.highlightedVentIndex != -1 && ConsoleJoystick.highlightedVentIndex < Vent.currentVent.Buttons.Length)
							{
								Vent.currentVent.Buttons[ConsoleJoystick.highlightedVentIndex].spriteRenderer.color = Color.white;
								ConsoleJoystick.highlightedVentIndex = -1;
							}
							ConsoleJoystick.highlightedVentIndex = num2;
							Vent.currentVent.Buttons[ConsoleJoystick.highlightedVentIndex].spriteRenderer.color = Color.red;
						}
						else if (ConsoleJoystick.highlightedVentIndex != -1 && ConsoleJoystick.highlightedVentIndex < Vent.currentVent.Buttons.Length)
						{
							Vent.currentVent.Buttons[ConsoleJoystick.highlightedVentIndex].spriteRenderer.color = Color.white;
							ConsoleJoystick.highlightedVentIndex = -1;
						}
					}
					else if (ConsoleJoystick.highlightedVentIndex != -1 && ConsoleJoystick.highlightedVentIndex < Vent.currentVent.Buttons.Length)
					{
						Vent.currentVent.Buttons[ConsoleJoystick.highlightedVentIndex].spriteRenderer.color = Color.white;
						ConsoleJoystick.highlightedVentIndex = -1;
					}
					if (!flag)
					{
						if (ConsoleJoystick.player.GetButtonDown(6))
						{
							DestroyableSingleton<HudManager>.Instance.UseButton.DoClick();
						}
					}
					else if (ConsoleJoystick.highlightedVentIndex != -1 && ConsoleJoystick.player.GetButtonDown(6))
					{
						Vent.currentVent.Buttons[ConsoleJoystick.highlightedVentIndex].spriteRenderer.color = Color.white;
						Vent.currentVent.Buttons[ConsoleJoystick.highlightedVentIndex].OnClick.Invoke();
						ConsoleJoystick.highlightedVentIndex = -1;
					}
				}
			}
		}
		else
		{
			if (ConsoleJoystick.player.GetButtonDown(7))
			{
				DestroyableSingleton<HudManager>.Instance.ReportButton.DoClick();
			}
			if (ConsoleJoystick.player.GetButtonDown(6))
			{
				DestroyableSingleton<HudManager>.Instance.UseButton.DoClick();
			}
			if (PlayerControl.LocalPlayer.Data != null && PlayerControl.LocalPlayer.Data.IsImpostor && ConsoleJoystick.player.GetButtonDown(8))
			{
				DestroyableSingleton<HudManager>.Instance.KillButton.PerformKill();
			}
		}
		if (ConsoleJoystick.player.GetButtonDown(5) && DestroyableSingleton<TaskPanelBehaviour>.InstanceExists)
		{
			DestroyableSingleton<TaskPanelBehaviour>.Instance.ToggleOpen();
		}
		if (ConsoleJoystick.player.GetButtonDown(1))
		{
			if (DestroyableSingleton<HudManager>.Instance.GameMenu.IsOpen)
			{
				DestroyableSingleton<HudManager>.Instance.GameMenu.Close();
			}
			else
			{
				DestroyableSingleton<HudManager>.Instance.GameMenu.Open();
			}
		}
		if (ConsoleJoystick.player.GetButtonDown(4))
		{
			if (ConsoleJoystick.inputState == ConsoleJoystick.ConsoleInputState.Sabotage)
			{
				ConsoleJoystick.SetMode_Gameplay();
				if (MapBehaviour.Instance)
				{
					MapBehaviour.Instance.Close();
				}
			}
			else
			{
				DestroyableSingleton<HudManager>.Instance.OpenMap();
			}
		}
		if (ConsoleJoystick.player.GetButtonDown(12))
		{
			if (ControllerManager.Instance.IsUiControllerActive)
			{
				return;
			}
			if (Minigame.Instance)
			{
				Minigame.Instance.Close();
			}
			else if (MapBehaviour.Instance)
			{
				MapBehaviour.Instance.Close();
			}
			if (Vent.currentVent)
			{
				ConsoleJoystick.SetMode_Vent();
				return;
			}
			if (ConsoleJoystick.inputState != ConsoleJoystick.ConsoleInputState.Gameplay)
			{
				ConsoleJoystick.SetMode_Gameplay();
			}
		}
	}

	public enum ConsoleInputState
	{
		Gameplay,
		Menu,
		Task,
		Sabotage,
		Vent,
		QuickChat
	}
}
