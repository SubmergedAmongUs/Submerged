using System;
using UnityEngine;

public class CrossFadeImages : MonoBehaviour
{
	public SpriteRenderer Image1;

	public SpriteRenderer Image2;

	public float Period = 5f;

	private void Update()
	{
		Color white = Color.white;
		white.a = Mathf.Clamp((Mathf.Sin(3.1415927f * Time.time / this.Period) + 0.75f) * 0.75f, 0f, 1f);
		this.Image1.color = white;
		white.a = 1f - white.a;
		this.Image2.color = white;
	}
}
