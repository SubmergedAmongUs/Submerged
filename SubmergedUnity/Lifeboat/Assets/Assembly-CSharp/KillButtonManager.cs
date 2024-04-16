using System;
using TMPro;
using UnityEngine;

public class KillButtonManager : MonoBehaviour
{
	public PlayerControl CurrentTarget;

	public SpriteRenderer renderer;

	public TextMeshPro TimerText;

	public TextMeshPro killText;

	public bool isCoolingDown = true;

	public bool isActive;

	private Vector2 uv;

	public void Start()
	{
		this.renderer.SetCooldownNormalizedUvs();
		this.SetTarget(null);
	}

	public void PerformKill()
	{
		if (base.isActiveAndEnabled && this.CurrentTarget && !this.isCoolingDown && !PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.CanMove)
		{
			PlayerControl.LocalPlayer.RpcMurderPlayer(this.CurrentTarget);
			this.SetTarget(null);
		}
	}

	public void SetTarget(PlayerControl target)
	{
		if (this.CurrentTarget && this.CurrentTarget != target)
		{
			this.CurrentTarget.MyRend.material.SetFloat("_Outline", 0f);
		}
		this.CurrentTarget = target;
		if (this.CurrentTarget)
		{
			SpriteRenderer myRend = this.CurrentTarget.MyRend;
			myRend.material.SetFloat("_Outline", (float)(this.isActive ? 1 : 0));
			myRend.material.SetColor("_OutlineColor", Color.red);
			this.renderer.color = Palette.EnabledColor;
			this.killText.alpha = Palette.EnabledColor.a;
			this.renderer.material.SetFloat("_Desat", 0f);
			return;
		}
		this.killText.alpha = Palette.DisabledClear.a;
		this.renderer.color = Palette.DisabledClear;
		this.renderer.material.SetFloat("_Desat", 1f);
	}

	public void SetCoolDown(float timer, float maxTimer)
	{
		float num = Mathf.Clamp(timer / maxTimer, 0f, 1f);
		if (this.renderer)
		{
			this.renderer.material.SetFloat("_Percent", num);
		}
		this.isCoolingDown = (num > 0f);
		if (this.isCoolingDown)
		{
			this.TimerText.text = Mathf.CeilToInt(timer).ToString();
			this.TimerText.gameObject.SetActive(true);
			return;
		}
		this.TimerText.gameObject.SetActive(false);
	}
}
