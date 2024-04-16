using System;
using System.Collections;
using UnityEngine;

public class ShadowCollab : MonoBehaviour
{
	public Camera ShadowCamera;

	public MeshRenderer ShadowQuad;

	private float oldAspect;

	public void OnEnable()
	{
		base.StartCoroutine(this.Run());
	}

	private IEnumerator Run()
	{
		Camera cam = Camera.main;
		for (;;)
		{
			if (this.oldAspect != cam.aspect)
			{
				this.oldAspect = cam.aspect;
				this.ShadowCamera.aspect = cam.aspect;
				this.ShadowCamera.orthographicSize = cam.orthographicSize;
				this.ShadowQuad.transform.localScale = new Vector3(cam.orthographicSize * cam.aspect, cam.orthographicSize) * 2f;
			}
			yield return Effects.Wait(1f);
		}
		yield break;
	}
}
