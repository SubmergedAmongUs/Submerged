using System;
using System.Collections;
using Rewired;
using UnityEngine;

public class FixShowerMinigame : Minigame
{
	private const float PowerRatio = 0.32f;

	private const float BasePower = 0.04f;

	private const float CompleteTolerance = 0.07f;

	private FloatRange hammerAngles = new FloatRange(0f, -45f);

	private FloatRange showerAngles = new FloatRange(-30f, 30f);

	private float showerPos;

	public SpriteRenderer mallet;

	public Collider2D showerHead;

	public AnimationCurve hammerAnim;

	private Controller controller = new Controller();

	private float powerTime;

	public PowerBar powerBar;

	public AudioClip[] bashSounds;

	public AudioClip swingSound;

	public GameObject leftGlyph;

	public GameObject rightGlyph;

	private bool prevButtonHeld;

	private bool animating;

	private float Power
	{
		get
		{
			return Mathf.Sin(-1.5707964f + this.powerTime * 4f) / 2f + 0.5f;
		}
	}

	public void Start()
	{
		this.showerPos = BitConverter.ToSingle(this.MyNormTask.Data, 0);
		this.showerHead.transform.localEulerAngles = new Vector3(0f, 0f, this.showerAngles.Lerp(this.showerPos));
		this.powerBar.gameObject.SetActive(false);
		this.mallet.enabled = false;
		base.SetupInput(true);
	}

	public void Update()
	{
		if (this.amClosing != Minigame.CloseState.None)
		{
			return;
		}
		if (this.animating)
		{
			return;
		}
		this.controller.Update();
		if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
		{
			Player player = ReInput.players.GetPlayer(0);
			bool flag = this.showerPos < 0.5f;
			if (this.leftGlyph.activeSelf != flag)
			{
				this.leftGlyph.SetActive(flag);
			}
			if (this.rightGlyph.activeSelf == flag)
			{
				this.rightGlyph.SetActive(!flag);
			}
			bool button = player.GetButton(flag ? 24 : 21);
			if (button)
			{
				if (!this.prevButtonHeld)
				{
					this.powerBar.gameObject.SetActive(true);
					this.powerTime = 0f;
					this.powerBar.SetValue(0f);
				}
				else
				{
					this.powerTime += Time.deltaTime;
					this.powerBar.SetValue(this.Power);
				}
			}
			else if (this.prevButtonHeld)
			{
				this.powerBar.gameObject.SetActive(false);
				base.StartCoroutine(this.Bash(this.Power * 0.32f + 0.04f));
			}
			this.prevButtonHeld = button;
			return;
		}
		switch (this.controller.CheckDrag(this.showerHead))
		{
		case DragState.TouchStart:
			this.powerBar.gameObject.SetActive(true);
			this.powerTime = 0f;
			this.powerBar.SetValue(0f);
			return;
		case DragState.Holding:
		case DragState.Dragging:
			this.powerTime += Time.deltaTime;
			this.powerBar.SetValue(this.Power);
			return;
		case DragState.Released:
			this.powerBar.gameObject.SetActive(false);
			base.StartCoroutine(this.Bash(this.Power * 0.32f + 0.04f));
			return;
		default:
			return;
		}
	}

	public IEnumerator Bash(float power)
	{
		this.animating = true;
		this.mallet.transform.localEulerAngles = Vector3.zero;
		if ((double)this.showerPos < 0.5)
		{
			this.mallet.flipX = false;
			this.mallet.transform.localPosition = new Vector3(-2.5f, -0.4f, 0f);
		}
		else
		{
			this.mallet.flipX = true;
			this.mallet.transform.localPosition = new Vector3(2.5f, -0.4f, 0f);
		}
		this.mallet.enabled = true;
		yield return Effects.Wait(0.05f);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.swingSound, false, 1f);
		}
		yield return Effects.Lerp(0.15f, delegate(float t)
		{
			float num = this.hammerAngles.Lerp(this.hammerAnim.Evaluate(t));
			if (this.mallet.flipX)
			{
				num = -num;
			}
			this.mallet.transform.localEulerAngles = new Vector3(0f, 0f, num);
		});
		if (this.showerPos < 0.5f)
		{
			VibrationManager.Vibrate(power, 0f, 0.3f, VibrationManager.VibrationFalloff.Linear, null, false);
		}
		else
		{
			VibrationManager.Vibrate(0f, power, 0.3f, VibrationManager.VibrationFalloff.Linear, null, false);
		}
		if ((double)this.showerPos > 0.5)
		{
			power = -power;
		}
		this.showerPos += power;
		this.showerHead.transform.localEulerAngles = new Vector3(0f, 0f, this.showerAngles.Lerp(this.showerPos));
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.bashSounds.Random<AudioClip>(), false, 1f);
		}
		yield return Effects.Wait(0.05f);
		this.mallet.enabled = false;
		if (Mathf.Abs(this.showerPos - 0.5f) < 0.07f)
		{
			this.MyNormTask.NextStep();
			yield return base.CoStartClose(0.75f);
		}
		this.animating = false;
		yield break;
	}

	public override void Close()
	{
		this.MyNormTask.Data = BitConverter.GetBytes(this.showerPos);
		base.Close();
	}
}
