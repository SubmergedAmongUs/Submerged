using System;
using UnityEngine;

public class DoorConsole : MonoBehaviour, IUsable
{
	private PlainDoor MyDoor;

	public Minigame MinigamePrefab;

	private SpriteRenderer Image;

	public ImageNames UseIcon
	{
		get
		{
			return ImageNames.UseButton;
		}
	}

	public float UsableDistance
	{
		get
		{
			return 1f;
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
		PlayerControl.LocalPlayer.NetTransform.Halt();
		Minigame minigame = UnityEngine.Object.Instantiate<Minigame>(this.MinigamePrefab, Camera.main.transform);
		minigame.transform.localPosition = new Vector3(0f, 0f, -50f);
		((IDoorMinigame)minigame).SetDoor(this.MyDoor);
		minigame.Begin(null);
	}
}
