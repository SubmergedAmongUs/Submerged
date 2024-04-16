using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnlockManifoldsMinigame : Minigame
{
	public SpriteRenderer[] Buttons;

	public byte SystemId;

	private int buttonCounter;

	private bool animating;

	public AudioClip PressButtonSound;

	public AudioClip FailSound;

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
		int num = 2;
		int num2 = this.Buttons.Length / num;
		float[] array = FloatRange.SpreadToEdges(-1.7f, 1.7f, num2).ToArray<float>();
		float[] array2 = FloatRange.SpreadToEdges(-0.43f, 0.43f, num).ToArray<float>();
		SpriteRenderer[] array3 = this.Buttons.ToArray<SpriteRenderer>();
		array3.Shuffle(0);
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				int num3 = i + j * num2;
				array3[num3].transform.localPosition = new Vector3(array[i], array2[j], 0f);
			}
		}
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton, this.DefaultButtonSelected, this.ControllerSelectable, false);
	}

	public void HitButton(int idx)
	{
		if (this.MyNormTask.IsComplete)
		{
			return;
		}
		if (this.animating)
		{
			return;
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.PressButtonSound, false, 1f).pitch = Mathf.Lerp(0.5f, 1.5f, (float)idx / 10f);
		}
		if (idx == this.buttonCounter)
		{
			this.Buttons[idx].color = Color.green;
			this.buttonCounter++;
			if (this.buttonCounter == this.Buttons.Length)
			{
				this.MyNormTask.NextStep();
				base.StartCoroutine(base.CoStartClose(0.75f));
				return;
			}
		}
		else
		{
			this.buttonCounter = 0;
			base.StartCoroutine(this.ResetAll());
		}
	}

	private IEnumerator ResetAll()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.FailSound, false, 1f);
		}
		this.animating = true;
		for (int i = 0; i < this.Buttons.Length; i++)
		{
			this.Buttons[i].color = Color.red;
		}
		yield return new WaitForSeconds(0.25f);
		for (int j = 0; j < this.Buttons.Length; j++)
		{
			this.Buttons[j].color = Color.white;
		}
		yield return new WaitForSeconds(0.25f);
		for (int k = 0; k < this.Buttons.Length; k++)
		{
			this.Buttons[k].color = Color.red;
		}
		yield return new WaitForSeconds(0.25f);
		for (int l = 0; l < this.Buttons.Length; l++)
		{
			this.Buttons[l].color = Color.white;
		}
		this.animating = false;
		yield break;
	}
}
