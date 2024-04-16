using System;
using PowerTools;
using UnityEngine;

public class SurvCamera : MonoBehaviour
{
	public string CamName;

	public StringNames NewName;

	public SpriteAnim[] Images;

	public float CamSize = 3f;

	public float CamAspect = 1f;

	public Vector3 Offset;

	public AnimationClip OnAnim;

	public AnimationClip OffAnim;

	public StringNames camNameString;

	public void Awake()
	{
		if (this.Images == null || this.Images.Length == 0)
		{
			this.Images = base.GetComponents<SpriteAnim>();
		}
	}

	public virtual void SetAnimation(bool on)
	{
		SpriteAnim[] images = this.Images;
		for (int i = 0; i < images.Length; i++)
		{
			images[i].Play(on ? this.OnAnim : this.OffAnim, 1f);
		}
	}
}
