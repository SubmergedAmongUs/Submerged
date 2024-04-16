using System;
using UnityEngine;

public class ArrowBehaviour : MonoBehaviour
{
	public Vector3 target;

	public float perc = 0.925f;

	[HideInInspector]
	public SpriteRenderer image;

	public void Awake()
	{
		this.image = base.GetComponent<SpriteRenderer>();
	}

	public void Update()
	{
		Camera main = Camera.main;
		Vector2 vector = this.target - main.transform.position;
		float num = vector.magnitude / (main.orthographicSize * this.perc);
		this.image.enabled = ((double)num > 0.3);
		Vector2 vector2 = main.WorldToViewportPoint(this.target);
		if (this.Between(vector2.x, 0f, 1f) && this.Between(vector2.y, 0f, 1f))
		{
			base.transform.position = this.target - (Vector3) vector.normalized * 0.6f;
			float num2 = Mathf.Clamp(num, 0f, 1f);
			base.transform.localScale = new Vector3(num2, num2, num2);
		}
		else
		{
			Vector2 vector3 = new Vector2(Mathf.Clamp(vector2.x * 2f - 1f, -1f, 1f), Mathf.Clamp(vector2.y * 2f - 1f, -1f, 1f));
			float orthographicSize = main.orthographicSize;
			float num3 = main.orthographicSize * main.aspect;
			Vector3 vector4 = new Vector3(Mathf.LerpUnclamped(0f, num3 * 0.88f, vector3.x), Mathf.LerpUnclamped(0f, orthographicSize * 0.79f, vector3.y), 0f);
			base.transform.position = main.transform.position + vector4;
			base.transform.localScale = Vector3.one;
		}
		base.transform.LookAt2d(this.target);
	}

	private bool Between(float value, float min, float max)
	{
		return value > min && value < max;
	}
}
