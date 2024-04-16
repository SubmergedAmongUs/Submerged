using System;
using System.Collections;
using UnityEngine;

public class GamePhotoBehaviour : MonoBehaviour
{
	public static readonly Color InWaterPink = Color.Lerp(Color.white, new Color32(211, 106, 129, byte.MaxValue), 0.6f);

	public float zOffset;

	public SpriteRenderer Frame;

	public SpriteRenderer Image;

	public Collider2D Hitbox;

	public Color TargetColor = Palette.ClearWhite;

	public void Start()
	{
		base.transform.SetLocalZ(1f + this.zOffset);
	}

	internal IEnumerator Pickup()
	{
		yield return Effects.All(new IEnumerator[]
		{
			Effects.Lerp(0.3f, delegate(float t)
			{
				base.transform.SetLocalZ(1f - t + this.zOffset);
			}),
			Effects.ScaleIn(base.transform, 1f, 1.2f, 0.3f),
			Effects.Sequence(new IEnumerator[]
			{
				Effects.Wait(0.15f),
				Effects.Action(delegate
				{
					this.TargetColor = Color.white;
				})
			})
		});
		yield break;
	}

	internal IEnumerator Drop(bool inWater)
	{
		yield return Effects.All(new IEnumerator[]
		{
			Effects.Lerp(0.3f, delegate(float t)
			{
				this.transform.SetLocalZ(t + this.zOffset);
			}),
			Effects.ScaleIn(base.transform, 1.2f, 1f, 0.3f),
			Effects.Sequence(new IEnumerator[]
			{
				Effects.Wait(0.15f),
				Effects.Action(delegate
				{
					if (inWater)
					{
						this.TargetColor = GamePhotoBehaviour.InWaterPink;
					}
				})
			})
		});
		yield break;
	}
}
