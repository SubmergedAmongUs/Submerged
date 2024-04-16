using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class VendingMinigame : Minigame
{
	public static readonly string[] Letters = new string[]
	{
		"a",
		"b",
		"c"
	};

	public TextMeshPro NumberText;

	public SpriteRenderer TargetImage;

	public string enteredCode = string.Empty;

	private bool animating;

	private bool done;

	private string targetCode;

	public SpriteRenderer AcceptButton;

	public VendingSlot[] Slots;

	public Sprite[] Drinks;

	public Sprite[] DrawnDrinks;

	public AudioClip Ambience;

	public AudioClip Button;

	public AudioClip Error;

	public AudioClip SliderOpen;

	public AudioClip DrinkShake;

	public AudioClip DrinkLand;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		int num = this.Drinks.RandomIdx<Sprite>();
		this.TargetImage.sprite = this.DrawnDrinks[num];
		for (int i = 0; i < this.Drinks.Length; i++)
		{
			Sprite sprite = this.Drinks[i];
			int num2;
			while (!this.PickARandomSlot(sprite, out num2))
			{
			}
			this.Slots[num2].DrinkImage.enabled = true;
			this.Slots[num2].DrinkImage.sprite = sprite;
			if (num == i)
			{
				this.targetCode = VendingMinigame.SlotIdToString(num2);
			}
		}
		this.NumberText.text = string.Empty;
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton, this.DefaultButtonSelected, this.ControllerSelectable, false);
	}

	private static int StringToSlotId(string code)
	{
		int num;
		if (int.TryParse(code[0].ToString(), out num) || VendingMinigame.Letters.Any(new Func<string, bool>(code.EndsWith)))
		{
			return -1;
		}
		int num2 = VendingMinigame.Letters.IndexOf(new Predicate<string>(code.StartsWith));
		return int.Parse(code[1].ToString()) - 1 + num2 * 4;
	}

	private static string SlotIdToString(int slotId)
	{
		int num = slotId % 4 + 1;
		int num2 = slotId / 4;
		return VendingMinigame.Letters[num2] + num.ToString();
	}

	private bool PickARandomSlot(Sprite drink, out int slotId)
	{
		slotId = this.Slots.RandomIdx<VendingSlot>();
		return !this.Slots[slotId].DrinkImage.enabled;
	}

	public void EnterDigit(string s)
	{
		if (this.animating)
		{
			return;
		}
		if (this.done)
		{
			return;
		}
		if (this.enteredCode.Length >= 2)
		{
			base.StartCoroutine(this.BlinkAccept());
			return;
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.Button, false, 1f).pitch = FloatRange.Next(0.8f, 1.2f);
		}
		this.enteredCode += s;
		this.NumberText.text = this.enteredCode;
	}

	public void ClearDigits()
	{
		if (this.animating)
		{
			return;
		}
		this.enteredCode = string.Empty;
		this.NumberText.text = string.Empty;
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.Button, false, 1f).pitch = FloatRange.Next(0.8f, 1.2f);
		}
	}

	public void AcceptDigits()
	{
		if (this.animating || this.enteredCode.Length != 2)
		{
			return;
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.Button, false, 1f).pitch = FloatRange.Next(0.8f, 1.2f);
		}
		base.StartCoroutine(this.Animate());
	}

	private IEnumerator BlinkAccept()
	{
		int num;
		for (int i = 0; i < 5; i = num)
		{
			this.AcceptButton.color = Color.gray;
			yield return null;
			yield return null;
			this.AcceptButton.color = Color.white;
			yield return null;
			yield return null;
			num = i + 1;
		}
		yield break;
	}

	private IEnumerator Animate()
	{
		this.animating = true;
		int slotId = VendingMinigame.StringToSlotId(this.enteredCode);
		if (slotId >= 0 && this.Slots[slotId].DrinkImage.enabled)
		{
			yield return Effects.All(new IEnumerator[]
			{
				this.CoBlinkVend(),
				this.Slots[slotId].CoBuy(this.SliderOpen, this.DrinkShake, this.DrinkLand)
			});
			if (this.targetCode == this.enteredCode)
			{
				this.done = true;
				this.MyNormTask.NextStep();
				base.StartCoroutine(base.CoStartClose(0.75f));
			}
			yield return this.Slots[slotId].CloseSlider(this.SliderOpen);
		}
		else
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.Error, false, 1f);
			}
			WaitForSeconds wait = new WaitForSeconds(0.1f);
			this.NumberText.text = "XXXXXXXX";
			yield return wait;
			this.NumberText.text = string.Empty;
			yield return wait;
			this.NumberText.text = "XXXXXXXX";
			yield return wait;
			wait = null;
		}
		this.enteredCode = string.Empty;
		this.NumberText.text = this.enteredCode;
		this.animating = false;
		yield break;
	}

	private IEnumerator CoBlinkVend()
	{
		int num;
		for (int i = 0; i < 5; i = num)
		{
			this.NumberText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Vending, Array.Empty<object>());
			yield return Effects.Wait(0.1f);
			this.NumberText.text = string.Empty;
			yield return Effects.Wait(0.1f);
			num = i + 1;
		}
		yield break;
	}
}
