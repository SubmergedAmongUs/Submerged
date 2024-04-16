using System;
using UnityEngine;

public class MapConsole : MonoBehaviour, IUsable
{
	public ImageNames useIcon = ImageNames.AdminMapButton;

	public float usableDistance = 1f;

	public SpriteRenderer Image;

	public ImageNames UseIcon
	{
		get
		{
			return this.useIcon;
		}
	}

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

	public void SetOutline(bool on, bool mainTarget)
	{
		if (this.Image)
		{
			this.Image.material.SetFloat("_Outline", (float)(on ? 1 : 0));
			this.Image.material.SetColor("_OutlineColor", Color.white);
			this.Image.material.SetColor("_AddColor", mainTarget ? Color.white : Color.clear);
		}
	}

	public float CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
	{
		float num = float.MaxValue;
		PlayerControl @object = pc.Object;
		couldUse = pc.Object.CanMove;
		canUse = couldUse;
		if (canUse)
		{
			num = Vector2.Distance(@object.GetTruePosition(), base.transform.position);
			canUse &= (num <= this.UsableDistance);
		}
		return num;
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
		PlayerControl.LocalPlayer.NetTransform.Halt();
		DestroyableSingleton<HudManager>.Instance.ShowMap(delegate(MapBehaviour m)
		{
			m.ShowCountOverlay();
		});
		if (PlayerControl.LocalPlayer.AmOwner)
		{
			PlayerControl.LocalPlayer.MyPhysics.inputHandler.enabled = true;
			ConsoleJoystick.SetMode_Task();
		}
	}
}
