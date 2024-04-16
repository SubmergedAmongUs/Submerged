using System;
using System.Collections;
using UnityEngine;

public class CrystalBehaviour : UiElement
{
	public Transform TargetPosition;

	public SpriteRenderer Renderer;

	public BoxCollider2D Collider;

	public bool CanMove = true;

	public FloatRange Padding;

	private const float Speed = 15f;

	public float XFloatMag = 0.01f;

	private const float FloatMag = 0.05f;

	private const float FloatSpeed = 0.35f;

	public float PieceIndex;

	private void Update()
	{
		if (!this.TargetPosition)
		{
			return;
		}
		Vector3 localPosition = base.transform.localPosition;
		if ((double)Vector2.Distance(this.TargetPosition.localPosition, base.transform.localPosition) > 0.01)
		{
			localPosition = Vector3.Lerp(this.TargetPosition.localPosition, base.transform.localPosition, Time.deltaTime);
		}
		float num = Time.time * 0.35f;
		localPosition.x += (Mathf.PerlinNoise(num, this.PieceIndex * 100f) * 2f - 1f) * this.XFloatMag;
		localPosition.y += (Mathf.PerlinNoise(this.PieceIndex * 100f, num) * 2f - 1f) * 0.05f;
		base.transform.localPosition = localPosition;
	}

	public void Flash(float delay = 0f)
	{
		base.StopAllCoroutines();
		base.StartCoroutine(CrystalBehaviour.Flash(this, delay));
	}

	private static IEnumerator Flash(CrystalBehaviour c, float delay)
	{
		for (float time = 0f; time < delay; time += Time.deltaTime)
		{
			yield return null;
		}
		Color col = Color.clear;
		for (float time = 0f; time < 0.1f; time += Time.deltaTime)
		{
			float num = time / 0.1f;
			col.r = (col.g = (col.b = Mathf.Lerp(0f, 1f, num)));
			c.Renderer.material.SetColor("_AddColor", col);
			yield return null;
		}
		for (float time = 0f; time < 0.1f; time += Time.deltaTime)
		{
			float num2 = time / 0.1f;
			col.r = (col.g = (col.b = Mathf.Lerp(1f, 0f, num2)));
			c.Renderer.material.SetColor("_AddColor", col);
			yield return null;
		}
		col.r = (col.g = (col.b = 0f));
		c.Renderer.material.SetColor("_AddColor", col);
		yield break;
	}
}
