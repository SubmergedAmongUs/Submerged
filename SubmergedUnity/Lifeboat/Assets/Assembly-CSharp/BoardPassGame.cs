using System;
using System.Collections;
using QRCoder;
using QRCoder.Unity;
using Rewired;
using Rewired.ControllerExtensions;
using TMPro;
using UnityEngine;

public class BoardPassGame : Minigame
{
	private static Color[] BgColors = new Color[]
	{
		new Color32(101, 170, 119, byte.MaxValue),
		new Color32(85, 93, 182, byte.MaxValue),
		new Color32(198, 127, 174, byte.MaxValue),
		new Color32(161, 126, 100, byte.MaxValue),
		new Color32(149, 219, 209, byte.MaxValue)
	};

	public SpriteRenderer renderer;

	public SpriteRenderer pass;

	public Sprite passBack;

	public TextMeshPro NameText;

	public SpriteRenderer ImageBg;

	public SpriteRenderer Image;

	public Sprite[] Photos;

	public PassiveButton pullButton;

	public PassiveButton flipButton;

	public SpriteRenderer Scanner;

	public Sprite ScannerAccept;

	public Sprite ScannerScanning;

	public Sprite ScannerWaiting;

	public Collider2D Sensor;

	public Collider2D BarCode;

	public AudioClip slideinSound;

	public AudioClip flipSound;

	public AudioClip scanStartSound;

	public AudioClip scanSound;

	private Coroutine blinky;

	private Controller controller;

	private TouchpadBehavior touchpad;

	private bool prevHadInput;

	private float rotateAngle;

	private Vector2 prevStickDir;

	private bool enableControllerPassMovement;

	private bool grabbed;

	public void Start()
	{
		Texture2D graphic = null;//new UnityQRCode(new QRCodeGenerator().CreateQrCode("Yo holmes, smell you later", QRCodeGenerator.ECCLevel.M, false, false, 0, -1)).GetGraphic(1);
		this.renderer.sprite = Sprite.Create(graphic, new Rect(0f, 0f, (float)graphic.width, (float)graphic.height), Vector2.one / 2f);
		this.Image.sprite = this.Photos[(int)PlayerControl.LocalPlayer.PlayerId % this.Photos.Length];
		PlayerControl.LocalPlayer.SetPlayerMaterialColors(this.Image);
		this.NameText.text = PlayerControl.LocalPlayer.Data.PlayerName;
		this.touchpad = base.GetComponent<TouchpadBehavior>();
		base.SetupInput(true);
		this.controller = new Controller();
	}

	public void Update()
	{
		this.controller.Update();
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			Player player = ReInput.players.GetPlayer(0);
			player.controllers.Joysticks[0].GetExtension<IDualShock4Extension>();
			if (this.pullButton.gameObject.activeSelf)
			{
				if (player.GetAxisRaw(13) > 0.7f)
				{
					this.PullPass();
					this.pullButton.gameObject.SetActive(false);
				}
				else if (this.touchpad.IsTouching() && this.touchpad.GetTouchVector().x > 1f)
				{
					this.PullPass();
					this.pullButton.gameObject.SetActive(false);
				}
			}
			else if (this.flipButton.gameObject.activeSelf)
			{
				Vector2 axis2DRaw = player.GetAxis2DRaw(13, 14);
				if (axis2DRaw.sqrMagnitude > 0.9f)
				{
					Vector2 normalized = axis2DRaw.normalized;
					if (this.prevHadInput)
					{
						float num = Vector2.SignedAngle(this.prevStickDir, normalized);
						if (num > 0f)
						{
							this.rotateAngle += num;
						}
						if (this.rotateAngle > 45f)
						{
							this.FlipPass();
							this.flipButton.gameObject.SetActive(false);
						}
					}
					this.prevStickDir = normalized;
					this.prevHadInput = true;
				}
				else if (this.touchpad.IsTouching())
				{
					if (this.touchpad.GetTouchVector().x < -1f)
					{
						this.FlipPass();
						this.flipButton.gameObject.SetActive(false);
					}
				}
				else
				{
					this.prevHadInput = false;
				}
			}
			else if (this.enableControllerPassMovement)
			{
				Vector3 localPosition = (Vector3) VirtualCursor.currentPosition - base.transform.position;
				localPosition.z = -1f;
				this.pass.transform.localPosition = localPosition;
			}
		}
		else if (this.grabbed)
		{
			Vector3 localPosition2 = (Vector3) DestroyableSingleton<PassiveButtonManager>.Instance.controller.DragPosition - base.transform.position;
			localPosition2.z = -1f;
			this.pass.transform.localPosition = localPosition2;
		}
		if (!this.flipButton.isActiveAndEnabled && !this.pullButton.isActiveAndEnabled && !this.MyNormTask.IsComplete)
		{
			if (this.Sensor.IsTouching(this.BarCode))
			{
				if (this.blinky == null)
				{
					this.blinky = base.StartCoroutine(this.CoRunBlinky());
					return;
				}
			}
			else if (this.blinky != null)
			{
				base.StopCoroutine(this.blinky);
				this.blinky = null;
				this.Scanner.sprite = this.ScannerWaiting;
			}
		}
	}

	private IEnumerator CoRunBlinky()
	{
		int num;
		for (int i = 0; i < 3; i = num)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.scanStartSound, false, 1f);
			}
			this.Scanner.sprite = this.ScannerAccept;
			yield return Effects.Wait(0.1f);
			this.Scanner.sprite = this.ScannerScanning;
			yield return Effects.Wait(0.2f);
			num = i + 1;
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.scanSound, false, 1f);
		}
		this.blinky = null;
		this.Scanner.sprite = this.ScannerAccept;
		this.MyNormTask.NextStep();
		yield return base.CoStartClose(0.75f);
		yield break;
	}

	public void PullPass()
	{
		base.StartCoroutine(this.CoPullPass());
	}

	private IEnumerator CoPullPass()
	{
		this.pullButton.gameObject.SetActive(false);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.slideinSound, false, 1f);
		}
		yield return Effects.Slide2D(this.pass.transform, new Vector2(-10f, 0f), new Vector2(-1.4f, 0f), 0.3f);
		this.flipButton.gameObject.SetActive(true);
		yield break;
	}

	public void Grab()
	{
		if (!this.flipButton.isActiveAndEnabled && !this.pullButton.isActiveAndEnabled)
		{
			this.grabbed = !this.grabbed;
		}
	}

	public void FlipPass()
	{
		base.StartCoroutine(this.CoFlipPass());
	}

	private IEnumerator CoFlipPass()
	{
		this.flipButton.gameObject.SetActive(false);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.flipSound, false, 1f);
		}
		yield return Effects.Lerp(0.2f, delegate(float t)
		{
			this.pass.transform.localEulerAngles = new Vector3(0f, Mathf.Lerp(0f, 90f, t), 0f);
		});
		this.pass.sprite = this.passBack;
		this.renderer.gameObject.SetActive(false);
		this.ImageBg.gameObject.SetActive(false);
		this.NameText.gameObject.SetActive(false);
		yield return Effects.Lerp(0.2f, delegate(float t)
		{
			this.pass.transform.localEulerAngles = new Vector3(0f, Mathf.Lerp(90f, 180f, t), 0f);
		});
		this.enableControllerPassMovement = true;
		this.inputHandler.disableVirtualCursor = false;
		VirtualCursor.instance.SetWorldPosition(this.pass.transform.position);
		yield break;
	}
}
