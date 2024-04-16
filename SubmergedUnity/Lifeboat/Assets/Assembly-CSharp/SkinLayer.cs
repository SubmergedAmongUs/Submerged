using System;
using PowerTools;
using UnityEngine;

public class SkinLayer : MonoBehaviour
{
	public SpriteRenderer layer;

	public SpriteAnim animator;

	public SkinData skin;

	public bool Flipped
	{
		set
		{
			if (!this.skin)
			{
				this.layer.flipX = value;
				return;
			}
			this.layer.flipX = (value && this.animator.Clip != this.skin.IdleLeftAnim && this.animator.Clip != this.skin.RunLeftAnim && this.animator.Clip != this.skin.EnterLeftVentAnim && this.animator.Clip != this.skin.ExitLeftVentAnim && this.animator.Clip != this.skin.SpawnLeftAnim);
		}
	}

	public bool Visible
	{
		set
		{
			this.layer.enabled = value;
		}
	}

	public void SetMaskLayer(int layer)
	{
		this.layer.material.SetInt("_MaskLayer", layer);
	}

	public void SetRun(bool isLeft)
	{
		if (!this.skin || !this.animator)
		{
			this.SetGhost();
			return;
		}
		if (isLeft && this.skin.RunLeftAnim)
		{
			if (!this.animator.IsPlaying(this.skin.RunLeftAnim))
			{
				this.animator.Play(this.skin.RunLeftAnim, 1f);
				this.animator.Time = 0.45833334f;
				return;
			}
		}
		else if (!this.animator.IsPlaying(this.skin.RunAnim))
		{
			this.animator.Play(this.skin.RunAnim, 1f);
			this.animator.Time = 0.45833334f;
		}
	}

	public void SetSpawn(bool isLeft, float time = 0f)
	{
		if (!this.skin || !this.animator)
		{
			this.SetGhost();
			return;
		}
		if (isLeft && this.skin.SpawnLeftAnim)
		{
			this.Flipped = false;
			this.animator.Play(this.skin.SpawnLeftAnim, 1f);
		}
		else
		{
			this.animator.Play(this.skin.SpawnAnim, 1f);
		}
		this.animator.Time = time;
	}

	internal void SetClimb(bool down)
	{
		if (!this.skin || !this.animator)
		{
			this.SetGhost();
			return;
		}
		this.animator.Play(down ? this.skin.ClimbDownAnim : this.skin.ClimbAnim, 1f);
		this.animator.Time = 0f;
	}

	public void SetExitVent(bool isLeft)
	{
		if (!this.skin || !this.animator)
		{
			this.SetGhost();
			return;
		}
		if (isLeft && this.skin.ExitLeftVentAnim)
		{
			this.animator.Play(this.skin.ExitLeftVentAnim, 1f);
		}
		else
		{
			this.animator.Play(this.skin.ExitVentAnim, 1f);
		}
		this.animator.Time = 0f;
	}

	public void SetEnterVent(bool isLeft)
	{
		if (!this.skin || !this.animator)
		{
			this.SetGhost();
			return;
		}
		if (isLeft && this.skin.EnterLeftVentAnim)
		{
			this.animator.Play(this.skin.EnterLeftVentAnim, 1f);
		}
		else
		{
			this.animator.Play(this.skin.EnterVentAnim, 1f);
		}
		this.animator.Time = 0f;
	}

	public void SetIdle(bool isLeft)
	{
		if (!this.skin || !this.animator)
		{
			this.SetGhost();
			return;
		}
		if (isLeft && this.skin.RunLeftAnim)
		{
			if (!this.animator.IsPlaying(this.skin.IdleLeftAnim))
			{
				this.animator.Play(this.skin.IdleLeftAnim, 1f);
				return;
			}
		}
		else if (!this.animator.IsPlaying(this.skin.IdleAnim))
		{
			this.animator.Play(this.skin.IdleAnim, 1f);
		}
	}

	public void SetGhost()
	{
		if (!this.animator)
		{
			return;
		}
		this.animator.Stop();
		this.layer.sprite = null;
	}

	internal void SetSkin(uint skinId, bool isLeft)
	{
		this.skin = DestroyableSingleton<HatManager>.Instance.GetSkinById(skinId);
		this.SetIdle(isLeft);
	}
}
