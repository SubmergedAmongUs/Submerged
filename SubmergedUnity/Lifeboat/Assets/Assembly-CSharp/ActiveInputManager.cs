using System;
using Rewired;
using UnityEngine;

public class ActiveInputManager : DestroyableSingleton<ActiveInputManager>
{
	public ActiveInputManager.InputType testCurrentControlType;

	public static ActiveInputManager.InputType currentControlType = ActiveInputManager.PlatformDefault;

	public double kChangeTime;

	public double mChangeTime;

	public double jChangeTime;

	public double tChangeTime;

	public static Action CurrentInputSourceChanged;

	private Rewired.Controller lastUsedController;

	private static ActiveInputManager.InputType PlatformDefault
	{
		get
		{
			return ActiveInputManager.InputType.Keyboard;
		}
	}

	private void Start()
	{
		ReInput.players.GetPlayer(0).controllers.AddLastActiveControllerChangedDelegate(new PlayerActiveControllerChangedDelegate(this.OnLastActiveControllerChanged));
	}

	private void Update()
	{
		this.UpdateJoystickState();
	}

	public void SetTouchAsCurrentInput()
	{
		ActiveInputManager.currentControlType = ActiveInputManager.InputType.Touch;
		this.tChangeTime = Math.Max(this.kChangeTime, Math.Max(this.mChangeTime, this.jChangeTime));
	}

	private void OnLastActiveControllerChanged(Player player, Rewired.Controller controller)
	{
		if (controller != null)
		{
			Debug.Log("OnLastActiveControllerChanged: " + controller.name);
			ActiveInputManager.currentControlType = ActiveInputManager.InputType.Joystick;
			if (DestroyableSingleton<HudManager>.InstanceExists && !(DestroyableSingleton<HudManager>.Instance.joystick is ConsoleJoystick))
			{
				DestroyableSingleton<HudManager>.Instance.SetTouchType(ControlTypes.Controller);
			}
		}
		else
		{
			Debug.Log("OnLastActiveControllerChanged: NULL");
			ActiveInputManager.currentControlType = ActiveInputManager.PlatformDefault;
			if (DestroyableSingleton<HudManager>.InstanceExists && DestroyableSingleton<HudManager>.Instance.joystick is ConsoleJoystick)
			{
				DestroyableSingleton<HudManager>.Instance.SetTouchType(SaveManager.ControlMode);
			}
		}
		this.lastUsedController = controller;
		if (ActiveInputManager.CurrentInputSourceChanged != null)
		{
			ActiveInputManager.CurrentInputSourceChanged();
		}
	}

	public void UpdateJoystickState()
	{
		Player player = ReInput.players.GetPlayer(0);
		ActiveInputManager.InputType inputType = ActiveInputManager.currentControlType;
		if (ActiveInputManager.currentControlType != ActiveInputManager.InputType.Keyboard)
		{
			if (player.controllers.hasKeyboard)
			{
				this.kChangeTime = player.controllers.Keyboard.GetLastTimeAnyElementChanged();
				if (this.kChangeTime > this.jChangeTime && this.kChangeTime > this.tChangeTime)
				{
					ActiveInputManager.currentControlType = ActiveInputManager.InputType.Keyboard;
				}
			}
			if (player.controllers.hasMouse)
			{
				this.mChangeTime = player.controllers.Mouse.GetLastTimeAnyButtonChanged();
				if (this.mChangeTime > this.jChangeTime && this.mChangeTime > this.tChangeTime)
				{
					ActiveInputManager.currentControlType = ActiveInputManager.InputType.Keyboard;
				}
			}
		}
		if (player.controllers.joystickCount > 0 && ActiveInputManager.currentControlType != ActiveInputManager.InputType.Joystick)
		{
			Rewired.Controller lastActiveController = player.controllers.GetLastActiveController(ControllerType.Joystick);
			if (lastActiveController != null)
			{
				this.jChangeTime = lastActiveController.GetLastTimeAnyElementChanged();
				if (this.jChangeTime > this.mChangeTime && this.jChangeTime > this.kChangeTime && this.jChangeTime > this.tChangeTime)
				{
					ActiveInputManager.currentControlType = ActiveInputManager.InputType.Joystick;
				}
			}
		}
		if (inputType != ActiveInputManager.currentControlType)
		{
			if (ActiveInputManager.CurrentInputSourceChanged != null)
			{
				ActiveInputManager.CurrentInputSourceChanged();
			}
			if (DestroyableSingleton<HudManager>.InstanceExists)
			{
				if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
				{
					if (!(DestroyableSingleton<HudManager>.Instance.joystick is ConsoleJoystick))
					{
						DestroyableSingleton<HudManager>.Instance.SetTouchType(ControlTypes.Controller);
						return;
					}
				}
				else if (DestroyableSingleton<HudManager>.Instance.joystick is ConsoleJoystick)
				{
					DestroyableSingleton<HudManager>.Instance.SetTouchType(SaveManager.ControlMode);
				}
			}
		}
	}

	public enum InputType
	{
		Joystick,
		Keyboard,
		Touch
	}
}
