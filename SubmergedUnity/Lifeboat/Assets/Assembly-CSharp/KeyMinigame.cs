using System;
using Rewired;
using UnityEngine;

public class KeyMinigame : Minigame
{
	public KeySlotBehaviour[] Slots;

	private int keyState;

	public SpriteRenderer KeyImage;

	public Sprite normalImage;

	public Sprite insertImage;

	public BoxCollider2D key;

	private int targetSlotId;

	private Controller controller = new Controller();

	public AudioClip KeyGrab;

	public AudioClip KeyInsert;

	public AudioClip KeyOpen;

	public AudioClip KeyTurn;

	private TouchpadBehavior touchpad;

	private Vector3 initialPos;

	private bool prevHadInput;

	private Vector2 prevInputDir;

	private float currentAngle;

	public GameObject moveKeyGlyph;

	public GameObject turnKeyGlyph;

	public void Start()
	{
		this.targetSlotId = ((int)(PlayerControl.LocalPlayer ? PlayerControl.LocalPlayer.PlayerId : 0)).Wrap(this.Slots.Length);
		this.Slots[this.targetSlotId].SetHighlight();
		this.touchpad = base.GetComponent<TouchpadBehavior>();
		base.SetupInput(true);
	}

	public void Update()
	{
		if (this.keyState == 2)
		{
			return;
		}
		KeySlotBehaviour keySlotBehaviour = this.Slots[this.targetSlotId];
		this.controller.Update();
		int num = this.keyState;
		if (num != 0)
		{
			if (num == 1)
			{
				if (this.moveKeyGlyph.activeSelf)
				{
					this.moveKeyGlyph.SetActive(false);
				}
				if (!this.turnKeyGlyph.activeSelf)
				{
					this.turnKeyGlyph.SetActive(true);
				}
			}
		}
		else
		{
			if (!this.moveKeyGlyph.activeSelf)
			{
				this.moveKeyGlyph.SetActive(true);
			}
			if (this.turnKeyGlyph.activeSelf)
			{
				this.turnKeyGlyph.SetActive(false);
			}
		}
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			Player player = ReInput.players.GetPlayer(0);
			Vector2 vector = Vector2.zero;
			vector = player.GetAxis2DRaw(13, 14);
			if ((double)vector.sqrMagnitude > 0.5)
			{
				if (!this.prevHadInput)
				{
					if (this.keyState == 0)
					{
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(this.KeyGrab, false, 1f);
						}
					}
					else if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(this.KeyTurn, false, 1f);
					}
				}
				if (this.keyState == 0)
				{
					Vector3 vector2 = vector * Time.deltaTime * 5f;
					this.key.transform.localPosition += vector2;
					if (this.key.IsTouching(keySlotBehaviour.Hitbox))
					{
						this.KeyImage.sprite = this.insertImage;
					}
					else
					{
						this.KeyImage.sprite = this.normalImage;
					}
				}
				else
				{
					vector = vector.normalized;
					if (this.prevHadInput)
					{
						this.currentAngle += Vector2.SignedAngle(this.prevInputDir, vector);
						this.currentAngle = Mathf.Clamp(this.currentAngle, 0f, 90f);
						this.key.transform.localEulerAngles = new Vector3(0f, 0f, this.currentAngle);
					}
					else
					{
						this.currentAngle = this.key.transform.localEulerAngles.z;
					}
					this.prevInputDir = vector;
				}
				this.prevHadInput = true;
				return;
			}
			if (this.touchpad.IsTouching())
			{
				if (this.touchpad.IsFirstTouch())
				{
					this.initialPos = this.key.transform.localPosition;
				}
				vector = this.touchpad.GetTouchVector();
				if (!this.prevHadInput)
				{
					if (this.keyState == 0)
					{
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(this.KeyGrab, false, 1f);
						}
					}
					else if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(this.KeyTurn, false, 1f);
					}
				}
				if (this.keyState == 0)
				{
					this.key.transform.localPosition = (Vector3) vector + this.initialPos;
					if (this.key.IsTouching(keySlotBehaviour.Hitbox))
					{
						this.KeyImage.sprite = this.insertImage;
					}
					else
					{
						this.KeyImage.sprite = this.normalImage;
					}
				}
				else
				{
					vector = this.touchpad.GetCenterToTouch().normalized;
					if (this.prevHadInput)
					{
						this.currentAngle += Vector2.SignedAngle(this.prevInputDir, vector);
						this.currentAngle = Mathf.Clamp(this.currentAngle, 0f, 90f);
						this.key.transform.localEulerAngles = new Vector3(0f, 0f, this.currentAngle);
					}
					else
					{
						this.currentAngle = this.key.transform.localEulerAngles.z;
					}
					this.prevInputDir = vector;
				}
				this.prevHadInput = true;
				return;
			}
			if (this.prevHadInput)
			{
				if (this.keyState == 0)
				{
					if (this.key.IsTouching(keySlotBehaviour.Hitbox))
					{
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(this.KeyInsert, false, 1f);
						}
						this.keyState = 1;
						this.key.size = new Vector2(2f, 2f);
						Vector3 position = keySlotBehaviour.transform.position;
						position.z -= 1f;
						this.key.transform.position = position;
						this.KeyImage.sprite = this.insertImage;
						keySlotBehaviour.SetInserted();
					}
				}
				else
				{
					float num2 = this.key.transform.localEulerAngles.z;
					if (num2 > 180f)
					{
						num2 -= 360f;
					}
					num2 %= 360f;
					if (Mathf.Abs(num2) > 80f)
					{
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(this.KeyOpen, false, 1f);
						}
						keySlotBehaviour.SetFinished();
						this.key.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
						this.keyState = 2;
						this.MyNormTask.NextStep();
						base.StartCoroutine(base.CoStartClose(0.75f));
					}
					else
					{
						this.key.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
					}
				}
				this.prevHadInput = false;
				return;
			}
		}
		else
		{
			switch (this.controller.CheckDrag(this.key))
			{
			case DragState.TouchStart:
				if (this.keyState == 0)
				{
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(this.KeyGrab, false, 1f);
						return;
					}
				}
				else if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySound(this.KeyTurn, false, 1f);
					return;
				}
				break;
			case DragState.Holding:
				break;
			case DragState.Dragging:
			{
				if (this.keyState != 0)
				{
					Vector2 vector3 = this.key.transform.position;
					float num3 = Vector2.SignedAngle(this.controller.DragStartPosition - vector3, this.controller.DragPosition - vector3);
					this.key.transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Clamp(num3, -90f, 90f));
					return;
				}
				Vector2 vector4 = this.controller.DragPosition - (Vector2) base.transform.position;
				this.key.transform.localPosition = vector4;
				if (this.key.IsTouching(keySlotBehaviour.Hitbox))
				{
					this.KeyImage.sprite = this.insertImage;
					return;
				}
				this.KeyImage.sprite = this.normalImage;
				return;
			}
			case DragState.Released:
				if (this.keyState == 0)
				{
					if (this.key.IsTouching(keySlotBehaviour.Hitbox))
					{
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(this.KeyInsert, false, 1f);
						}
						this.keyState = 1;
						this.key.size = new Vector2(2f, 2f);
						Vector3 position2 = keySlotBehaviour.transform.position;
						position2.z -= 1f;
						this.key.transform.position = position2;
						this.KeyImage.sprite = this.insertImage;
						keySlotBehaviour.SetInserted();
						return;
					}
				}
				else
				{
					float num4 = this.key.transform.localEulerAngles.z;
					if (num4 > 180f)
					{
						num4 -= 360f;
					}
					num4 %= 360f;
					if (Mathf.Abs(num4) > 80f)
					{
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(this.KeyOpen, false, 1f);
						}
						keySlotBehaviour.SetFinished();
						this.key.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
						this.keyState = 2;
						this.MyNormTask.NextStep();
						base.StartCoroutine(base.CoStartClose(0.75f));
						return;
					}
					this.key.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
				}
				break;
			default:
				return;
			}
		}
	}
}
