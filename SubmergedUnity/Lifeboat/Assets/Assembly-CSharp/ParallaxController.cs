using System;
using UnityEngine;

public class ParallaxController : MonoBehaviour
{
	public ParallaxChild[] Children;

	public float Scale = 1f;

	public void Start()
	{
		this.Children = base.GetComponentsInChildren<ParallaxChild>();
	}

	public void SetParallax(float x)
	{
		for (int i = 0; i < this.Children.Length; i++)
		{
			ParallaxChild parallaxChild = this.Children[i];
			Vector3 basePosition = parallaxChild.BasePosition;
			float scale = this.Scale;
			if (basePosition.z >= 0f)
			{
				basePosition.x += x / (basePosition.z * this.Scale + 1f);
			}
			else
			{
				basePosition.x += x * (-basePosition.z * this.Scale + 1f);
			}
			parallaxChild.transform.localPosition = basePosition;
		}
	}
}
