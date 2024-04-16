using System;
using UnityEngine;

public class VirtualJoystick : MonoBehaviour, IVirtualJoystick
{
	public float InnerRadius = 0.64f;

	public float OuterRadius = 1.28f;

	public CircleCollider2D Outer;

	public SpriteRenderer Inner;

	private Controller myController = new Controller();

	public Vector2 Delta { get; private set; }

	protected virtual void FixedUpdate()
	{
		this.myController.Update();
		switch (this.myController.CheckDrag(this.Outer))
		{
		case DragState.TouchStart:
		case DragState.Dragging:
		{
			float num = this.OuterRadius - this.InnerRadius;
			Vector2 vector = this.myController.DragPosition - (Vector2) base.transform.position;
			float magnitude = vector.magnitude;
			Vector2 vector2 = new Vector2(Mathf.Sqrt(Mathf.Abs(vector.x)) * Mathf.Sign(vector.x), Mathf.Sqrt(Mathf.Abs(vector.y)) * Mathf.Sign(vector.y));
			this.Delta = Vector2.ClampMagnitude(vector2 / this.OuterRadius, 1f);
			this.Inner.transform.localPosition = Vector3.ClampMagnitude(vector, num) + Vector3.back;
			return;
		}
		case DragState.Holding:
			break;
		case DragState.Released:
			this.Delta = Vector2.zero;
			this.Inner.transform.localPosition = Vector3.back;
			break;
		default:
			return;
		}
	}

	public virtual void UpdateJoystick(FingerBehaviour finger, Vector2 velocity, bool syncFinger)
	{
		Vector3 vector = this.Inner.transform.localPosition;
		Vector3 vector2 = velocity.normalized * this.InnerRadius;
		vector2.z = vector.z;
		if (syncFinger)
		{
			vector = Vector3.Lerp(vector, vector2, Time.fixedDeltaTime * 5f);
			this.Inner.transform.localPosition = vector;
			vector = this.Inner.transform.position;
			vector.z = -26f;
			finger.transform.position = vector;
			return;
		}
		if (this.Inner.gameObject != finger.gameObject)
		{
			this.Inner.transform.localPosition = vector2;
		}
	}
}
