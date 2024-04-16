using System;
using UnityEngine;

public class DressUpMinigame : Minigame
{
	public SpriteRenderer DummyHat;

	public SpriteRenderer DummyAccessory;

	public SpriteRenderer DummyClothes;

	public SpriteRenderer ActualHat;

	public SpriteRenderer ActualAccessory;

	public SpriteRenderer ActualClothes;

	public DressUpCosmetic[] buttons;

	public Sprite[] Hats;

	public Sprite[] Accessories;

	public Sprite[] Clothes;

	public Collider2D hatHitbox;

	public Collider2D faceHitbox;

	public Collider2D bodyHitbox;

	public SpriteRenderer draggable;

	public AudioClip hatSound;

	public AudioClip faceSound;

	public AudioClip clothesSound;

	public AudioClip correctSound;

	public AudioClip incorrectSound;

	public AudioClip finishedSound;

	private Controller controller = new Controller();

	public SpriteRenderer grabbyHand;

	public Transform cursorObject;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		int num = this.Hats.RandomIdx<Sprite>();
		this.DummyHat.transform.SetLocalZ((float)((num == 1) ? 1 : -1));
		this.DummyHat.sprite = this.Hats[num];
		this.DummyAccessory.sprite = this.Accessories.Random<Sprite>();
		this.DummyClothes.sprite = this.Clothes.Random<Sprite>();
		this.draggable.enabled = false;
		PlayerControl.LocalPlayer.SetPlayerMaterialColors(this.grabbyHand);
		base.SetupInput(false);
	}

	public void Update()
	{
		if (this.amClosing != Minigame.CloseState.None)
		{
			return;
		}
		Vector3 position = this.cursorObject.position;
		position.x = VirtualCursor.currentPosition.x;
		position.y = VirtualCursor.currentPosition.y;
		this.cursorObject.position = position;
		if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
		{
			if (this.grabbyHand.enabled == this.draggable.enabled)
			{
				this.grabbyHand.enabled = !this.draggable.enabled;
			}
		}
		else if (this.grabbyHand.enabled)
		{
			this.grabbyHand.enabled = false;
		}
		this.controller.Update();
		foreach (DressUpCosmetic dressUpCosmetic in this.buttons)
		{
			Vector2 dragPosition = this.controller.DragPosition;
			switch (this.controller.CheckDrag(dressUpCosmetic.Hitbox))
			{
			case DragState.TouchStart:
				this.draggable.enabled = true;
				this.draggable.sprite = dressUpCosmetic.Rend.sprite;
				this.draggable.transform.position = (Vector3) this.controller.DragPosition - this.draggable.sprite.bounds.center;
				switch (dressUpCosmetic.Slot)
				{
				case CosmeticType.Hat:
				{
					int num = this.Hats.IndexOf(dressUpCosmetic.Rend.sprite);
					this.ActualHat.transform.SetLocalZ((float)((num == 1) ? 1 : -1));
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(this.hatSound, false, 1f);
					}
					break;
				}
				case CosmeticType.Accessory:
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(this.faceSound, false, 1f);
					}
					break;
				case CosmeticType.Skin:
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(this.clothesSound, false, 1f);
					}
					break;
				}
				break;
			case DragState.Dragging:
			{
				Vector3 vector = this.controller.DragPosition;
				vector -= this.draggable.sprite.bounds.center;
				vector.z = -5f + base.transform.position.z;
				switch (dressUpCosmetic.Slot)
				{
				case CosmeticType.Hat:
					if (this.hatHitbox.OverlapPoint(this.controller.DragPosition))
					{
						vector = this.ActualHat.transform.position;
					}
					break;
				case CosmeticType.Accessory:
					if (this.faceHitbox.OverlapPoint(this.controller.DragPosition))
					{
						vector = this.ActualAccessory.transform.position;
					}
					break;
				case CosmeticType.Skin:
					if (this.bodyHitbox.OverlapPoint(this.controller.DragPosition))
					{
						vector = this.ActualClothes.transform.position;
					}
					break;
				}
				this.draggable.transform.position = vector;
				break;
			}
			case DragState.Released:
				this.draggable.enabled = false;
				switch (dressUpCosmetic.Slot)
				{
				case CosmeticType.Hat:
					if (this.hatHitbox.OverlapPoint(dragPosition))
					{
						this.SetHat(this.Hats.IndexOf(dressUpCosmetic.Rend.sprite));
					}
					break;
				case CosmeticType.Accessory:
					if (this.faceHitbox.OverlapPoint(dragPosition))
					{
						this.SetAccessory(this.Accessories.IndexOf(dressUpCosmetic.Rend.sprite));
					}
					break;
				case CosmeticType.Skin:
					if (this.bodyHitbox.OverlapPoint(dragPosition))
					{
						this.SetClothes(this.Clothes.IndexOf(dressUpCosmetic.Rend.sprite));
					}
					break;
				}
				break;
			}
		}
	}

	public void SetHat(int i)
	{
		if (this.amClosing != Minigame.CloseState.None || i < 0)
		{
			return;
		}
		this.ActualHat.transform.SetLocalZ((float)((i == 1) ? 1 : -1));
		this.ActualHat.sprite = this.Hats[i];
		if (this.DummyHat.sprite == this.ActualHat.sprite)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.correctSound, false, 1f);
			}
		}
		else if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.incorrectSound, false, 1f);
		}
		this.CheckOutfit();
	}

	public void SetAccessory(int i)
	{
		if (this.amClosing != Minigame.CloseState.None || i < 0)
		{
			return;
		}
		this.ActualAccessory.sprite = this.Accessories[i];
		if (this.DummyAccessory.sprite == this.ActualAccessory.sprite)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.correctSound, false, 1f);
			}
		}
		else if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.incorrectSound, false, 1f);
		}
		this.CheckOutfit();
	}

	public void SetClothes(int i)
	{
		if (this.amClosing != Minigame.CloseState.None || i < 0)
		{
			return;
		}
		this.ActualClothes.sprite = this.Clothes[i];
		if (this.DummyClothes.sprite == this.ActualClothes.sprite)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.correctSound, false, 1f);
			}
		}
		else if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.incorrectSound, false, 1f);
		}
		this.CheckOutfit();
	}

	private void CheckOutfit()
	{
		if (this.DummyHat.sprite == this.ActualHat.sprite && this.DummyClothes.sprite == this.ActualClothes.sprite && this.DummyAccessory.sprite == this.ActualAccessory.sprite)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.finishedSound, false, 1f);
			}
			this.MyNormTask.NextStep();
			base.StartCoroutine(base.CoStartClose(0.7f));
		}
	}
}
