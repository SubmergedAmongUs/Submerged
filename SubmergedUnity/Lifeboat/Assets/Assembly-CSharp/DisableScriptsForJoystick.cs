using System;
using UnityEngine;

public class DisableScriptsForJoystick : MonoBehaviour
{
	public MonoBehaviour[] scripts;

	private void Start()
	{
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Combine(ActiveInputManager.CurrentInputSourceChanged, new Action(this.OnInputMethodChanged));
		this.OnInputMethodChanged();
	}

	private void OnDestroy()
	{
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Remove(ActiveInputManager.CurrentInputSourceChanged, new Action(this.OnInputMethodChanged));
	}

	private void OnInputMethodChanged()
	{
		MonoBehaviour[] array = this.scripts;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = (ActiveInputManager.currentControlType > ActiveInputManager.InputType.Joystick);
		}
	}
}
