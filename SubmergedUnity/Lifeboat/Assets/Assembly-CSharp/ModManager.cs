using System;
using Beebyte.Obfuscator;
using UnityEngine;

[Skip]
public class ModManager : DestroyableSingleton<ModManager>
{
	public SpriteRenderer ModStamp;

	public Camera localCamera;

	public void ShowModStamp()
	{
		this.ModStamp.enabled = true;
	}

	private void LateUpdate()
	{
		if (!this.ModStamp.enabled)
		{
			return;
		}
		if (!this.localCamera)
		{
			if (DestroyableSingleton<HudManager>.InstanceExists)
			{
				this.localCamera = DestroyableSingleton<HudManager>.Instance.GetComponentInChildren<Camera>();
			}
			else
			{
				this.localCamera = Camera.main;
			}
		}
		this.ModStamp.transform.position = AspectPosition.ComputeWorldPosition(this.localCamera, AspectPosition.EdgeAlignments.RightTop, new Vector3(0.6f, 0.6f, this.localCamera.nearClipPlane + 0.1f));
	}
}
