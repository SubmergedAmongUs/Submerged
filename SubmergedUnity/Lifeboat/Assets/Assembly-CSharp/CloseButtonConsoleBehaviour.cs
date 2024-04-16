using System;
using UnityEngine;

public class CloseButtonConsoleBehaviour : MonoBehaviour
{
	public bool keepActiveOnConsoles;

	private void Start()
	{
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Combine(ActiveInputManager.CurrentInputSourceChanged, new Action(this.OnInputMethodChanged));
		this.OnInputMethodChanged();
	}

	private void SetActive(bool active)
	{
		base.gameObject.SetActive(active || this.keepActiveOnConsoles);
	}

	private void OnDestroy()
	{
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Remove(ActiveInputManager.CurrentInputSourceChanged, new Action(this.OnInputMethodChanged));
	}

	private void OnInputMethodChanged()
	{
		this.SetActive(ActiveInputManager.currentControlType > ActiveInputManager.InputType.Joystick);
	}
}
