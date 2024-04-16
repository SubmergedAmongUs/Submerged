using System;
using UnityEngine;

public class LeafBehaviour : MonoBehaviour
{
	public Sprite[] Images;

	public FloatRange SpinSpeed = new FloatRange(-45f, 45f);

	public Vector2Range StartVel;

	public float AccelRate = 30f;

	[HideInInspector]
	public LeafMinigame Parent;

	public bool Held;

	private static RandomFill<Sprite> ImageFiller = new RandomFill<Sprite>();

	[HideInInspector]
	public Rigidbody2D body;

	public void Start()
	{
		LeafBehaviour.ImageFiller.Set(this.Images);
		Sprite sprite = LeafBehaviour.ImageFiller.Get();
		if (!sprite)
		{
			LeafBehaviour.ImageFiller = new RandomFill<Sprite>();
			LeafBehaviour.ImageFiller.Set(this.Images);
			sprite = LeafBehaviour.ImageFiller.Get();
		}
		base.GetComponent<SpriteRenderer>().sprite = sprite;
		Debug.LogError(sprite);
		this.body = base.GetComponent<Rigidbody2D>();
		this.body.angularVelocity = this.SpinSpeed.Next();
		this.body.velocity = this.StartVel.Next();
	}

	public void FixedUpdate()
	{
		if (!this.Held && (double)base.transform.localPosition.x < -2.5)
		{
			this.Parent.LeafDone(this);
		}
	}
}
