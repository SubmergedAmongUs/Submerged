using System;
using PowerTools;
using UnityEngine;

public class HatParent : MonoBehaviour
{
	public SpriteRenderer BackLayer;

	public SpriteRenderer FrontLayer;

	public SpriteRenderer Parent;

	public HatBehaviour Hat { get; set; }

	public Color color
	{
		set
		{
			this.BackLayer.color = value;
			this.FrontLayer.color = value;
		}
	}

	public bool flipX
	{
		set
		{
			this.BackLayer.flipX = value;
			this.FrontLayer.flipX = value;
		}
	}

	public void SetHat(HatBehaviour hat, int color)
	{
		this.Hat = hat;
		this.SetHat(color);
	}

	public void SetHat(int color)
	{
		this.SetIdleAnim();
		this.SetColor(color);
	}

	public void SetIdleAnim()
	{
		if (!this.Hat)
		{
			return;
		}
		if (this.Hat.AltShader)
		{
			this.FrontLayer.sharedMaterial = this.Hat.AltShader;
			this.BackLayer.sharedMaterial = this.Hat.AltShader;
		}
		else
		{
			this.FrontLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultHatShader;
			this.BackLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultHatShader;
		}
		SpriteAnimNodeSync component = base.GetComponent<SpriteAnimNodeSync>();
		if (component)
		{
			component.NodeId = (this.Hat.NoBounce ? 1 : 0);
		}
		if (this.Hat.InFront)
		{
			this.BackLayer.enabled = false;
			this.FrontLayer.enabled = true;
			this.FrontLayer.sprite = this.Hat.MainImage;
			return;
		}
		if (this.Hat.BackImage)
		{
			this.BackLayer.enabled = true;
			this.FrontLayer.enabled = true;
			this.BackLayer.sprite = this.Hat.BackImage;
			this.FrontLayer.sprite = this.Hat.MainImage;
			return;
		}
		this.BackLayer.enabled = true;
		this.FrontLayer.enabled = false;
		this.FrontLayer.sprite = null;
		this.BackLayer.sprite = this.Hat.MainImage;
	}

	public void SetHat(uint hatId, int color)
	{
		if (!DestroyableSingleton<HatManager>.InstanceExists)
		{
			return;
		}
		this.Hat = DestroyableSingleton<HatManager>.Instance.GetHatById(hatId);
		this.SetHat(color);
	}

	internal void SetFloorAnim()
	{
		this.BackLayer.enabled = false;
		this.FrontLayer.enabled = true;
		this.FrontLayer.sprite = this.Hat.FloorImage;
	}

	internal void SetClimbAnim()
	{
		this.BackLayer.enabled = false;
		this.FrontLayer.enabled = true;
		this.FrontLayer.sprite = this.Hat.ClimbImage;
	}

	public void LateUpdate()
	{
		if (this.Parent && this.Hat && this.FrontLayer.sprite != this.Hat.ClimbImage && this.FrontLayer.sprite != this.Hat.FloorImage)
		{
			if ((this.Hat.InFront || this.Hat.BackImage) && this.Hat.LeftMainImage)
			{
				this.FrontLayer.sprite = (this.Parent.flipX ? this.Hat.LeftMainImage : this.Hat.MainImage);
			}
			if (this.Hat.BackImage && this.Hat.LeftBackImage)
			{
				this.BackLayer.sprite = (this.Parent.flipX ? this.Hat.LeftBackImage : this.Hat.BackImage);
			}
			else if (!this.Hat.BackImage && !this.Hat.InFront && this.Hat.LeftMainImage)
			{
				this.BackLayer.sprite = (this.Parent.flipX ? this.Hat.LeftMainImage : this.Hat.MainImage);
			}
			this.flipX = this.Parent.flipX;
		}
	}

	internal void SetColor(int color)
	{
		PlayerControl.SetPlayerMaterialColors(color, this.FrontLayer);
		PlayerControl.SetPlayerMaterialColors(color, this.BackLayer);
	}

	internal void SetMaskLayer(int layer)
	{
		this.FrontLayer.material.SetInt("_MaskLayer", layer);
		this.BackLayer.material.SetInt("_MaskLayer", layer);
	}
}
