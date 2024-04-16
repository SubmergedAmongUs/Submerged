using System;
using UnityEngine;

public class AutoParallaxController : MonoBehaviour
{
	public ParallaxChild[] Children;

	public float XScale = 1f;

	public float YScale = 1f;

	public void Start()
	{
		this.Children = base.GetComponentsInChildren<ParallaxChild>();
	}

	public void Update()
	{
		if (!PlayerControl.LocalPlayer)
		{
			return;
		}
		Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
		for (int i = 0; i < this.Children.Length; i++)
		{
			ParallaxChild parallaxChild = this.Children[i];
			Vector3 basePosition = parallaxChild.BasePosition;
			if (basePosition.z < 0f)
			{
				basePosition.z = -basePosition.z;
			}
			basePosition.x -= truePosition.x / (basePosition.z * this.XScale);
			basePosition.y -= truePosition.y / (basePosition.z * this.YScale);
			parallaxChild.transform.localPosition = basePosition;
		}
	}
}
