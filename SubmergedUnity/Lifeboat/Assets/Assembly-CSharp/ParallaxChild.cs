using System;
using UnityEngine;

public class ParallaxChild : MonoBehaviour
{
	[HideInInspector]
	public Vector3 BasePosition;

	public void OnEnable()
	{
		this.BasePosition = base.transform.localPosition;
	}
}
