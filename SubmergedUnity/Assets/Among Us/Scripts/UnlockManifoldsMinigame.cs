using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class UnlockManifoldsMinigame : Minigame
{
	[UsedImplicitly]
	public void HitButton(int idx)
	{
	}

	public SpriteRenderer[] Buttons;
	public byte SystemId;
	public AudioClip PressButtonSound;
	public AudioClip FailSound;
	[Header("Console Controller Navigation")]
	public UiElement BackButton;
	public UiElement DefaultButtonSelected;
	public List<UiElement> ControllerSelectable;
}
