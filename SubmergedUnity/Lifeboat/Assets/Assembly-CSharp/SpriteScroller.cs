using System;
using UnityEngine;

public class SpriteScroller : MonoBehaviour
{
	public Renderer rend;

	public float XRate = 1f;

	public float YRate = 1f;

	private void Update()
	{
		if (this.rend)
		{
			this.rend.material.SetTextureOffset("_MainTex", new Vector2(Time.time * this.XRate, Time.time * this.YRate));
		}
	}
}
