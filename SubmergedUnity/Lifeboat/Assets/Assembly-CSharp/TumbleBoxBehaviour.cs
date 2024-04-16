using System;
using UnityEngine;

public class TumbleBoxBehaviour : MonoBehaviour
{
	public FloatRange BoxHeight;

	public FloatRange shadowScale;

	public SpriteRenderer Shadow;

	public SpriteRenderer Box;

	public void FixedUpdate()
	{
		float num = Time.time * 15f;
		float v = Mathf.Cos(Time.time * 3.1415927f / 10f) / 2f + 0.5f;
		float num2 = this.shadowScale.Lerp(v);
		this.Shadow.transform.localScale = new Vector3(num2, num2, num2);
		float num3 = this.BoxHeight.Lerp(v);
		this.Box.transform.localPosition = new Vector3(0f, num3, -0.01f);
		this.Box.transform.eulerAngles = new Vector3(0f, 0f, num);
	}
}
