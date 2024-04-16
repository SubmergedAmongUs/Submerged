using UnityEngine;
using UnityEngine.UI;

public class DeconControl : MonoBehaviour
{
	public DeconSystem System;
	public float usableDistance = 1f;
	public SpriteRenderer Image;
	public AudioClip UseSound;
	public Button.ButtonClickedEvent OnUse;
}
