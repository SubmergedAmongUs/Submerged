using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PassiveButtonManager : DestroyableSingleton<PassiveButtonManager>
{
	public List<PassiveUiElement> Buttons = new List<PassiveUiElement>();

	private List<IFocusHolder> FocusHolders = new List<IFocusHolder>();

	private PassiveUiElement currentOver;

	public Controller controller = new Controller();

	private PassiveButtonManager.ButtonStates currentState;

	private Collider2D[] results = new Collider2D[40];

	public void RegisterOne(PassiveUiElement button)
	{
		this.Buttons.Add(button);
	}

	public void RemoveOne(PassiveUiElement passiveButton)
	{
		this.Buttons.Remove(passiveButton);
	}

	public void RegisterOne(IFocusHolder focusHolder)
	{
		this.FocusHolders.Add(focusHolder);
	}

	public void RemoveOne(IFocusHolder focusHolder)
	{
		this.FocusHolders.Remove(focusHolder);
	}

	public void Update()
	{
		if (!Application.isFocused)
		{
			return;
		}
		this.controller.Update();
		for (int i = 1; i < this.Buttons.Count; i++)
		{
			if (PassiveButtonManager.DepthComparer.Instance.Compare(this.Buttons[i - 1], this.Buttons[i]) > 0)
			{
				this.Buttons.Sort(PassiveButtonManager.DepthComparer.Instance);
				break;
			}
		}
		this.HandleMouseOut();
		for (int j = 0; j < this.Buttons.Count; j++)
		{
			PassiveUiElement passiveUiElement = this.Buttons[j];
			if (!passiveUiElement)
			{
				this.Buttons.RemoveAt(j);
				j--;
			}
			else if (passiveUiElement.isActiveAndEnabled)
			{
				if (passiveUiElement.ClickMask)
				{
					Vector2 position = this.controller.GetTouch(0).Position;
					if (!passiveUiElement.ClickMask.OverlapPoint(position))
					{
						goto IL_212;
					}
				}
				for (int k = 0; k < passiveUiElement.Colliders.Length; k++)
				{
					Collider2D col = passiveUiElement.Colliders[k];
					if (col && col.isActiveAndEnabled)
					{
						this.HandleMouseOver(passiveUiElement, col);
						switch (this.controller.CheckDrag(col))
						{
						case DragState.TouchStart:
							if (passiveUiElement.HandleDown)
							{
								passiveUiElement.ReceiveClickDown();
							}
							break;
						case DragState.Holding:
							if (passiveUiElement.HandleRepeat)
							{
								passiveUiElement.ReceiveRepeatDown();
							}
							break;
						case DragState.Dragging:
							if (passiveUiElement.HandleDrag)
							{
								Vector2 dragDelta = this.controller.DragPosition - this.controller.DragStartPosition;
								passiveUiElement.ReceiveClickDrag(dragDelta);
								this.controller.ResetDragPosition();
							}
							else if (passiveUiElement.HandleRepeat)
							{
								passiveUiElement.ReceiveRepeatDown();
							}
							else if (this.Buttons.Any((PassiveUiElement b2) => b2.HandleDrag && b2.isActiveAndEnabled && b2.transform.position.z > col.transform.position.z))
							{
								this.controller.ClearTouch();
							}
							break;
						case DragState.Released:
							if (passiveUiElement.HandleUp)
							{
								passiveUiElement.ReceiveClickUp();
							}
							break;
						}
					}
				}
			}
			IL_212:;
		}
		if (this.controller.AnyTouchDown)
		{
			Vector2 touch = this.GetTouch(true);
			this.HandleFocus(touch);
		}
	}

	private void HandleFocus(Vector2 pt)
	{
		bool flag = false;
		Func<Collider2D, bool> func = (Collider2D c) => c.OverlapPoint(pt);
		for (int i = 0; i < this.FocusHolders.Count; i++)
		{
			IFocusHolder focusHolder = this.FocusHolders[i];
			if (!(focusHolder as MonoBehaviour))
			{
				this.FocusHolders.RemoveAt(i);
				i--;
			}
			else if (focusHolder.CheckCollision(pt))
			{
				float depth = (focusHolder as MonoBehaviour).transform.position.z;
				if (!this.Buttons.Any(delegate(PassiveUiElement top)
				{
					if (top.transform.position.z < depth)
					{
						IEnumerable<Collider2D> colliders = top.Colliders;
						Func<Collider2D, bool> predicate;
						if ((predicate = func) == null)
						{
							predicate = (func = ((Collider2D c) => c.OverlapPoint(pt)));
						}
						return colliders.Any(predicate);
					}
					return false;
				}))
				{
					flag = true;
					focusHolder.GiveFocus();
					for (int j = 0; j < this.FocusHolders.Count; j++)
					{
						if (j != i)
						{
							this.FocusHolders[j].LoseFocus();
						}
					}
					break;
				}
				break;
			}
		}
		if (!flag)
		{
			for (int k = 0; k < this.FocusHolders.Count; k++)
			{
				this.FocusHolders[k].LoseFocus();
			}
		}
	}

	public void LoseFocusForAll()
	{
		for (int i = 0; i < this.FocusHolders.Count; i++)
		{
			this.FocusHolders[i].LoseFocus();
		}
	}

	private void HandleMouseOut()
	{
		if (this.currentOver)
		{
			bool flag = false;
			for (int i = 0; i < this.controller.Touches.Length; i++)
			{
				Controller.TouchState pt = this.controller.GetTouch(i);
				if (pt.active && this.currentOver.Colliders.Any((Collider2D c) => c.OverlapPoint(pt.Position)))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				this.currentOver.ReceiveMouseOut();
				this.currentOver = null;
			}
		}
	}

	private void HandleMouseOver(PassiveUiElement button, Collider2D col)
	{
		if (!button.HandleOverOut || button == this.currentOver)
		{
			return;
		}
		if (button.ClickMask)
		{
			Vector2 position = this.controller.GetTouch(0).Position;
			if (!button.ClickMask.OverlapPoint(position))
			{
				return;
			}
		}
		if (this.currentOver && button.transform.position.z > this.currentOver.transform.position.z)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < this.controller.Touches.Length; i++)
		{
			if (this.controller.Touches[i].active && col.OverlapPoint(this.controller.GetTouch(i).Position))
			{
				flag = true;
			}
		}
		if (flag)
		{
			if (this.currentOver && this.currentOver != button)
			{
				this.currentOver.ReceiveMouseOut();
			}
			this.currentOver = button;
			this.currentOver.ReceiveMouseOver();
			return;
		}
	}

	private Vector2 GetTouch(bool getDownTouch)
	{
		if (getDownTouch)
		{
			for (int i = 0; i < this.controller.Touches.Length; i++)
			{
				if (this.controller.Touches[i].TouchStart)
				{
					return this.controller.Touches[i].Position;
				}
			}
		}
		else
		{
			for (int j = 0; j < this.controller.Touches.Length; j++)
			{
				if (this.controller.Touches[j].TouchEnd)
				{
					return this.controller.Touches[j].Position;
				}
			}
		}
		return new Vector2(-5000f, -5000f);
	}

	private enum ButtonStates
	{
		Up,
		Down,
		Drag
	}

	private class DepthComparer : IComparer<MonoBehaviour>
	{
		public static readonly PassiveButtonManager.DepthComparer Instance = new PassiveButtonManager.DepthComparer();

		public int Compare(MonoBehaviour x, MonoBehaviour y)
		{
			if (x == null)
			{
				return 1;
			}
			if (y == null)
			{
				return -1;
			}
			return x.transform.position.z.CompareTo(y.transform.position.z);
		}
	}
}
