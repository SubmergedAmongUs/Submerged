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
	// public DragSlot[] GunsSlots;
	public AudioClip pickupRifle;
	public AudioClip placeRifle;
	public Collider2D[] RifleColliders;
	// public DragSlot[] RifleSlots;
}
