using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class EmergencyMinigame : Minigame
{
	[UsedImplicitly]
	public void CallMeeting()
	{
	}

	public SpriteRenderer ClosedLid;
	public SpriteRenderer OpenLid;
	public Transform meetingButton;
	public TextMeshPro StatusText;
	public TextMeshPro NumberText;
	public bool ButtonActive = true;
	public AudioClip ButtonSound;
	[Header("Console Controller Navigation")]
	public UiElement BackButton;
	public UiElement DefaultButtonSelected;
}
