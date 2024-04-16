using System;
using Rewired;
using UnityEngine;
using Object = UnityEngine.Object;

public class AirshipGarbageGame : Minigame
{
	public GarbageCanBehaviour[] GarbagePrefabs;

	public Sprite RelaxeHandle;

	public Sprite PulledHandle;

	private GarbageCanBehaviour can;

	public AudioClip grabSound;

	public Controller controller = new Controller();

	public Transform handCursorObject;

	public GameObject waitingHands;

	public GameObject grabbedHands;

	public SpriteRenderer[] handSprites;

	private bool prevHadLeftInput;

	private const float stickVelocityMagnitude = 6f;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		this.can = UnityEngine.Object.Instantiate<GarbageCanBehaviour>(this.GarbagePrefabs[base.ConsoleId], base.transform);
		base.SetupInput(true);
		foreach (SpriteRenderer playerMaterialColors in this.handSprites)
		{
			PlayerControl.LocalPlayer.SetPlayerMaterialColors(playerMaterialColors);
		}
	}

	private void Update()
	{
		if (this.amOpening)
		{
			return;
		}
		if (this.amClosing != Minigame.CloseState.None)
		{
			return;
		}
		this.controller.Update();
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			Player player = ReInput.players.GetPlayer(0);
			this.handCursorObject.position = this.can.Handle.transform.position;
			if (player.GetButton(24) && player.GetButton(21))
			{
				if (!this.grabbedHands.activeSelf)
				{
					this.grabbedHands.SetActive(true);
					this.waitingHands.SetActive(false);
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(this.grabSound, false, 1f);
					}
				}
				this.can.Handle.sprite = this.PulledHandle;
				Vector2 axis2DRaw = player.GetAxis2DRaw(13, 14);
				if (axis2DRaw.magnitude > 0.9f)
				{
					if (!this.prevHadLeftInput)
					{
						this.can.Body.velocity = axis2DRaw.normalized * 6f;
					}
					this.prevHadLeftInput = true;
				}
				else
				{
					this.prevHadLeftInput = false;
				}
			}
			else
			{
				if (!this.waitingHands.activeSelf)
				{
					this.grabbedHands.SetActive(false);
					this.waitingHands.SetActive(true);
				}
				this.can.Handle.sprite = this.RelaxeHandle;
			}
		}
		else
		{
			DragState dragState = this.controller.CheckDrag(this.can.Hitbox);
			if (dragState != DragState.TouchStart)
			{
				if (dragState != DragState.Dragging)
				{
					this.can.Handle.sprite = this.RelaxeHandle;
				}
				else
				{
					this.can.Handle.sprite = this.PulledHandle;
					Vector2 vector = this.controller.DragPosition - (Vector2) this.can.Handle.transform.position;
					this.can.Body.velocity = 10f * vector;
				}
			}
			else if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.grabSound, false, 1f);
			}
		}
		if (!this.can.Body.IsTouching(this.can.Success))
		{
			this.MyNormTask.NextStep();
			base.StartCoroutine(base.CoStartClose(0.7f));
		}
	}
}
