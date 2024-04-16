using System;
using UnityEngine;

public class OpenDoorConsole : MonoBehaviour, IUsable
{
	private ImageNames useIcon = ImageNames.UseButton;

	private PlainDoor MyDoor;

	private SpriteRenderer Image;

	public float usableDisance = 1.5f;

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
			return this.usableDisance;
		}
	}

	public float PercentCool
	{
		get
		{
			return 0f;
		}
	}

	public void Awake()
	{
		this.MyDoor = base.GetComponent<PlainDoor>();
		this.Image = base.GetComponent<SpriteRenderer>();
	}

	public float CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
	{
		float num = Vector2.Distance(pc.Object.GetTruePosition(), base.transform.position);
		couldUse = (!pc.IsDead && !this.MyDoor.Open);
		canUse = (couldUse && num <= this.UsableDistance);
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
		ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, this.MyDoor.Id | 64);
		this.MyDoor.SetDoorway(true);
	}
}
