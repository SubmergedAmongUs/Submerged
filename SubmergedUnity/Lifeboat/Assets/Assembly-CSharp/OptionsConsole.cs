using System;
using UnityEngine;

public class OptionsConsole : MonoBehaviour, IUsable
{
	public CustomPlayerMenu MenuPrefab;

	public SpriteRenderer Outline;

	public ImageNames UseIcon
	{
		get
		{
			return ImageNames.OptionsButton;
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

	public float CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
	{
		float num = float.MaxValue;
		PlayerControl @object = pc.Object;
		couldUse = @object.CanMove;
		canUse = couldUse;
		if (canUse)
		{
			num = Vector2.Distance(@object.GetTruePosition(), base.transform.position);
			canUse &= (num <= this.UsableDistance);
		}
		return num;
	}

	public void SetOutline(bool on, bool mainTarget)
	{
		if (this.Outline)
		{
			this.Outline.material.SetFloat("_Outline", (float)(on ? 1 : 0));
			this.Outline.material.SetColor("_OutlineColor", Color.white);
			this.Outline.material.SetColor("_AddColor", mainTarget ? Color.white : Color.clear);
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
		CustomPlayerMenu customPlayerMenu = UnityEngine.Object.Instantiate<CustomPlayerMenu>(this.MenuPrefab);
		customPlayerMenu.transform.SetParent(Camera.main.transform, false);
		customPlayerMenu.transform.localPosition = new Vector3(0f, 0f, -20f);
	}
}
