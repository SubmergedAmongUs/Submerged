using System;
using System.Collections;
using UnityEngine;

public class VentDirt : PoolableBehavior
{
	public Sprite[] DirtImages;

	public ParticleSystem CleanedEffect;

	private int imgIdx;

	public override void Reset()
	{
		base.enabled = true;
		SpriteRenderer component = base.GetComponent<SpriteRenderer>();
		this.imgIdx = this.DirtImages.RandomIdx<Sprite>();
		component.sprite = this.DirtImages[this.imgIdx];
		component.enabled = true;
		ButtonBehavior component2 = base.GetComponent<ButtonBehavior>();
		component2.enabled = true;
		component2.OnClick.RemoveAllListeners();
		base.Reset();
	}

	public IEnumerator CoDisappear()
	{
		if (!base.enabled)
		{
			yield break;
		}
		base.enabled = false;
		base.GetComponent<ButtonBehavior>().enabled = false;
		VibrationManager.Vibrate(0.5f, 0.5f, 0.35f, VibrationManager.VibrationFalloff.Linear, null, false);
		this.CleanedEffect.Play();
		SpriteRenderer rend = base.GetComponent<SpriteRenderer>();
		yield return Effects.Lerp(0.2f, delegate(float t)
		{
			rend.transform.localScale = new Vector3(1f - t, 1f - t, 1f - t);
		});
		yield return Effects.Wait(0.1f);
		rend.sprite = null;
		this.OwnerPool.Reclaim(this);
		yield break;
	}
}
