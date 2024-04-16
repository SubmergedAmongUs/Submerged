using System;
using UnityEngine;

public class AspectSize : MonoBehaviour
{
	public Sprite Background;

	public SpriteRenderer Renderer;

	public float PercentWidth = 0.95f;

	public void OnEnable()
	{
		Camera main = Camera.main;
		float num = main.orthographicSize * main.aspect;
		Vector3 vector;
		if (this.Background)
		{
			vector = this.Background.bounds.size;
		}
		else
		{
			if (this.Renderer.drawMode != null)
			{
				vector = this.Renderer.size;
			}
			else
			{
				vector = this.Renderer.sprite.bounds.size;
			}
			vector = this.Renderer.transform.TransformVector(vector);
		}
		float num2 = vector.x / 2f;
		float num3 = num / num2 * this.PercentWidth;
		if (num3 < 1f)
		{
			base.transform.localScale = new Vector3(num3, num3, num3);
		}
	}

	public static float CalculateSize(Vector3 parentPos, Sprite sprite)
	{
		Camera main = Camera.main;
		float num = main.orthographicSize * main.aspect + parentPos.x;
		float x = sprite.bounds.size.x;
		float num2 = num / x * 0.98f;
		return Mathf.Min(1f, num2);
	}
}
