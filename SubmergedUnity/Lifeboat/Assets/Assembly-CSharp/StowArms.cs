using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class StowArms : Minigame
{
	public GameObject GunContent;

	public GameObject RifleContent;

	public Transform selectorObject;

	public GameObject[] selectorSubobjects;

	public SpriteRenderer[] handSprites;

	public AudioClip pickupGun;

	public AudioClip placeGun;

	public Collider2D[] GunColliders;

	public DragSlot[] GunsSlots;

	public AudioClip pickupRifle;

	public AudioClip placeRifle;

	public Collider2D[] RifleColliders;

	public DragSlot[] RifleSlots;

	private Controller cont = new Controller();

	private Collider2D currentGrabbedObject;

	private Vector3 grabOffset;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		if (base.ConsoleId == 1)
		{
			this.GunContent.SetActive(true);
			this.RifleContent.SetActive(false);
		}
		else
		{
			this.GunContent.SetActive(false);
			this.RifleContent.SetActive(true);
		}
		foreach (SpriteRenderer playerMaterialColors in this.handSprites)
		{
			PlayerControl.LocalPlayer.SetPlayerMaterialColors(playerMaterialColors);
		}
		base.SetupInput(false);
	}

	private AudioClip PickupSound
	{
		get
		{
			if (base.ConsoleId != 1)
			{
				return this.pickupRifle;
			}
			return this.pickupGun;
		}
	}

	private AudioClip PlaceSound
	{
		get
		{
			if (base.ConsoleId != 1)
			{
				return this.placeRifle;
			}
			return this.placeGun;
		}
	}

	public void Update()
	{
		void ValidateSelectorActive(GameObject selector, bool shouldBeActive)
		{
			if (selector.gameObject.activeSelf != shouldBeActive)
			{
				selector.gameObject.SetActive(shouldBeActive);
			}
		}

		this.cont.Update();
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			if (!this.selectorObject.gameObject.activeSelf)
			{
				this.selectorObject.gameObject.SetActive(true);
			}
		}
		else if (this.selectorObject.gameObject.activeSelf)
		{
			this.selectorObject.gameObject.SetActive(false);
		}
		if (base.ConsoleId == 1)
		{
			this.DoUpdate(this.GunColliders, this.GunsSlots);
		}
		else
		{
			this.DoUpdate(this.RifleColliders, this.RifleSlots);
		}
		if (this.currentGrabbedObject)
		{
			this.selectorObject.position = this.currentGrabbedObject.transform.position;
			this.selectorObject.SetLocalZ(0f);
			ValidateSelectorActive(this.selectorSubobjects[0], false);
			ValidateSelectorActive(this.selectorSubobjects[1], base.ConsoleId == 1);
			ValidateSelectorActive(this.selectorSubobjects[2], base.ConsoleId != 1);
			return;
		}
		this.selectorObject.position = VirtualCursor.currentPosition;
		this.selectorObject.SetLocalZ(0f);
		ValidateSelectorActive(this.selectorSubobjects[0], true);
		ValidateSelectorActive(this.selectorSubobjects[1], false);
		ValidateSelectorActive(this.selectorSubobjects[2], false);
	}

	private void DoUpdate(Collider2D[] colliders, DragSlot[] slots)
	{
		this.currentGrabbedObject = null;
		for (int i = 0; i < colliders.Length; i++)
		{
			Collider2D collider2D = colliders[i];
			DragSlot dragSlot = slots[i];
			if (!(dragSlot.Occupant == collider2D))
			{
				switch (this.cont.CheckDrag(collider2D))
				{
				case DragState.TouchStart:
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(this.PickupSound, false, 1f).pitch = FloatRange.Next(0.8f, 1.2f);
					}
					this.currentGrabbedObject = collider2D;
					this.grabOffset = collider2D.transform.position - (Vector3) this.cont.DragStartPosition;
					break;
				case DragState.Holding:
				case DragState.Dragging:
				{
					this.currentGrabbedObject = collider2D;
					Vector3 position = base.transform.position;
					Vector3 position2 = (Vector3) this.cont.DragPosition + this.grabOffset;
					position2.z = position.z;
					collider2D.transform.position = position2;
					if (Vector2.Distance(collider2D.transform.position, dragSlot.TargetPosition) < 0.25f && !dragSlot.Occupant)
					{
						collider2D.transform.position = dragSlot.TargetPosition;
					}
					break;
				}
				case DragState.Released:
					if (Vector2.Distance(collider2D.transform.position, dragSlot.TargetPosition) < 0.25f && !dragSlot.Occupant)
					{
						collider2D.transform.position = dragSlot.TargetPosition;
						dragSlot.Occupant = collider2D;
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(this.PlaceSound, false, 1f).pitch = FloatRange.Next(0.8f, 1.2f);
						}
					}
					this.CheckForWin(colliders, slots);
					break;
				}
			}
		}
	}

	private void CheckForWin(Collider2D[] colliders, DragSlot[] slots)
	{
		bool flag = true;
		for (int i = 0; i < colliders.Length; i++)
		{
			if (Vector2.Distance(colliders[i].transform.position, slots[i].TargetPosition) > 0.25f)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			this.MyNormTask.NextStep();
			base.StartCoroutine(base.CoStartClose(0.75f));
		}
	}
}
