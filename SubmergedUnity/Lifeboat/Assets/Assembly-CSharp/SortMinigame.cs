using System;
using Rewired;
using UnityEngine;

public class SortMinigame : Minigame
{
	public SortGameObject[] Objects;

	public BoxCollider2D AnimalBox;

	public BoxCollider2D PlantBox;

	public BoxCollider2D MineralBox;

	public AudioClip[] PickUpSounds;

	public AudioClip[] DropSounds;

	private Controller myController = new Controller();

	public Transform selectorObject;

	public SpriteRenderer selectorHand;

	private bool prevHadButton;

	private int currentlyGrabbedObject = -1;

	public void Start()
	{
		this.Objects.Shuffle(0);
		for (int i = 0; i < this.Objects.Length; i++)
		{
			SortGameObject sortGameObject = this.Objects[i];
			sortGameObject.transform.localPosition = new Vector3(Mathf.Lerp(-2f, 2f, (float)i / ((float)this.Objects.Length - 1f)), FloatRange.Next(-2.25f, -1.7f), -1f);
			this.CheckBox(sortGameObject, true);
		}
		PlayerControl.LocalPlayer.SetPlayerMaterialColors(this.selectorHand);
		base.SetupInput(false);
	}

	public void Update()
	{
		if (this.amClosing != Minigame.CloseState.None)
		{
			return;
		}
		this.myController.Update();
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			bool button = ReInput.players.GetPlayer(0).GetButton(11);
			Vector3 position = this.selectorObject.transform.position;
			position.x = VirtualCursor.currentPosition.x;
			position.y = VirtualCursor.currentPosition.y;
			this.selectorObject.transform.position = position;
			if (button)
			{
				if (!this.prevHadButton)
				{
					float num = 0f;
					this.currentlyGrabbedObject = -1;
					for (int i = 0; i < this.Objects.Length; i++)
					{
						float sqrMagnitude = (this.Objects[i].transform.localPosition - this.selectorObject.transform.localPosition).sqrMagnitude;
						if (sqrMagnitude <= 2f && (this.currentlyGrabbedObject == -1 || sqrMagnitude < num))
						{
							num = sqrMagnitude;
							this.currentlyGrabbedObject = i;
						}
					}
					if (this.currentlyGrabbedObject != -1)
					{
						SortGameObject sortGameObject = this.Objects[this.currentlyGrabbedObject];
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(this.PickUpSounds.Random<AudioClip>(), false, 1f);
						}
						sortGameObject.StopAllCoroutines();
						sortGameObject.StartCoroutine(sortGameObject.CoShadowRise());
					}
				}
				else if (this.currentlyGrabbedObject != -1)
				{
					SortGameObject sortGameObject2 = this.Objects[this.currentlyGrabbedObject];
					Vector3 position2 = sortGameObject2.transform.position;
					position2.x = position.x;
					position2.y = position.y;
					sortGameObject2.transform.position = position2;
					this.CheckBox(sortGameObject2, false);
				}
				this.prevHadButton = true;
			}
			else
			{
				if (this.prevHadButton && this.currentlyGrabbedObject != -1)
				{
					SortGameObject sortGameObject3 = this.Objects[this.currentlyGrabbedObject];
					bool flag = true;
					for (int j = 0; j < this.Objects.Length; j++)
					{
						SortGameObject obj = this.Objects[j];
						flag &= this.CheckBox(obj, true);
					}
					sortGameObject3.StopAllCoroutines();
					sortGameObject3.StartCoroutine(sortGameObject3.CoShadowFall(this.CheckBox(sortGameObject3, true), this.DropSounds.Random<AudioClip>()));
					if (flag)
					{
						this.MyNormTask.NextStep();
						base.StartCoroutine(base.CoStartClose(0.75f));
					}
				}
				this.prevHadButton = false;
				this.currentlyGrabbedObject = -1;
			}
			if (this.currentlyGrabbedObject != -1 && this.selectorObject.gameObject.activeSelf)
			{
				this.selectorObject.gameObject.SetActive(false);
				return;
			}
			if (this.currentlyGrabbedObject == -1 && !this.selectorObject.gameObject.activeSelf)
			{
				this.selectorObject.gameObject.SetActive(true);
				return;
			}
		}
		else
		{
			if (this.selectorObject.gameObject.activeSelf)
			{
				this.selectorObject.gameObject.SetActive(false);
			}
			if (this.currentlyGrabbedObject != -1)
			{
				SortGameObject sortGameObject4 = this.Objects[this.currentlyGrabbedObject];
				bool flag2 = true;
				for (int k = 0; k < this.Objects.Length; k++)
				{
					SortGameObject obj2 = this.Objects[k];
					flag2 &= this.CheckBox(obj2, true);
				}
				sortGameObject4.StopAllCoroutines();
				sortGameObject4.StartCoroutine(sortGameObject4.CoShadowFall(this.CheckBox(sortGameObject4, true), this.DropSounds.Random<AudioClip>()));
				if (flag2)
				{
					this.MyNormTask.NextStep();
					base.StartCoroutine(base.CoStartClose(0.75f));
				}
				this.currentlyGrabbedObject = -1;
				this.prevHadButton = false;
			}
			for (int l = 0; l < this.Objects.Length; l++)
			{
				SortGameObject sortGameObject5 = this.Objects[l];
				switch (this.myController.CheckDrag(sortGameObject5.Collider))
				{
				case DragState.TouchStart:
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(this.PickUpSounds.Random<AudioClip>(), false, 1f);
					}
					sortGameObject5.StopAllCoroutines();
					sortGameObject5.StartCoroutine(sortGameObject5.CoShadowRise());
					break;
				case DragState.Dragging:
				{
					Vector2 dragPosition = this.myController.DragPosition;
					Vector3 position3 = sortGameObject5.transform.position;
					position3.x = dragPosition.x;
					position3.y = dragPosition.y;
					sortGameObject5.transform.position = position3;
					this.CheckBox(sortGameObject5, false);
					break;
				}
				case DragState.Released:
				{
					bool flag3 = true;
					for (int m = 0; m < this.Objects.Length; m++)
					{
						SortGameObject obj3 = this.Objects[m];
						flag3 &= this.CheckBox(obj3, true);
					}
					sortGameObject5.StopAllCoroutines();
					sortGameObject5.StartCoroutine(sortGameObject5.CoShadowFall(this.CheckBox(sortGameObject5, true), this.DropSounds.Random<AudioClip>()));
					if (flag3)
					{
						this.MyNormTask.NextStep();
						base.StartCoroutine(base.CoStartClose(0.75f));
					}
					break;
				}
				}
			}
		}
	}

	private bool CheckBox(SortGameObject obj, bool dropped)
	{
		BoxCollider2D boxCollider2D = null;
		switch (obj.MyType)
		{
		case SortGameObject.ObjType.Plant:
			boxCollider2D = this.PlantBox;
			break;
		case SortGameObject.ObjType.Mineral:
			boxCollider2D = this.MineralBox;
			break;
		case SortGameObject.ObjType.Animal:
			boxCollider2D = this.AnimalBox;
			break;
		}
		if (obj.Collider.IsTouching(boxCollider2D))
		{
			obj.Shadow.material.SetFloat("_Outline", 1f);
			obj.Shadow.material.SetColor("_OutlineColor", new Color(0f, 0.8f, 1f));
			return true;
		}
		if (dropped)
		{
			obj.Shadow.material.SetFloat("_Outline", 1f);
			obj.Shadow.material.SetColor("_OutlineColor", Color.red);
		}
		else
		{
			obj.Shadow.material.SetFloat("_Outline", 0f);
		}
		return false;
	}
}
