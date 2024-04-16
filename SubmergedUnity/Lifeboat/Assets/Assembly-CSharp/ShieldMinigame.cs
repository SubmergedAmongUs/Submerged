using System;
using Rewired;
using UnityEngine;

public class ShieldMinigame : Minigame
{
	public Color OnColor = Color.white;

	public Color OffColor = Color.red;

	public SpriteRenderer[] Shields;

	public SpriteRenderer Gauge;

	private byte shields;

	public AudioClip ShieldOnSound;

	public AudioClip ShieldOffSound;

	public Transform selectedButtonHighlight;

	private Controller controller = new Controller();

	private int oldSelectedIndex = -1;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		this.shields = this.MyNormTask.Data[0];
		this.UpdateButtons();
		base.SetupInput(true);
	}

	public void ToggleShield(int i)
	{
		if (!this.MyNormTask.IsComplete)
		{
			byte b = (byte)(1 << i);
			this.shields ^= b;
			this.MyNormTask.Data[0] = this.shields;
			if ((this.shields & b) != 0)
			{
				if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySound(this.ShieldOnSound, false, 1f);
				}
			}
			else if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.ShieldOffSound, false, 1f);
			}
			if (this.shields == 127)
			{
				this.MyNormTask.NextStep();
				base.StartCoroutine(base.CoStartClose(0.75f));
				if (ShipStatus.Instance.ShieldsImages.Length != 0 && !ShipStatus.Instance.ShieldsImages[0].IsPlaying())
				{
					PlayerControl.LocalPlayer.RpcPlayAnimation(1);
				}
			}
		}
	}

	private void Update()
	{
		this.controller.Update();
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			Player player = ReInput.players.GetPlayer(0);
			Vector2 normalized = new Vector2(player.GetAxis(13), player.GetAxis(14));
			int num = 0;
			if (normalized.sqrMagnitude > 0.5f)
			{
				normalized = normalized.normalized;
				float num2 = float.NegativeInfinity;
				num = -1;
				Vector2 vector = this.Shields[0].transform.localPosition;
				for (int i = 1; i < this.Shields.Length; i++)
				{
					float num3 = Vector2.Dot(((Vector2) this.Shields[i].transform.localPosition - vector).normalized, normalized);
					if (num == -1 || num3 > num2)
					{
						num = i;
						num2 = num3;
					}
				}
			}
			if (this.oldSelectedIndex != num)
			{
				Vector3 localPosition = this.selectedButtonHighlight.transform.localPosition;
				Vector3 localPosition2 = this.Shields[num].transform.localPosition;
				localPosition.x = localPosition2.x;
				localPosition.y = localPosition2.y;
				this.selectedButtonHighlight.transform.localPosition = localPosition;
				this.oldSelectedIndex = num;
			}
			if (player.GetButtonDown(11))
			{
				this.ToggleShield(num);
			}
		}
	}

	public void FixedUpdate()
	{
		this.UpdateButtons();
	}

	private void UpdateButtons()
	{
		int num = 0;
		for (int i = 0; i < this.Shields.Length; i++)
		{
			bool flag = ((int)this.shields & 1 << i) == 0;
			if (!flag)
			{
				num++;
			}
			this.Shields[i].color = (flag ? this.OffColor : this.OnColor);
		}
		if (this.shields == 127)
		{
			this.Gauge.transform.Rotate(0f, 0f, Time.fixedDeltaTime * 45f);
			this.Gauge.color = new Color(1f, 1f, 1f, 1f);
			return;
		}
		float num2 = Mathf.Lerp(0.1f, 0.5f, (float)num / 6f);
		this.Gauge.color = new Color(1f, num2, num2, 1f);
	}
}
