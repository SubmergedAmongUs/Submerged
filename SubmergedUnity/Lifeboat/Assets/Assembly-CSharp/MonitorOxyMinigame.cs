using System;
using System.Linq;
using Rewired;
using UnityEngine;

public class MonitorOxyMinigame : Minigame
{
	public SpriteRenderer[] Targets;

	public BoxCollider2D[] Sliders;

	public VerticalSpriteGauge[] Fills;

	public FloatRange YRange;

	public FloatRange[] RandomRanges;

	private Controller controller = new Controller();

	public AudioClip[] DragSounds;

	private AudioSource ActiveSound;

	public Transform selectorObject;

	private TouchpadBehavior touchpad;

	private float initialY;

	private int selectedIndex;

	private bool prevHadInput;

	private float selectCooldown;

	private bool isTouchInput;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		for (int i = 0; i < this.Sliders.Length; i++)
		{
			BoxCollider2D boxCollider2D = this.Sliders[i];
			Vector3 localPosition = boxCollider2D.transform.localPosition;
			localPosition.y = this.RandomRanges[i].Next();
			boxCollider2D.transform.localPosition = localPosition;
			float value = this.YRange.ReverseLerp(localPosition.y);
			this.Fills[i].Value = value;
		}
		this.touchpad = base.GetComponent<TouchpadBehavior>();
		base.SetupInput(true);
	}

	public void Update()
	{
		this.controller.Update();
		Vector2 vector = ReInput.players.GetPlayer(0).GetAxis2DRaw(13, 14);
		if (this.touchpad.IsTouching())
		{
			vector = this.touchpad.GetTouchVector();
			vector.x *= 0.5f;
		}
		if (Mathf.Abs(vector.x) > 0.7f)
		{
			if (this.selectCooldown <= 0f)
			{
				this.selectedIndex += (int)Mathf.Sign(vector.x);
				this.selectedIndex = Mathf.Clamp(this.selectedIndex, 0, this.Sliders.Length - 1);
				this.selectorObject.SetParent(this.Sliders[this.selectedIndex].transform, false);
				if (this.prevHadInput)
				{
					if (this.ActiveSound)
					{
						this.ActiveSound.Stop();
					}
					this.prevHadInput = false;
				}
				this.selectCooldown = 0.2f;
			}
		}
		else
		{
			this.selectCooldown = 0f;
		}
		BoxCollider2D boxCollider2D = this.Sliders[this.selectedIndex];
		if (boxCollider2D.enabled && Mathf.Abs(vector.x) < 0.7f && !this.isTouchInput)
		{
			if (Mathf.Abs(vector.y) > 0.05f || this.touchpad.IsTouching())
			{
				if (!this.prevHadInput && Constants.ShouldPlaySfx())
				{
					this.ActiveSound = SoundManager.Instance.PlaySound(this.DragSounds[this.selectedIndex], true, 0.7f);
				}
				Vector3 localPosition = boxCollider2D.transform.localPosition;
				if (this.touchpad.IsTouching())
				{
					if (this.touchpad.IsFirstTouch())
					{
						this.initialY = boxCollider2D.transform.localPosition.y;
					}
					localPosition.y = vector.y + this.initialY;
				}
				else
				{
					localPosition.y += vector.y * 2f * Time.deltaTime;
				}
				localPosition.y = this.YRange.Clamp(localPosition.y);
				boxCollider2D.transform.localPosition = localPosition;
				float num = this.YRange.ReverseLerp(localPosition.y);
				this.Fills[this.selectedIndex].Value = num;
				if (this.ActiveSound)
				{
					this.ActiveSound.pitch = Mathf.Lerp(0.8f, 1.2f, num);
				}
				this.prevHadInput = true;
			}
			else
			{
				if (this.prevHadInput && this.ActiveSound)
				{
					this.ActiveSound.Stop();
				}
				SpriteRenderer spriteRenderer = this.Targets[this.selectedIndex];
				if (Mathf.Abs(boxCollider2D.transform.localPosition.y - spriteRenderer.transform.localPosition.y) < 0.1f)
				{
					boxCollider2D.enabled = false;
					spriteRenderer.color = Color.green;
					if (this.Sliders.All((BoxCollider2D s) => !s.enabled))
					{
						this.MyNormTask.NextStep();
						base.StartCoroutine(base.CoStartClose(0.75f));
					}
					VibrationManager.Vibrate(0.2f, 0.2f, 0.2f, VibrationManager.VibrationFalloff.None, null, false);
				}
				this.prevHadInput = false;
			}
		}
		for (int i = 0; i < this.Sliders.Length; i++)
		{
			BoxCollider2D boxCollider2D2 = this.Sliders[i];
			if (boxCollider2D2.enabled)
			{
				switch (this.controller.CheckDrag(boxCollider2D2))
				{
				case DragState.TouchStart:
					if (Constants.ShouldPlaySfx())
					{
						this.ActiveSound = SoundManager.Instance.PlaySound(this.DragSounds[i], true, 0.7f);
					}
					this.isTouchInput = true;
					break;
				case DragState.Dragging:
				{
					Vector2 vector2 = this.controller.DragPosition - (Vector2) boxCollider2D2.transform.parent.position;
					Vector3 localPosition2 = boxCollider2D2.transform.localPosition;
					localPosition2.y = this.YRange.Clamp(vector2.y);
					boxCollider2D2.transform.localPosition = localPosition2;
					float num2 = this.YRange.ReverseLerp(localPosition2.y);
					this.Fills[i].Value = num2;
					if (this.ActiveSound)
					{
						this.ActiveSound.pitch = Mathf.Lerp(0.8f, 1.2f, num2);
					}
					break;
				}
				case DragState.Released:
				{
					if (this.ActiveSound)
					{
						this.ActiveSound.Stop();
					}
					SpriteRenderer spriteRenderer2 = this.Targets[i];
					if (Mathf.Abs(boxCollider2D2.transform.localPosition.y - spriteRenderer2.transform.localPosition.y) < 0.1f)
					{
						boxCollider2D2.enabled = false;
						spriteRenderer2.color = Color.green;
						if (this.Sliders.All((BoxCollider2D s) => !s.enabled))
						{
							this.MyNormTask.NextStep();
							base.StartCoroutine(base.CoStartClose(0.75f));
						}
						VibrationManager.Vibrate(0.2f, 0.2f, 0.2f, VibrationManager.VibrationFalloff.None, null, false);
					}
					this.isTouchInput = false;
					break;
				}
				}
			}
		}
	}

	public override void Close()
	{
		if (this.ActiveSound)
		{
			this.ActiveSound.Stop();
		}
		base.Close();
	}
}
