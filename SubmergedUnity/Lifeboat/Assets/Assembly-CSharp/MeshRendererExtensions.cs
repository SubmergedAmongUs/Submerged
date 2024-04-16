using System;
using UnityEngine;

public static class MeshRendererExtensions
{
	public static void SetSprite(this MeshRenderer self, Texture2D spr)
	{
		if (spr != null)
		{
			self.SetCutout(spr);
			self.material.color = Color.white;
			return;
		}
		self.SetCutout(null);
		self.material.color = Color.clear;
	}

	public static void SetCutout(this MeshRenderer self, Texture2D txt)
	{
		self.material.SetTexture("_MainTex", txt);
		self.material.SetTexture("_EmissionMap", txt);
	}
}
