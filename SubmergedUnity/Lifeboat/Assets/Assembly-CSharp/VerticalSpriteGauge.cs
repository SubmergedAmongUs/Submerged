using System;
using UnityEngine;

public class VerticalSpriteGauge : MonoBehaviour
{
	public float Value = 0.5f;

	public float MaxValue = 1f;

	public FloatRange YRange;

	public SpriteRenderer Mask;

	private float lastValue = float.MinValue;

	public float TopY { get; private set; }

	public void Update()
	{
		if (this.MaxValue != 0f && this.lastValue != this.Value)
		{
			this.lastValue = this.Value;
			Vector3 localPosition = this.Mask.transform.localPosition;
			this.YRange.Lerp(this.lastValue / this.MaxValue);
			Vector3 localScale = this.Mask.transform.localScale;
			localScale.y = this.lastValue / this.MaxValue * this.YRange.Width;
			this.Mask.transform.localScale = localScale;
			localPosition.y = this.YRange.min + localScale.y / 2f;
			this.Mask.transform.localPosition = localPosition;
			this.TopY = this.YRange.min + localScale.y;
		}
	}
}
