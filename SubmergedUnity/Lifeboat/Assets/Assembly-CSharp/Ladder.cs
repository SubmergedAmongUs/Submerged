using System;
using UnityEngine;

public class Ladder : MonoBehaviour, IUsable
{
	public byte Id;

	public SpriteRenderer SpotArea;

	public bool IsTop;

	public Ladder Destination;

	public AudioClip UseSound;

	public SpriteRenderer Image;

	public float UsableDistance
	{
		get
		{
			return 0.5f;
		}
	}

	public float PercentCool
	{
		get
		{
			return 0f;
		}
	}

	public ImageNames UseIcon
	{
		get
		{
			return ImageNames.UseButton;
		}
	}

	public float CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
	{
		float num = float.MaxValue;
		PlayerControl @object = pc.Object;
		couldUse = (!pc.IsDead && @object.CanMove);
		canUse = couldUse;
		if (canUse)
		{
			Vector2 truePosition = @object.GetTruePosition();
			Vector3 position = base.transform.position;
			num = Vector2.Distance(truePosition, position);
			canUse &= (num <= this.UsableDistance && !PhysicsHelpers.AnythingBetween(truePosition, position, Constants.ShipOnlyMask, false));
		}
		return num;
	}

	public void SetOutline(bool on, bool mainTarget)
	{
		this.Image.material.SetFloat("_Outline", (float)(on ? 1 : 0));
		this.Image.material.SetColor("_OutlineColor", Color.white);
		this.Image.material.SetColor("_AddColor", mainTarget ? Color.white : Color.clear);
	}

	public void Use()
	{
		GameData.PlayerInfo data = PlayerControl.LocalPlayer.Data;
		bool flag;
		bool flag2;
		this.CanUse(data, out flag, out flag2);
		if (flag)
		{
			PlayerControl.LocalPlayer.MyPhysics.RpcClimbLadder(this);
		}
	}
}
