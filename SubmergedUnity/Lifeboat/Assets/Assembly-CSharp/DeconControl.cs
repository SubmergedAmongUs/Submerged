using System;
using UnityEngine;
using UnityEngine.UI;

public class DeconControl : MonoBehaviour, IUsable
{
	public DeconSystem System;

	public float usableDistance = 1f;

	public SpriteRenderer Image;

	public AudioClip UseSound;

	public Button.ButtonClickedEvent OnUse;

	private const float CooldownDuration = 6f;

	private float cooldown;

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
			return this.usableDistance;
		}
	}

	public float PercentCool
	{
		get
		{
			return this.cooldown / 6f;
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

	public void Update()
	{
		this.cooldown = Mathf.Max(this.cooldown - Time.deltaTime, 0f);
	}

	public float CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
	{
		if (this.System.CurState != DeconSystem.States.Idle)
		{
			canUse = false;
			couldUse = false;
			return 0f;
		}
		float num = float.MaxValue;
		PlayerControl @object = pc.Object;
		Vector2 truePosition = @object.GetTruePosition();
		Vector3 position = base.transform.position;
		position.y -= 0.1f;
		couldUse = (@object.CanMove && !pc.IsDead && !PhysicsHelpers.AnythingBetween(truePosition, position, Constants.ShipAndObjectsMask, false));
		canUse = (couldUse && this.cooldown == 0f);
		if (canUse)
		{
			num = Vector2.Distance(truePosition, position);
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
		this.cooldown = 6f;
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.UseSound, false, 1f);
		}
		this.OnUse.Invoke();
	}
}
