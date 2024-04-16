using System;
using System.Linq;
using UnityEngine;

public class DoorBreakerGame : Minigame, IDoorMinigame
{
	public PlainDoor MyDoor;

	public SpriteRenderer[] Buttons;

	public AudioClip FlipSound;

	public void SetDoor(PlainDoor door)
	{
		this.MyDoor = door;
	}

	public void Start()
	{
		for (int i = 0; i < this.Buttons.Length; i++)
		{
			SpriteRenderer spriteRenderer = this.Buttons[i];
			spriteRenderer.color = Color.gray;
			spriteRenderer.GetComponent<PassiveButton>().enabled = false;
		}
		int j = 0;
		while (j < 4)
		{
			SpriteRenderer spriteRenderer2 = this.Buttons.Random<SpriteRenderer>();
			if (!spriteRenderer2.flipX)
			{
				spriteRenderer2.color = Color.white;
				spriteRenderer2.GetComponent<PassiveButton>().enabled = true;
				spriteRenderer2.flipX = true;
				j++;
			}
		}
	}

	public void FlipSwitch(SpriteRenderer button)
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.FlipSound, false, 1f);
		}
		button.color = Color.gray;
		button.flipX = false;
		button.GetComponent<PassiveButton>().enabled = false;
		if (this.Buttons.All((SpriteRenderer s) => !s.flipX))
		{
			ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, this.MyDoor.Id | 64);
			this.MyDoor.SetDoorway(true);
			base.StartCoroutine(base.CoStartClose(0.4f));
		}
	}
}
