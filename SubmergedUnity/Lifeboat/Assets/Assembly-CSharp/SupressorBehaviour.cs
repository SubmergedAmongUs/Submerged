using System;
using PowerTools;
using UnityEngine;

public class SupressorBehaviour : MonoBehaviour
{
	public Sprite ActiveBase;

	public Sprite InactiveBase;

	public SpriteRenderer BaseImage;

	public AnimationClip ElectricActive;

	public AnimationClip ElectricInactive;

	public SpriteAnim Electric;

	public AnimationClip LightsActive;

	public AnimationClip LightsInactive;

	public SpriteAnim Lights;

	public void Activate()
	{
		this.BaseImage.sprite = this.ActiveBase;
		this.Electric.Play(this.ElectricActive, 1f);
		this.Lights.Play(this.LightsActive, 1f);
	}

	public void Deactivate()
	{
		this.BaseImage.sprite = this.InactiveBase;
		this.Electric.Play(this.ElectricInactive, 1f);
		this.Lights.Play(this.LightsInactive, 1f);
	}
}
