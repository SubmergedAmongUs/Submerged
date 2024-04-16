using System;
using UnityEngine;

public class PlayerParticles : MonoBehaviour
{
	public PlayerParticleInfo[] Sprites;

	public FloatRange velocity;

	public FloatRange scale;

	public ObjectPoolBehavior pool;

	public float StartRadius;

	public Camera FollowCamera;

	private RandomFill<PlayerParticleInfo> fill;

	public void Start()
	{
		this.fill = new RandomFill<PlayerParticleInfo>();
		this.fill.Set(this.Sprites);
		int num = 0;
		while (this.pool.NotInUse > 0)
		{
			PlayerParticle playerParticle = this.pool.Get<PlayerParticle>();
			PlayerControl.SetPlayerMaterialColors(num++, playerParticle.myRend);
			this.PlacePlayer(playerParticle, true);
		}
	}

	public void Update()
	{
		while (this.pool.NotInUse > 0)
		{
			PlayerParticle part = this.pool.Get<PlayerParticle>();
			this.PlacePlayer(part, false);
		}
	}

	private void PlacePlayer(PlayerParticle part, bool initial)
	{
		Vector3 vector = UnityEngine.Random.insideUnitCircle;
		if (!initial)
		{
			vector.Normalize();
		}
		vector *= this.StartRadius;
		float num = this.scale.Next();
		part.transform.localScale = new Vector3(num, num, num);
		vector.z = -num * 0.001f;
		if (this.FollowCamera)
		{
			Vector3 position = this.FollowCamera.transform.position;
			position.z = 0f;
			vector += position;
			part.FollowCamera = this.FollowCamera;
		}
		part.transform.localPosition = vector;
		PlayerParticleInfo playerParticleInfo = this.fill.Get();
		part.myRend.sprite = playerParticleInfo.image;
		part.myRend.flipX = BoolRange.Next(0.5f);
		Vector2 vector2 = -vector.normalized;
		vector2 = vector2.Rotate(FloatRange.Next(-45f, 45f));
		part.velocity = vector2 * this.velocity.Next();
		part.angularVelocity = playerParticleInfo.angularVel.Next();
		if (playerParticleInfo.alignToVel)
		{
			part.transform.localEulerAngles = new Vector3(0f, 0f, Vector2.up.AngleSigned(vector2));
		}
	}
}
