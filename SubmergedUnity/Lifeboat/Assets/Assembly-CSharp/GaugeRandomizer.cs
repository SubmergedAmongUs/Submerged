using System;
using UnityEngine;

public class GaugeRandomizer : MonoBehaviour
{
	public FloatRange Range;

	public SpriteRenderer Gauge;

	public float Frequency = 1f;

	public float Offset = 1f;

	private float naturalY;

	private float naturalSizeY;

	private Color goodLineColor = new Color(100f, 193f, 255f);

	public void Start()
	{
		this.naturalSizeY = this.Gauge.size.y;
		this.naturalY = base.transform.localPosition.y;
	}

	private void Update()
	{
		float num = this.Range.Lerp(Mathf.PerlinNoise(this.Offset, Time.time * this.Frequency) / 2f + 0.5f);
		Vector2 size = this.Gauge.size;
		size.y = num;
		this.Gauge.size = size;
		Vector3 localPosition = base.transform.localPosition;
		localPosition.y = this.naturalY - (this.naturalSizeY - num) / 2f;
		base.transform.localPosition = localPosition;
	}
}
