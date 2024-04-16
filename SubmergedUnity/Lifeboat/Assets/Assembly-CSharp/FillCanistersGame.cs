using System;
using System.Collections;
using Rewired;
using UnityEngine;

public class FillCanistersGame : Minigame
{
	private Vector3 CanisterAppearPosition = new Vector3(0f, 4.5f, 0f);

	private Vector3 CanisterStartPosition = new Vector3(-0.75f, 1.5f, 0f);

	private Vector3 CanisterDragPosition = new Vector3(0.4f, -1f, 0f);

	private Vector3 CanisterSnapPosition = new Vector3(0f, -1f, 0f);

	private Vector3 CanisterAwayPosition = new Vector3(8f, -1f, 0f);

	public float FillTime = 2.5f;

	public CanisterBehaviour Canister;

	private Controller controller = new Controller();

	public AudioClip FillLoop;

	public AudioClip DropSound;

	public AudioClip GrabSound;

	public AudioClip PlugInSound;

	public AudioClip PlugOutSound;

	private TouchpadBehavior touchpad;

	private bool prevHadInput;

	public void Start()
	{
		base.StartCoroutine(this.Run());
		this.touchpad = base.GetComponent<TouchpadBehavior>();
		base.SetupInput(true);
	}

	public override void Close()
	{
		SoundManager.Instance.StopSound(this.FillLoop);
		base.Close();
	}

	private IEnumerator Run()
	{
		for (;;)
		{
			this.Canister.Gauge.Value = 0f;
			yield return Effects.Slide2D(this.Canister.transform, this.CanisterAppearPosition, this.CanisterStartPosition, 0.1f);
			this.controller.ClearTouch();
			for (;;)
			{
				this.controller.Update();
				if (Controller.currentTouchType == Controller.TouchType.Joystick)
				{
					float num = ReInput.players.GetPlayer(0).GetAxisRaw(14);
					if (this.touchpad.IsTouching())
					{
						num = this.touchpad.GetTouchVector().y;
					}
					if (Mathf.Abs(num) > 0.2f)
					{
						if (!this.prevHadInput && Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(this.GrabSound, false, 1f);
						}
						float num2 = FloatRange.ReverseLerp(this.Canister.transform.localPosition.y, this.CanisterDragPosition.y, this.CanisterStartPosition.y);
						num2 += num * 3f * Time.deltaTime;
						num2 = Mathf.Clamp01(num2);
						this.Canister.transform.localPosition = Vector3.Lerp(this.CanisterDragPosition, this.CanisterStartPosition, num2);
						this.prevHadInput = true;
					}
					else
					{
						if (this.prevHadInput)
						{
							if (Constants.ShouldPlaySfx())
							{
								SoundManager.Instance.PlaySound(this.DropSound, false, 1f);
							}
							if (FloatRange.ReverseLerp(this.Canister.transform.localPosition.y, this.CanisterDragPosition.y, this.CanisterStartPosition.y) < 0.05f)
							{
								goto Block_7;
							}
						}
						this.prevHadInput = false;
					}
				}
				else
				{
					switch (this.controller.CheckDrag(this.Canister.Hitbox))
					{
					case DragState.TouchStart:
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(this.GrabSound, false, 1f);
						}
						break;
					case DragState.Dragging:
					{
						float num3 = FloatRange.ReverseLerp((this.controller.DragPosition - (Vector2) base.transform.position).y, this.CanisterDragPosition.y, this.CanisterStartPosition.y);
						this.Canister.transform.localPosition = Vector3.Lerp(this.CanisterDragPosition, this.CanisterStartPosition, num3);
						break;
					}
					case DragState.Released:
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(this.DropSound, false, 1f);
						}
						if (FloatRange.ReverseLerp(this.Canister.transform.localPosition.y, this.CanisterDragPosition.y, this.CanisterStartPosition.y) < 0.05f)
						{
							goto Block_12;
						}
						break;
					}
				}
				yield return null;
			}
			IL_375:
			AudioSource fillSound = null;
			if (Constants.ShouldPlaySfx())
			{
				fillSound = SoundManager.Instance.PlaySound(this.FillLoop, true, 1f);
			}
			yield return Effects.Slide2D(this.Canister.transform, this.CanisterDragPosition, this.CanisterSnapPosition, 0.1f);
			yield return Effects.Lerp(this.FillTime, delegate(float t)
			{
				this.Canister.Gauge.Value = t;
			});
			if (fillSound)
			{
				fillSound.Stop();
			}
			Player player = ReInput.players.GetPlayer(0);
			float stickInput = 0f;
			stickInput = player.GetAxisRaw(13);
			if (this.touchpad.IsTouching())
			{
				stickInput = this.touchpad.GetTouchVector().x;
			}
			bool hasNoRemoveInput = this.controller.CheckDrag(this.Canister.Hitbox) != DragState.TouchStart && player.GetAxisRaw(13) < 0.9f;
			while (hasNoRemoveInput)
			{
				this.controller.Update();
				hasNoRemoveInput = (this.controller.CheckDrag(this.Canister.Hitbox) != DragState.TouchStart && stickInput < 0.9f);
				if (this.touchpad.IsTouching())
				{
					stickInput = this.touchpad.GetTouchVector().x;
				}
				else
				{
					stickInput = player.GetAxisRaw(13);
				}
				yield return null;
			}
			player = null;
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.PlugOutSound, false, 1f);
			}
			yield return Effects.Slide2D(this.Canister.transform, this.CanisterSnapPosition, this.CanisterAwayPosition, 0.3f);
			this.MyNormTask.NextStep();
			if (this.MyNormTask.IsComplete)
			{
				break;
			}
			continue;
			Block_7:
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.PlugInSound, false, 1f);
				goto IL_375;
			}
			goto IL_375;
			Block_12:
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.PlugInSound, false, 1f);
				goto IL_375;
			}
			goto IL_375;
		}
		base.StartCoroutine(base.CoStartClose(0.75f));
		yield break;
	}
}
