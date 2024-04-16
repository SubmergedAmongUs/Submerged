using System;
using TMPro;
using UnityEngine;

public class ReactorMinigame : Minigame
{
	private Color bad = new Color(1f, 0.16078432f, 0f);

	private Color good = new Color(0.3019608f, 0.8862745f, 0.8352941f);

	private ReactorSystemType reactor;

	public TextMeshPro statusText;

	public SpriteRenderer hand;

	private FloatRange YSweep = new FloatRange(-2.15f, 1.56f);

	public SpriteRenderer sweeper;

	public AudioClip HandSound;

	private bool isButtonDown;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		this.reactor = (ShipStatus.Instance.Systems[this.MyTask.StartAt] as ReactorSystemType);
		this.hand.color = this.bad;
		base.SetupInput(false);
	}

	public void ButtonDown()
	{
		if (!this.reactor.IsActive || this.amClosing != Minigame.CloseState.None)
		{
			return;
		}
		this.isButtonDown = !this.isButtonDown;
		if (this.isButtonDown)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.HandSound, true, 1f);
			}
			ShipStatus.Instance.RpcRepairSystem(this.MyTask.StartAt, (int)((byte)(64 | base.ConsoleId)));
		}
		else
		{
			SoundManager.Instance.StopSound(this.HandSound);
			ShipStatus.Instance.RpcRepairSystem(this.MyTask.StartAt, (int)((byte)(32 | base.ConsoleId)));
		}
		try
		{
			((SabotageTask)this.MyTask).MarkContributed();
		}
		catch
		{
		}
	}

	public void FixedUpdate()
	{
		VirtualCursor.instance.SetScreenPosition(Vector2.zero);
		if (!this.reactor.IsActive)
		{
			if (this.amClosing == Minigame.CloseState.None)
			{
				this.hand.color = this.good;
				this.statusText.text = DestroyableSingleton<TranslationController>.Instance.GetString((this.MyTask.StartAt == SystemTypes.Reactor) ? StringNames.ReactorNominal : StringNames.SeismicNominal, Array.Empty<object>());
				this.sweeper.enabled = false;
				SoundManager.Instance.StopSound(this.HandSound);
				base.StartCoroutine(base.CoStartClose(0.75f));
				return;
			}
		}
		else
		{
			if (!this.isButtonDown)
			{
				this.statusText.text = DestroyableSingleton<TranslationController>.Instance.GetString((this.MyTask.StartAt == SystemTypes.Reactor) ? StringNames.ReactorHoldToStop : StringNames.SeismicHoldToStop, Array.Empty<object>());
				this.sweeper.enabled = false;
				return;
			}
			this.statusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ReactorWaiting, Array.Empty<object>());
			Vector3 localPosition = this.sweeper.transform.localPosition;
			localPosition.y = this.YSweep.Lerp(Mathf.Sin(Time.time) * 0.5f + 0.5f);
			this.sweeper.transform.localPosition = localPosition;
			this.sweeper.enabled = true;
		}
	}

	public override void Close()
	{
		SoundManager.Instance.StopSound(this.HandSound);
		if (ShipStatus.Instance)
		{
			ShipStatus.Instance.RpcRepairSystem(this.MyTask.StartAt, (int)((byte)(32 | base.ConsoleId)));
		}
		base.Close();
	}
}
