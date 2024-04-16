using System;
using UnityEngine;

public class PooledMapIcon : PoolableBehavior
{
	public float NormalSize = 0.3f;

	public int lastMapTaskStep = -1;

	public SpriteRenderer rend;

	public AlphaPulse alphaPulse;

	public void Update()
	{
		if (this.alphaPulse.enabled)
		{
			float num = Mathf.Abs(Mathf.Cos((this.alphaPulse.Offset + Time.time) * 3.1415927f / this.alphaPulse.Duration));
			if ((double)num > 0.9)
			{
				num -= 0.9f;
				num = this.NormalSize + num;
				base.transform.localScale = new Vector3(num, num, num);
			}
		}
	}

	public override void Reset()
	{
		this.lastMapTaskStep = -1;
		this.alphaPulse.enabled = false;
		this.rend.material.SetFloat("_Outline", 0f);
		base.Reset();
	}
}
