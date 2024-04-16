using System;
using Rewired;
using UnityEngine;

public class Controller
{
	private const int maxTouchCount = 4;

	private const int mainTouchIndex = 0;

	public readonly Controller.TouchState[] Touches = new Controller.TouchState[4];

	private Collider2D amTouching;

	private int touchId = -1;

	private static Vector3 oldMousePos = new Vector3(0f, 0f, 0f);

	public static Controller.TouchType currentTouchType
	{
		get
		{
			return (Controller.TouchType)ActiveInputManager.currentControlType;
		}
	}

	public Controller()
	{
		this.Touches = new Controller.TouchState[4];
		for (int i = 0; i < this.Touches.Length; i++)
		{
			this.Touches[i] = new Controller.TouchState();
		}
	}

	public bool CheckHover(Collider2D coll)
	{
		if (!coll)
		{
			return false;
		}
		for (int i = 0; i < this.Touches.Length; i++)
		{
			Controller.TouchState touchState = this.Touches[i];
			if (touchState.active && coll.OverlapPoint(touchState.Position))
			{
				return true;
			}
		}
		return false;
	}

	public Vector2 HoverPosition
	{
		get
		{
			for (int i = this.Touches.Length - 1; i >= 0; i--)
			{
				Controller.TouchState touchState = this.Touches[i];
				if (touchState.active)
				{
					return touchState.Position;
				}
			}
			return Vector2.zero;
		}
	}

	public DragState CheckDrag(Collider2D coll)
	{
		if (!coll)
		{
			return DragState.NoTouch;
		}
		if (this.touchId > -1 && (!this.amTouching || !this.amTouching.isActiveAndEnabled))
		{
			this.touchId = -1;
			this.amTouching = null;
		}
		if (this.touchId <= -1)
		{
			for (int i = 0; i < this.Touches.Length; i++)
			{
				Controller.TouchState touchState = this.Touches[i];
				if (touchState.TouchStart && coll.OverlapPoint(touchState.Position))
				{
					this.amTouching = coll;
					this.touchId = i;
					touchState.dragState = DragState.TouchStart;
					return DragState.TouchStart;
				}
			}
			return DragState.NoTouch;
		}
		if (coll != this.amTouching)
		{
			return DragState.NoTouch;
		}
		Controller.TouchState touchState2 = this.Touches[this.touchId];
		if (!touchState2.IsDown)
		{
			this.amTouching = null;
			this.touchId = -1;
			touchState2.dragState = DragState.Released;
			return DragState.Released;
		}
		if (Vector2.Distance(touchState2.ScreenDownAt, touchState2.ScreenPosition) > 10f || touchState2.dragState == DragState.Dragging)
		{
			touchState2.dragState = DragState.Dragging;
			return DragState.Dragging;
		}
		touchState2.dragState = DragState.Holding;
		return DragState.Holding;
	}

	public bool AnyTouch
	{
		get
		{
			return this.Touches[0].IsDown || this.Touches[1].IsDown;
		}
	}

	public bool AnyTouchDown
	{
		get
		{
			return this.Touches[0].TouchStart || this.Touches[1].TouchStart;
		}
	}

	public bool AnyTouchUp
	{
		get
		{
			return this.Touches[0].TouchEnd || this.Touches[1].TouchEnd;
		}
	}

	public bool FirstDown
	{
		get
		{
			return this.Touches[0].TouchStart;
		}
	}

	public Vector2 DragPosition
	{
		get
		{
			if (this.touchId < 0)
			{
				return Vector2.zero;
			}
			return this.Touches[this.touchId].Position;
		}
	}

	public Vector2 DragStartPosition
	{
		get
		{
			return this.Touches[this.touchId].DownAt;
		}
	}

	public void ResetDragPosition()
	{
		this.Touches[this.touchId].DownAt = this.Touches[this.touchId].Position;
	}

	public void ClearTouch()
	{
		if (this.touchId < 0)
		{
			return;
		}
		Controller.TouchState touchState = this.Touches[this.touchId];
		touchState.dragState = DragState.NoTouch;
		touchState.TouchStart = true;
		this.amTouching = null;
		this.touchId = -1;
	}

	public Camera mainCam { get; set; }

	public void Update()
	{
		if (!this.mainCam)
		{
			this.mainCam = Camera.main;
		}
		if (Input.touchCount > 0)
		{
			DestroyableSingleton<ActiveInputManager>.Instance.SetTouchAsCurrentInput();
		}
		if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Touch)
		{
			for (int i = 0; i < this.Touches.Length; i++)
			{
				Controller.TouchState touchState = this.Touches[i];
				touchState.WasDown = touchState.IsDown;
				touchState.IsDown = false;
				touchState.active = false;
			}
			for (int j = 0; j < Input.touchCount; j++)
			{
				Touch touch = Input.GetTouch(j);
				if (j < this.Touches.Length)
				{
					Controller.TouchState touchState2 = this.Touches[j];
					touchState2.ScreenPosition = touch.position;
					touchState2.Position = this.mainCam.ScreenToWorldPoint(touch.position);
					touchState2.IsDown = true;
					touchState2.active = true;
				}
			}
			for (int k = 0; k < this.Touches.Length; k++)
			{
				Controller.TouchState touchState3 = this.Touches[k];
				touchState3.TouchStart = (!touchState3.WasDown && touchState3.IsDown);
				if (touchState3.TouchStart)
				{
					touchState3.ScreenDownAt = touchState3.ScreenPosition;
					touchState3.DownAt = touchState3.Position;
				}
				touchState3.TouchEnd = (touchState3.WasDown && !touchState3.IsDown);
			}
		}
		Player player = ReInput.players.GetPlayer(0);
		player.controllers.GetLastActiveController();
		if (Controller.currentTouchType == Controller.TouchType.Mouse)
		{
			if (player.controllers.hasMouse)
			{
				Controller.TouchState touchState4 = this.Touches[0];
				bool mouseButton = Input.GetMouseButton(0);
				touchState4.ScreenPosition = Input.mousePosition;
				touchState4.Position = this.mainCam.ScreenToWorldPoint(Input.mousePosition);
				touchState4.TouchStart = (!touchState4.IsDown && mouseButton);
				if (touchState4.TouchStart)
				{
					touchState4.ScreenDownAt = touchState4.ScreenPosition;
					touchState4.DownAt = touchState4.Position;
				}
				touchState4.TouchEnd = (touchState4.IsDown && !mouseButton);
				touchState4.IsDown = mouseButton;
				touchState4.active = true;
			}
			else
			{
				Controller.TouchState touchState5 = this.Touches[0];
				touchState5.TouchStart = false;
				touchState5.TouchEnd = false;
				touchState5.IsDown = false;
				touchState5.active = false;
			}
		}
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			if (VirtualCursor.instance && VirtualCursor.instance.isActiveAndEnabled && SpecialInputHandler.disableVirtualCursorCount == 0)
			{
				Controller.TouchState touchState6 = this.Touches[0];
				bool buttonDown = VirtualCursor.buttonDown;
				touchState6.TouchStart = (!touchState6.IsDown && buttonDown);
				touchState6.Position = VirtualCursor.currentPosition;
				touchState6.ScreenPosition = this.mainCam.WorldToScreenPoint(touchState6.Position);
				if (touchState6.TouchStart)
				{
					touchState6.ScreenDownAt = touchState6.ScreenPosition;
					touchState6.DownAt = touchState6.Position;
				}
				touchState6.TouchEnd = (touchState6.IsDown && !buttonDown);
				touchState6.IsDown = buttonDown;
				touchState6.active = true;
				return;
			}
			Controller.TouchState touchState7 = this.Touches[0];
			touchState7.TouchStart = false;
			touchState7.TouchEnd = false;
			touchState7.IsDown = false;
			touchState7.active = false;
		}
	}

	public void Reset()
	{
		for (int i = 0; i < this.Touches.Length; i++)
		{
			this.Touches[i] = new Controller.TouchState();
		}
		this.touchId = -1;
		this.amTouching = null;
	}

	public Controller.TouchState GetTouch(int i)
	{
		return this.Touches[i];
	}

	public class TouchState
	{
		public Vector2 ScreenDownAt;

		public Vector2 ScreenPosition;

		public Vector2 DownAt;

		public Vector2 Position;

		public bool WasDown;

		public bool IsDown;

		public bool TouchStart;

		public bool TouchEnd;

		public DragState dragState;

		public bool active;
	}

	public enum TouchType
	{
		Joystick,
		Mouse,
		Touch
	}
}
