using System;
using System.Collections;
using UnityEngine;

public class Asteroid : PoolableBehavior
{
	public Sprite[] AsteroidImages;

	public Sprite[] BrokenImages;

	private int imgIdx;

	public FloatRange MoveSpeed = new FloatRange(2f, 5f);

	public FloatRange RotateSpeed = new FloatRange(-10f, 10f);

	public SpriteRenderer Explosion;

	public Vector3 TargetPosition { get; internal set; }

	public void FixedUpdate()
	{
		base.transform.localRotation = Quaternion.Euler(0f, 0f, base.transform.localRotation.eulerAngles.z + this.RotateSpeed.Last * Time.fixedDeltaTime);
		Vector3 vector = this.TargetPosition - base.transform.localPosition;
		if (vector.sqrMagnitude > 0.05f)
		{
			vector.Normalize();
			base.transform.localPosition += vector * this.MoveSpeed.Last * Time.fixedDeltaTime;
			return;
		}
		this.OwnerPool.Reclaim(this);
	}

	public override void Reset()
	{
		base.enabled = true;
		this.Explosion.enabled = false;
		SpriteRenderer component = base.GetComponent<SpriteRenderer>();
		this.imgIdx = this.AsteroidImages.RandomIdx<Sprite>();
		component.sprite = this.AsteroidImages[this.imgIdx];
		component.enabled = true;
		ButtonBehavior component2 = base.GetComponent<ButtonBehavior>();
		component2.enabled = true;
		component2.OnClick.RemoveAllListeners();
		base.transform.Rotate(0f, 0f, this.RotateSpeed.Next());
		this.MoveSpeed.Next();
		base.Reset();
	}

	public IEnumerator CoBreakApart()
	{
		base.enabled = false;
		base.GetComponent<ButtonBehavior>().enabled = false;
		VibrationManager.Vibrate(0.5f, 0.5f, 0.35f, VibrationManager.VibrationFalloff.Linear, null, false);
		this.Explosion.enabled = true;
		yield return new WaitForLerp(0.1f, delegate(float t)
		{
			this.Explosion.transform.localScale = new Vector3(t, t, t);
		});
		yield return new WaitForSeconds(0.05f);
		yield return new WaitForLerp(0.05f, delegate(float t)
		{
			this.Explosion.transform.localScale = new Vector3(1f - t, 1f - t, 1f - t);
		});
		SpriteRenderer rend = base.GetComponent<SpriteRenderer>();
		yield return null;
		rend.sprite = this.BrokenImages[this.imgIdx];
		yield return new WaitForSeconds(0.2f);
		this.OwnerPool.Reclaim(this);
		yield break;
	}
}
