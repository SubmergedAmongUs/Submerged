using System;
using Rewired;
using UnityEngine;

public class RefuelStage : Minigame
{
	public float RefuelDuration = 5f;

	private Color darkRed = new Color32(90, 0, 0, byte.MaxValue);

	private Color red = new Color32(byte.MaxValue, 58, 0, byte.MaxValue);

	private Color green = Color.green;

	public SpriteRenderer redLight;

	public SpriteRenderer greenLight;

	public VerticalGauge srcGauge;

	public VerticalGauge destGauge;

	public AudioClip RefuelSound;

	private float timer;

	private bool isDown;

	private bool complete;

	private bool usingController;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		this.timer = (float)this.MyNormTask.Data[0] / 255f;
		base.SetupInput(true);
	}

	public void FixedUpdate()
	{
		if (ReInput.players.GetPlayer(0).GetButton(21))
		{
			if (!this.isDown)
			{
				this.usingController = true;
				this.Refuel();
			}
		}
		else if (this.isDown && this.usingController)
		{
			this.usingController = false;
			this.Refuel();
		}
		if (this.complete)
		{
			return;
		}
		if (this.isDown && this.timer < 1f)
		{
			this.timer += Time.fixedDeltaTime / this.RefuelDuration;
			this.MyNormTask.Data[0] = (byte)Mathf.Min(255f, this.timer * 255f);
			if (this.timer >= 1f)
			{
				this.complete = true;
				if (this.greenLight)
				{
					this.greenLight.color = this.green;
				}
				if (this.redLight)
				{
					this.redLight.color = this.darkRed;
				}
				if (this.MyNormTask.MaxStep == 1)
				{
					this.MyNormTask.NextStep();
				}
				else if (this.MyNormTask.StartAt == SystemTypes.CargoBay || this.MyNormTask.StartAt == SystemTypes.Engine)
				{
					this.MyNormTask.Data[0] = 0;
					this.MyNormTask.Data[1] = (byte) (BoolRange.Next(0.5f) ? 1 : 2);
					this.MyNormTask.NextStep();
				}
				else
				{
					this.MyNormTask.Data[0] = 0;
					byte[] data = this.MyNormTask.Data;
					int num = 1;
					data[num] += 1;
					if (this.MyNormTask.Data[1] % 2 == 0)
					{
						this.MyNormTask.NextStep();
					}
					this.MyNormTask.UpdateArrow();
				}
			}
		}
		this.destGauge.value = this.timer;
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
		if (this.redLight)
		{
			this.redLight.color = (this.isDown ? this.red : this.darkRed);
		}
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
		player.pitch = Mathf.Lerp(0.75f, 1.25f, this.timer);
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
