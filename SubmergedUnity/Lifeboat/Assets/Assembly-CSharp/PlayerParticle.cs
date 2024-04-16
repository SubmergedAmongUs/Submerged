using System;
using UnityEngine;

public class PlayerParticle : PoolableBehavior
{
	public SpriteRenderer myRend;

	public float maxDistance = 6f;

	public Vector2 velocity;

	public float angularVelocity;

	private Vector3 lastCamera;

	public Camera FollowCamera { get; set; }

	public void Update()
	{
		Vector3 vector = base.transform.localPosition;
		float sqrMagnitude = vector.sqrMagnitude;
		if (this.FollowCamera)
		{
			Vector3 position = this.FollowCamera.transform.position;
			position.z = 0f;
			vector += (position - this.lastCamera) * (1f - base.transform.localScale.x);
			this.lastCamera = position;
			sqrMagnitude = (vector - position).sqrMagnitude;
		}
		if (sqrMagnitude > this.maxDistance * this.maxDistance)
		{
			this.OwnerPool.Reclaim(this);
			return;
		}
		vector += (Vector3) this.velocity * Time.deltaTime;
		base.transform.localPosition = vector;
		base.transform.Rotate(0f, 0f, Time.deltaTime * this.angularVelocity);
	}
}
