using System;
using Rewired;
using UnityEngine;

public class WaterStage : Minigame
{
	public float RefuelDuration = 5f;

	public SpriteRenderer waterButton;

	public Sprite buttonDownSprite;

	public Sprite buttonUpSprite;

	public VerticalGauge srcGauge;

	public VerticalGauge destGauge;

	public AudioClip RefuelSound;

	private float timer;

	private bool isDown;

	private bool complete;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		this.timer = (float)this.MyNormTask.Data[0] / 255f;
	}

	public void FixedUpdate()
	{
		if (this.complete)
		{
			return;
		}
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			bool button = ReInput.players.GetPlayer(0).GetButton(11);
			if (button != this.isDown)
			{
				this.isDown = button;
				if (!button)
				{
					SoundManager.Instance.StopSound(this.RefuelSound);
				}
				this.waterButton.sprite = (button ? this.buttonDownSprite : this.buttonUpSprite);
			}
		}
		if (this.isDown && this.timer < 1f)
		{
			this.timer += Time.fixedDeltaTime / this.RefuelDuration;
			this.MyNormTask.Data[0] = (byte)Mathf.Min(255f, this.timer * 255f);
			if (this.timer >= 1f)
			{
				this.complete = true;
				this.MyNormTask.Data[0] = 0;
				this.MyNormTask.NextStep();
				SoundManager.Instance.StopSound(this.RefuelSound);
				base.transform.parent.GetComponent<Minigame>().Close();
			}
		}
		if (this.destGauge)
		{
			this.destGauge.value = this.timer;
		}
		if (this.srcGauge)
		{
			this.srcGauge.value = 1f - this.timer;
		}
	}

	public void Refuel()
	{
		if (this.complete)
		{
			base.transform.parent.GetComponent<Minigame>().Close();
			return;
		}
		this.isDown = !this.isDown;
		this.waterButton.sprite = (this.isDown ? this.buttonDownSprite : this.buttonUpSprite);
		if (this.isDown)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlayDynamicSound("Refuel", this.RefuelSound, true, new DynamicSound.GetDynamicsFunction(this.GetRefuelDynamics), true);
				return;
			}
		}
		else
		{
			SoundManager.Instance.StopSound(this.RefuelSound);
		}
	}

	private void GetRefuelDynamics(AudioSource player, float dt)
	{
		player.volume = 1f;
		if (this.MyNormTask.taskStep == 0)
		{
			player.pitch = Mathf.Lerp(0.75f, 1.25f, this.timer);
			return;
		}
		player.pitch = Mathf.Lerp(1.25f, 0.75f, this.timer);
	}

	public override void Close()
	{
		SoundManager.Instance.StopSound(this.RefuelSound);
		if (Minigame.Instance)
		{
			Minigame.Instance.Close();
		}
	}
}
