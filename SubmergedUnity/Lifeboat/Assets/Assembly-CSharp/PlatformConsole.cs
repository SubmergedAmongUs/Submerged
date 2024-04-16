using System;
using UnityEngine;

public class PlatformConsole : MonoBehaviour, IUsable
{
	public float usableDistance = 0.5f;

	public SpriteRenderer Image;

	public MovingPlatformBehaviour Platform;

	public float UsableDistance
	{
		get
		{
			return this.usableDistance;
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
		couldUse = (!pc.IsDead && @object.CanMove && !this.Platform.InUse && Vector2.Distance(this.Platform.transform.position, base.transform.position) < 2f);
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
		if (this.Image)
		{
			this.Image.material.SetFloat("_Outline", (float)(on ? 1 : 0));
			this.Image.material.SetColor("_OutlineColor", Color.white);
			this.Image.material.SetColor("_AddColor", mainTarget ? Color.white : Color.clear);
		}
	}

	public void Use()
	{
		bool flag;
		bool flag2;
		this.CanUse(PlayerControl.LocalPlayer.Data, out flag, out flag2);
		if (!flag)
		{
			return;
		}
		this.Platform.Use();
	}
}
