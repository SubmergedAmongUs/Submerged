using System;
using System.Collections;
using Rewired;
using TMPro;
using UnityEngine;

public class DoorCardSwipeGame : Minigame, IDoorMinigame
{
	private Color gray = new Color(0.45f, 0.45f, 0.45f);

	private Color green = new Color(0f, 0.8f, 0f);

	private DoorCardSwipeGame.TaskStages State;

	private Controller myController = new Controller();

	private FloatRange YRange = new FloatRange(-1.77f, 2f);

	public float minAcceptedTime = 0.3f;

	public Collider2D col;

	public SpriteRenderer confirmSymbol;

	public Sprite AcceptSymbol;

	public Sprite RejectSymbol;

	public TextMeshPro StatusText;

	public AudioClip AcceptSound;

	public AudioClip DenySound;

	public AudioClip[] CardMove;

	public AudioClip WalletOut;

	public float dragTime;

	private bool moving;

	private Vector2 prevStickInput = Vector2.zero;

	private bool hadPrev;

	private PlainDoor MyDoor;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		this.StatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardPleaseInsert, Array.Empty<object>());
		base.SetupInput(true);
	}

	public void Update()
	{
		if (this.amClosing != Minigame.CloseState.None)
		{
			return;
		}
		Vector3 localPosition = this.col.transform.localPosition;
		this.myController.Update();
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			Player player = ReInput.players.GetPlayer(0);
			Vector2 vector = player.GetAxis2DRaw(13, 14);
			float magnitude = vector.magnitude;
			DoorCardSwipeGame.TaskStages state = this.State;
			if (state != DoorCardSwipeGame.TaskStages.Before)
			{
				if (state == DoorCardSwipeGame.TaskStages.Inserted)
				{
					if (magnitude > 0.9f)
					{
						vector = vector.normalized;
						if (this.hadPrev)
						{
							float num = this.prevStickInput.AngleSigned(vector);
							if (num > 180f)
							{
								num -= 360f;
							}
							if (num < -180f)
							{
								num += 360f;
							}
							float num2 = Mathf.Abs(num) * 0.025f;
							float y = localPosition.y;
							localPosition.y -= num2;
							if (num2 > 0.01f)
							{
								this.dragTime += Time.deltaTime;
								if (!this.moving)
								{
									this.moving = true;
									if (Constants.ShouldPlaySfx())
									{
										SoundManager.Instance.PlaySound(this.CardMove.Random<AudioClip>(), false, 1f);
									}
								}
							}
							localPosition.y = this.YRange.Clamp(localPosition.y);
							float num3 = localPosition.y - y;
							float num4 = this.YRange.ReverseLerp(localPosition.y);
							float num5 = 0.8f * num3;
							VibrationManager.Vibrate(num5 * (1f - num4), num5 * num4, 0.01f, VibrationManager.VibrationFalloff.None, null, false);
						}
						else
						{
							this.dragTime = 0f;
						}
						this.prevStickInput = vector;
						this.hadPrev = true;
					}
					else
					{
						if (this.hadPrev)
						{
							if (localPosition.y - this.YRange.min < 0.05f && !BoolRange.Next(0.01f))
							{
								if (this.dragTime > this.minAcceptedTime)
								{
									if (Constants.ShouldPlaySfx())
									{
										SoundManager.Instance.PlaySound(this.AcceptSound, false, 1f);
									}
									this.State = DoorCardSwipeGame.TaskStages.After;
									this.StatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardAccepted, Array.Empty<object>());
									base.StartCoroutine(this.PutCardBack());
									ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, this.MyDoor.Id | 64);
									this.MyDoor.SetDoorway(true);
									base.StartCoroutine(base.CoStartClose(0.4f));
									this.confirmSymbol.sprite = this.AcceptSymbol;
								}
								else
								{
									if (Constants.ShouldPlaySfx())
									{
										SoundManager.Instance.PlaySound(this.DenySound, false, 1f);
									}
									this.StatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardTooFast, Array.Empty<object>());
									this.confirmSymbol.sprite = this.RejectSymbol;
								}
							}
							else
							{
								if (Constants.ShouldPlaySfx())
								{
									SoundManager.Instance.PlaySound(this.DenySound, false, 1f);
								}
								this.StatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardBadRead, Array.Empty<object>());
								this.confirmSymbol.sprite = this.RejectSymbol;
							}
						}
						localPosition.y = Mathf.Lerp(localPosition.y, this.YRange.max, Time.deltaTime * 4f);
						this.hadPrev = false;
						this.dragTime = 0f;
					}
				}
			}
			else if (player.GetAnyButtonDown())
			{
				this.State = DoorCardSwipeGame.TaskStages.Animating;
				base.StartCoroutine(this.InsertCard());
			}
		}
		else
		{
			switch (this.myController.CheckDrag(this.col))
			{
			case DragState.NoTouch:
				if (this.State == DoorCardSwipeGame.TaskStages.Inserted)
				{
					localPosition.y = Mathf.Lerp(localPosition.y, this.YRange.max, Time.deltaTime * 4f);
				}
				break;
			case DragState.TouchStart:
				this.dragTime = 0f;
				break;
			case DragState.Dragging:
				if (this.State == DoorCardSwipeGame.TaskStages.Inserted)
				{
					Vector2 vector2 = this.myController.DragPosition - (Vector2) base.transform.position;
					vector2.y = this.YRange.Clamp(vector2.y);
					if (localPosition.y - vector2.y > 0.01f)
					{
						this.dragTime += Time.deltaTime;
						this.confirmSymbol.sprite = null;
						if (!this.moving)
						{
							this.moving = true;
							if (Constants.ShouldPlaySfx())
							{
								SoundManager.Instance.PlaySound(this.CardMove.Random<AudioClip>(), false, 1f);
							}
						}
					}
					localPosition.y = vector2.y;
				}
				break;
			case DragState.Released:
				this.moving = false;
				if (this.State == DoorCardSwipeGame.TaskStages.Before)
				{
					this.State = DoorCardSwipeGame.TaskStages.Animating;
					base.StartCoroutine(this.InsertCard());
				}
				else if (this.State == DoorCardSwipeGame.TaskStages.Inserted)
				{
					if (localPosition.y - this.YRange.min < 0.05f && !BoolRange.Next(0.01f))
					{
						if (this.dragTime > this.minAcceptedTime)
						{
							if (Constants.ShouldPlaySfx())
							{
								SoundManager.Instance.PlaySound(this.AcceptSound, false, 1f);
							}
							this.State = DoorCardSwipeGame.TaskStages.After;
							this.StatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardAccepted, Array.Empty<object>());
							base.StartCoroutine(this.PutCardBack());
							ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, this.MyDoor.Id | 64);
							this.MyDoor.SetDoorway(true);
							base.StartCoroutine(base.CoStartClose(0.4f));
							this.confirmSymbol.sprite = this.AcceptSymbol;
						}
						else
						{
							if (Constants.ShouldPlaySfx())
							{
								SoundManager.Instance.PlaySound(this.DenySound, false, 1f);
							}
							this.StatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardTooFast, Array.Empty<object>());
							this.confirmSymbol.sprite = this.RejectSymbol;
						}
					}
					else
					{
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(this.DenySound, false, 1f);
						}
						this.StatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardBadRead, Array.Empty<object>());
						this.confirmSymbol.sprite = this.RejectSymbol;
					}
				}
				break;
			}
		}
		this.col.transform.localPosition = localPosition;
	}

	private IEnumerator PutCardBack()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.WalletOut, false, 1f);
		}
		Vector3 localPosition = this.col.transform.localPosition;
		Vector3 dest = new Vector3(0.452f, -1.9f, 0f);
		yield return Effects.All(new IEnumerator[]
		{
			Effects.Rotate2D(this.col.transform, 90f, 0f, 0.4f),
			Effects.Slide3D(this.col.transform, localPosition, dest, 0.4f)
		});
		this.StatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardPleaseInsert, Array.Empty<object>());
		this.State = DoorCardSwipeGame.TaskStages.Before;
		yield break;
	}

	private IEnumerator InsertCard()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.WalletOut, false, 1f);
		}
		Vector3 localPosition = this.col.transform.localPosition;
		Vector3 dest = new Vector3(-1.43f, this.YRange.max, 0f);
		yield return Effects.All(new IEnumerator[]
		{
			Effects.Rotate2D(this.col.transform, 0f, 90f, 0.4f),
			Effects.Slide3D(this.col.transform, localPosition, dest, 0.4f)
		});
		this.StatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardPleaseSwipe, Array.Empty<object>());
		this.State = DoorCardSwipeGame.TaskStages.Inserted;
		yield break;
	}

	public void SetDoor(PlainDoor door)
	{
		this.MyDoor = door;
	}

	private enum TaskStages
	{
		Before,
		Animating,
		Inserted,
		After
	}
}
