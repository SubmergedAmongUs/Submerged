using System;
using TMPro;
using UnityEngine;

public class TaskPanelBehaviour : DestroyableSingleton<TaskPanelBehaviour>
{
	public Vector3 OpenPosition;

	public Vector3 ClosedPosition;

	public SpriteRenderer background;

	public SpriteRenderer tab;

	public TextMeshPro TaskText;

	public bool open;

	private float timer;

	public float Duration;

	private void Update()
	{
		this.background.transform.localScale = ((this.TaskText.textBounds.size.x > 0f) ? new Vector3(this.TaskText.textBounds.size.x + 0.2f, this.TaskText.textBounds.size.y + 0.2f, 1f) : Vector3.zero);
		Vector3 vector = this.background.sprite.bounds.extents;
		vector.y = -vector.y;
		vector = vector.Mul(this.background.transform.localScale);
		this.background.transform.localPosition = vector;
		Vector3 vector2 = this.tab.sprite.bounds.extents;
		vector2 = vector2.Mul(this.tab.transform.localScale);
		vector2.y = -vector2.y;
		vector2.x += vector.x * 2f;
		this.tab.transform.localPosition = vector2;
		this.ClosedPosition.y = (this.OpenPosition.y = 0.6f);
		this.ClosedPosition.x = -this.background.sprite.bounds.size.x * this.background.transform.localScale.x;
		if (this.open)
		{
			this.timer = Mathf.Min(1f, this.timer + Time.deltaTime / this.Duration);
		}
		else
		{
			this.timer = Mathf.Max(0f, this.timer - Time.deltaTime / this.Duration);
		}
		Vector3 relativePos = new Vector3(Mathf.SmoothStep(this.ClosedPosition.x, this.OpenPosition.x, this.timer), Mathf.SmoothStep(this.ClosedPosition.y, this.OpenPosition.y, this.timer), this.OpenPosition.z);
		base.transform.localPosition = AspectPosition.ComputePosition(AspectPosition.EdgeAlignments.LeftTop, relativePos);
	}

	public void ToggleOpen()
	{
		this.open = !this.open;
	}
}
