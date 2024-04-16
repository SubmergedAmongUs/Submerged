using System;
using UnityEngine;

public class NoShadowBehaviour : MonoBehaviour
{
	public Renderer rend;

	public bool didHit;

	public Renderer shadowChild;

	public Collider2D hitOverride;

	private BoxCollider2D bc;

	private bool isBox;

	private bool verticalBox;

	private Vector2[] boxCheckPoints;

	private const int edgePoints = 4;

	private const int totalPointsPerEdge = 6;

	public void Start()
	{
		LightSource.NoShadows.Add(base.gameObject, this);
		this.CalculateBoxEdgeCheckPoints();
	}

	private void CalculateBoxEdgeCheckPoints()
	{
		this.bc = base.GetComponent<BoxCollider2D>();
		if (this.bc)
		{
			this.isBox = true;
			this.verticalBox = false;
			this.boxCheckPoints = new Vector2[8];
			Vector2 vector = this.bc.size * 0.5f;
			Vector2 vector2 = new Vector2(vector.x, 0f);
			Vector2 vector3 = new Vector2(0f, vector.y);
			if (vector.y > vector.x)
			{
				this.verticalBox = true;
				vector3 = vector2 = new Vector2(0f, vector.y);
			}
			int i = 1;
			int num = 0;
			while (i < 5)
			{
				Vector2 vector4 = Vector2.Lerp(-vector2, vector2, (float)i / 5f);
				this.boxCheckPoints[num] = vector4 + vector3 + this.bc.offset;
				this.boxCheckPoints[num + 4] = vector4 - vector3 + this.bc.offset;
				i++;
				num++;
			}
		}
	}

	public void OnDestroy()
	{
		LightSource.NoShadows.Remove(base.gameObject);
	}

	private void LateUpdate()
	{
		if (!PlayerControl.LocalPlayer)
		{
			return;
		}
		GameData.PlayerInfo data = PlayerControl.LocalPlayer.Data;
		if (data == null || !data.IsDead)
		{
			if (this.didHit)
			{
				this.didHit = false;
				ShipStatus instance = ShipStatus.Instance;
				if (instance && instance.CalculateLightRadius(data) > instance.MaxLightRadius / 3f)
				{
					this.SetMaskFunction(8);
					return;
				}
			}
			this.SetMaskFunction(1);
			return;
		}
		this.SetMaskFunction(8);
	}

	private void SetMaskFunction(int func)
	{
		this.rend.material.SetInt("_Mask", func);
		if (this.shadowChild)
		{
			this.shadowChild.material.SetInt("_Mask", func);
		}
	}

	internal void CheckHit(float lightRadius, Vector2 lightPosition)
	{
		Vector2 vector = base.transform.position;
		if (Vector2.Distance(vector, lightPosition) < lightRadius + 1f)
		{
			if (this.hitOverride)
			{
				if (!PhysicsHelpers.AnythingBetween(lightPosition, vector, Constants.ShadowMask, false, this.hitOverride, base.transform))
				{
					this.didHit = true;
				}
			}
			else if (!PhysicsHelpers.AnythingBetween(lightPosition, vector, Constants.ShadowMask, false, null, base.transform))
			{
				this.didHit = true;
			}
			if (!this.didHit && this.isBox)
			{
				int num = 0;
				int num2 = 4;
				if (this.verticalBox)
				{
					if (lightPosition.x < base.transform.position.x)
					{
						num += 4;
						num2 += 4;
					}
				}
				else if (lightPosition.y < base.transform.position.y)
				{
					num += 4;
					num2 += 4;
				}
				if (this.hitOverride)
				{
					for (int i = num; i < num2; i++)
					{
						if (!PhysicsHelpers.AnythingBetween(lightPosition, base.transform.TransformPoint(this.boxCheckPoints[i]), Constants.ShadowMask, false, this.hitOverride, base.transform))
						{
							this.didHit = true;
							return;
						}
					}
					return;
				}
				for (int j = num; j < num2; j++)
				{
					if (!PhysicsHelpers.AnythingBetween(lightPosition, base.transform.TransformPoint(this.boxCheckPoints[j]), Constants.ShadowMask, false, this.bc, base.transform))
					{
						this.didHit = true;
						return;
					}
				}
			}
		}
	}
}
