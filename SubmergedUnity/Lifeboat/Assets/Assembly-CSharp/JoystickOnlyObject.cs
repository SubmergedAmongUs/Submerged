using System;
using UnityEngine;

public class JoystickOnlyObject : MonoBehaviour
{
	private void Awake()
	{
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Combine(ActiveInputManager.CurrentInputSourceChanged, new Action(this.UpdateState));
		this.UpdateState();
	}

	private void OnDestroy()
	{
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Remove(ActiveInputManager.CurrentInputSourceChanged, new Action(this.UpdateState));
	}

	private void UpdateState()
	{
		base.gameObject.SetActive(ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick);
	}
}
