using System;
using UnityEngine;

public class ShadowCamera : MonoBehaviour
{
	public Shader Shadozer;

	public void OnEnable()
	{
		base.GetComponent<Camera>().SetReplacementShader(this.Shadozer, "RenderType");
	}

	public void OnDisable()
	{
		base.GetComponent<Camera>().ResetReplacementShader();
	}
}
