using System;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class SpriteParticleSystem : MonoBehaviour
{
	public Sprite sprite;

	public ParticleSystemRenderer ren;

	private MaterialPropertyBlock block;

	private void OnEnable()
	{
		this.block = new MaterialPropertyBlock();
		this.ren = (base.GetComponent<Renderer>() as ParticleSystemRenderer);
		this.SetPropertyBlock();
	}

	private void SetPropertyBlock()
	{
		if (this.block == null)
		{
			this.block = new MaterialPropertyBlock();
		}
		this.ren.GetPropertyBlock(this.block);
		this.block.SetTexture("_MainTex", this.sprite.texture);
		this.ren.SetPropertyBlock(this.block);
	}

	private void OnValidate()
	{
		this.SetPropertyBlock();
	}
}
