using System;
using System.Linq;
using UnityEngine;

public class Scroller : PassiveUiElement
{
	public Transform Inner;

	public bool allowX;

	public FloatRange XBounds = new FloatRange(-10f, 10f);

	public bool allowY;

	public FloatRange YBounds = new FloatRange(-10f, 10f);

	public FloatRange ScrollerYRange;

	public SpriteRenderer ScrollerY;

	private Vector2 velocity;

	private bool active;

	public override bool HandleUp
	{
		get
		{
			return true;
		}
	}

	public override bool HandleDown
	{
		get
		{
			return true;
		}
	}

	public override bool HandleDrag
	{
		get
		{
			return true;
		}
	}

	public override bool HandleOverOut
	{
		get
		{
			return false;
		}
	}

	public bool AtTop
	{
		get
		{
			return this.Inner.localPosition.y <= this.YBounds.min + 0.25f;
		}
	}

	public bool AtBottom
	{
		get
		{
			return this.Inner.localPosition.y >= this.YBounds.max - 0.25f;
		}
	}

	public Collider2D Hitbox
	{
		get
		{
			return this.Colliders[0];
		}
	}

	public void FixedUpdate()
	{
		if (!this.Inner)
		{
			return;
		}
		Vector2 mouseScrollDelta = Input.mouseScrollDelta;
		if (mouseScrollDelta.y != 0f)
		{
			mouseScrollDelta.y = -mouseScrollDelta.y;
			this.ScrollRelative(mouseScrollDelta);
		}
	}

	public void Update()
	{
		if (!this.active && this.velocity.sqrMagnitude > 0.01f)
		{
			this.velocity = Vector2.ClampMagnitude(this.velocity, this.velocity.magnitude - 10f * Time.deltaTime);
			this.ScrollRelative(this.velocity * Time.deltaTime);
		}
	}

	public void ScrollDown()
	{
		Collider2D collider2D = this.Colliders.First<Collider2D>();
		float num = collider2D.bounds.max.y - collider2D.bounds.min.y;
		this.ScrollRelative(new Vector2(0f, num * 0.75f));
	}

	public void ScrollUp()
	{
		Collider2D collider2D = this.Colliders.First<Collider2D>();
		float num = collider2D.bounds.max.y - collider2D.bounds.min.y;
		this.ScrollRelative(new Vector2(0f, num * -0.75f));
	}

	public float GetScrollPercY()
	{
		if ((double)this.YBounds.Width < 0.0001)
		{
			return 1f;
		}
		Vector3 localPosition = this.Inner.transform.localPosition;
		return this.YBounds.ReverseLerp(localPosition.y);
	}

	public void ScrollPercentY(float p)
	{
		Vector3 localPosition = this.Inner.transform.localPosition;
		localPosition.y = this.YBounds.Lerp(p);
		this.Inner.transform.localPosition = localPosition;
		this.UpdateScrollBars(localPosition);
	}

	public override void ReceiveClickDown()
	{
		this.active = true;
	}

	public override void ReceiveClickUp()
	{
		this.active = false;
	}

	public override void ReceiveClickDrag(Vector2 dragDelta)
	{
		this.velocity = dragDelta / Time.deltaTime * 0.9f;
		this.ScrollRelative(dragDelta);
	}

	public void ScrollRelative(Vector2 dragDelta)
	{
		if (dragDelta.magnitude < 0.05f)
		{
			return;
		}
		if (!this.allowX)
		{
			dragDelta.x = 0f;
		}
		if (!this.allowY)
		{
			dragDelta.y = 0f;
		}
		Vector3 vector = this.Inner.transform.localPosition + (Vector3) dragDelta;
		vector.x = this.XBounds.Clamp(vector.x);
		int childCount = this.Inner.transform.childCount;
		float num = Mathf.Max(this.YBounds.min, this.YBounds.max);
		vector.y = Mathf.Clamp(vector.y, this.YBounds.min, num);
		this.Inner.transform.localPosition = vector;
		this.UpdateScrollBars(vector);
	}

	private void UpdateScrollBars(Vector3 pos)
	{
		if (this.ScrollerY)
		{
			if (this.YBounds.min == this.YBounds.max)
			{
				this.ScrollerY.enabled = false;
				return;
			}
			this.ScrollerY.enabled = true;
			float num = this.YBounds.ReverseLerp(pos.y);
			Vector3 localPosition = this.ScrollerY.transform.localPosition;
			localPosition.y = this.ScrollerYRange.Lerp(1f - num);
			this.ScrollerY.transform.localPosition = localPosition;
		}
	}
}
