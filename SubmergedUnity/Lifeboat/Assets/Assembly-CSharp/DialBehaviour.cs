using System;
using UnityEngine;

public class DialBehaviour : MonoBehaviour
{
	public FloatRange DialRange;

	public Collider2D collider;

	public Controller myController = new Controller();

	public float Value;

	public bool Engaged;

	public Transform DialTrans;

	public Transform DialShadTrans;

	public void Update()
	{
		this.Engaged = false;
		this.myController.Update();
		if (this.myController.CheckDrag(this.collider) == DragState.Dragging)
		{
			Vector2 vector = this.myController.DragPosition - (Vector2) base.transform.position;
			float num = Vector2.up.AngleSigned(vector);
			if (num < -180f)
			{
				num += 360f;
			}
			num = this.DialRange.Clamp(num);
			this.SetValue(num);
			this.Engaged = true;
		}
	}

	public void SetValue(float angle)
	{
		this.Value = angle;
		Vector3 localEulerAngles = new Vector3(0f, 0f, angle);
		this.DialTrans.localEulerAngles = localEulerAngles;
		this.DialShadTrans.localEulerAngles = localEulerAngles;
	}
}
