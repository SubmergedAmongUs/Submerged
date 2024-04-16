using System;
using UnityEngine;

public class SystemConsole : MonoBehaviour, IUsable
{
	public ImageNames useIcon = ImageNames.UseButton;

	public float usableDistance = 1f;

	public bool FreeplayOnly;

	public bool onlyFromBelow;

	public SpriteRenderer Image;

	public Minigame MinigamePrefab;

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

	public void Start()
	{
		if (this.FreeplayOnly && !DestroyableSingleton<TutorialManager>.InstanceExists)
		{
			 UnityEngine.Object.Destroy(base.gameObject);
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
		Vector2 truePosition = @object.GetTruePosition();
		couldUse = (@object.CanMove && (!pc.IsDead || !(this.MinigamePrefab is EmergencyMinigame)));
		canUse = (couldUse && (!this.onlyFromBelow || truePosition.y < base.transform.position.y));
		if (canUse)
		{
			num = Vector2.Distance(truePosition, base.transform.position);
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
		Minigame minigame = UnityEngine.Object.Instantiate<Minigame>(this.MinigamePrefab);
		minigame.transform.SetParent(Camera.main.transform, false);
		minigame.transform.localPosition = new Vector3(0f, 0f, -50f);
		minigame.Begin(null);
	}
}
