using System;
using UnityEngine;

public abstract class PassiveUiElement : UiElement
{
	public Collider2D ClickMask;

	public Collider2D[] Colliders;

	public virtual bool HandleUp
	{
		get
		{
			return false;
		}
	}

	public virtual bool HandleDown
	{
		get
		{
			return false;
		}
	}

	public virtual bool HandleRepeat
	{
		get
		{
			return false;
		}
	}

	public virtual bool HandleDrag
	{
		get
		{
			return false;
		}
	}

	public virtual bool HandleOverOut
	{
		get
		{
			return true;
		}
	}

	public void Start()
	{
		DestroyableSingleton<PassiveButtonManager>.Instance.RegisterOne(this);
		if (this.Colliders == null || this.Colliders.Length == 0)
		{
			this.Colliders = base.GetComponents<Collider2D>();
		}
	}

	public void OnDestroy()
	{
		if (DestroyableSingleton<PassiveButtonManager>.InstanceExists)
		{
			DestroyableSingleton<PassiveButtonManager>.Instance.RemoveOne(this);
		}
	}

	public virtual void ReceiveClickDown()
	{
	}

	public virtual void ReceiveRepeatDown()
	{
	}

	public virtual void ReceiveClickUp()
	{
	}

	public virtual void ReceiveClickDrag(Vector2 dragDelta)
	{
	}
}
