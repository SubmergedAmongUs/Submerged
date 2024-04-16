using System;
using UnityEngine;

public class FullScreenScaler : MonoBehaviour
{
	public SpriteRenderer OptionalTarget;

	public bool ScaleWidth = true;

	public bool ScaleHeight = true;

	public void OnEnable()
	{
		Camera main = Camera.main;
		float num = main.orthographicSize * 2f;
		float x = num * main.aspect;
		if (this.OptionalTarget && this.OptionalTarget.drawMode != null)
		{
			Vector3 vector = this.OptionalTarget.size;
			if (this.ScaleWidth)
			{
				vector.x = x;
			}
			if (this.ScaleHeight)
			{
				vector.y = num;
			}
			this.OptionalTarget.size = vector;
			return;
		}
		Vector3 localScale = base.transform.localScale;
		if (this.ScaleWidth)
		{
			localScale.x = x;
		}
		if (this.ScaleHeight)
		{
			localScale.y = num;
		}
		base.transform.localScale = localScale;
	}
}
