using UnityEngine;

public class PooledMapIcon : PoolableBehavior
{
	public override void Reset()
	{
		lastMapTaskStep = -1;
		alphaPulse.enabled = false;
		rend.material.SetFloat("_Outline", 0f);
		base.Reset();
	}

	public float NormalSize = 0.3f;
	public int lastMapTaskStep = -1;
	public SpriteRenderer rend;
	public AlphaPulse alphaPulse;
}
