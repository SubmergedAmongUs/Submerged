using System;
using Rewired;
using Rewired.ControllerExtensions;
using UnityEngine;

public class TouchpadBehavior : MonoBehaviour
{
	private float aspect = (float)Screen.width / (float)Screen.height;

	private bool touching;

	private bool firstTouch;

	private Vector2 toCenter = new Vector2(0.5f, 0.5f);

	private Vector2 firstTouchPos;

	private Vector2 delta;

	private Vector2 fromCenter;

	private IDualShock4Extension ds4;

	public float touchpadSensitivity = 3f;

	private void Start()
	{
		this.GetExtension();
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Combine(ActiveInputManager.CurrentInputSourceChanged, new Action(this.GetExtension));
	}

	private void OnDestroy()
	{
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Remove(ActiveInputManager.CurrentInputSourceChanged, new Action(this.GetExtension));
	}

	private void GetExtension()
	{
		Player player = ReInput.players.GetPlayer(0);
		this.ds4 = null;
		if (player != null && player.controllers.joystickCount > 0)
		{
			this.ds4 = player.controllers.Joysticks[0].GetExtension<IDualShock4Extension>();
		}
	}

	private void Update()
	{
		if (this.ds4 != null && this.ds4.touchCount > 0)
		{
			Vector2 vector;
			this.ds4.GetTouchPosition(0, out vector);
			if (!this.touching)
			{
				this.touching = true;
				this.firstTouch = true;
				this.firstTouchPos = vector;
			}
			else
			{
				this.firstTouch = false;
			}
			this.delta = vector - this.firstTouchPos;
			this.delta.x = this.delta.x * (this.touchpadSensitivity * this.aspect);
			this.delta.y = this.delta.y * this.touchpadSensitivity;
			this.fromCenter = vector - this.toCenter;
			this.fromCenter.x = this.fromCenter.x * (this.touchpadSensitivity * this.aspect);
			this.fromCenter.y = this.fromCenter.y * this.touchpadSensitivity;
			return;
		}
		this.touching = false;
	}

	public bool IsTouching()
	{
		return this.touching;
	}

	public bool IsFirstTouch()
	{
		return this.firstTouch;
	}

	public void ResetTouch()
	{
		this.touching = false;
		this.firstTouch = false;
	}

	public Vector2 GetTouchVector()
	{
		if (this.touching)
		{
			return this.delta;
		}
		return Vector2.zero;
	}

	public Vector2 GetCenterToTouch()
	{
		if (this.touching)
		{
			return this.fromCenter;
		}
		return Vector2.zero;
	}
}
