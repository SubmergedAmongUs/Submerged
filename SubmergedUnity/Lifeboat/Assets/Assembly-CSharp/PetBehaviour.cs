using System;
using PowerTools;
using UnityEngine;

public class PetBehaviour : MonoBehaviour, IBuyable, ISteamBuyable, IEpicBuyable
{
	private const float SnapDistance = 2f;

	public bool Free;

	public bool NotInStore;

	public string ProductId;

	public StringNames StoreName;

	public uint SteamId;

	public string EpicId;

	public int ItchId;

	public string ItchUrl;

	public string Win10Id;

	public PlayerControl Source;

	public const float MinDistance = 0.2f;

	public const float damping = 0.7f;

	public const float Easing = 0.2f;

	public const float Speed = 5f;

	public float YOffset = -0.25f;

	public SpriteAnim animator;

	public SpriteRenderer rend;

	public SpriteRenderer shadowRend;

	public Rigidbody2D body;

	public Collider2D Collider;

	public AnimationClip idleClip;

	public AnimationClip sadClip;

	public AnimationClip scaredClip;

	public AnimationClip walkClip;

	public string ProdId
	{
		get
		{
			return this.ProductId;
		}
	}

	public string SteamPrice
	{
		get
		{
			return "$2.99";
		}
	}

	public string EpicPrice
	{
		get
		{
			return "$2.99";
		}
	}

	public uint SteamAppId
	{
		get
		{
			return this.SteamId;
		}
	}

	public string EpicAppId
	{
		get
		{
			return this.EpicId;
		}
	}

	public bool Visible
	{
		set
		{
			if (this.rend)
			{
				this.rend.enabled = value;
			}
			if (this.shadowRend)
			{
				this.shadowRend.enabled = value;
			}
		}
	}

	private Vector2 GetTruePosition()
	{
		return (Vector2) base.transform.position + this.Collider.offset * 0.7f;
	}

	public void Start()
	{
		if (Application.targetFrameRate > 30)
		{
			this.body.interpolation = RigidbodyInterpolation2D.Interpolate;
		}
	}

	public void Update()
	{
		if (!this.Source)
		{
			this.body.velocity = Vector2.zero;
			return;
		}
		Vector2 truePosition = this.Source.GetTruePosition();
		Vector2 truePosition2 = this.GetTruePosition();
		Vector2 vector = this.body.velocity;
		Vector2 vector2 = truePosition - truePosition2;
		float num = 0f;
		if (this.Source.CanMove)
		{
			num = 0.2f;
		}
		if (vector2.sqrMagnitude > num)
		{
			if (vector2.sqrMagnitude > 2f)
			{
				base.transform.position = truePosition;
				return;
			}
			vector2 *= 5f * PlayerControl.GameOptions.PlayerSpeedMod;
			vector = vector * 0.8f + vector2 * 0.2f;
		}
		else
		{
			vector *= 0.7f;
		}
		AnimationClip currentAnimation = this.animator.GetCurrentAnimation();
		if (vector.sqrMagnitude > 0.01f)
		{
			if (currentAnimation != this.walkClip)
			{
				this.animator.Play(this.walkClip, 1f);
			}
			if (vector.x < -0.01f)
			{
				this.rend.flipX = true;
			}
			else if (vector.x > 0.01f)
			{
				this.rend.flipX = false;
			}
		}
		else if (currentAnimation == this.walkClip)
		{
			this.animator.Play(this.idleClip, 1f);
		}
		this.body.velocity = vector;
	}

	private void LateUpdate()
	{
		Vector3 localPosition = base.transform.localPosition;
		localPosition.z = (localPosition.y + this.YOffset) / 1000f + 0.0002f;
		base.transform.localPosition = localPosition;
	}

	public void SetMourning()
	{
		this.Source = null;
		this.body.velocity = Vector2.zero;
		this.animator.Play(this.sadClip, 1f);
	}
}
