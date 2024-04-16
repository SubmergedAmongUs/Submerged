using System;
using UnityEngine;

public class BurgerTopping : MonoBehaviour
{
	public Collider2D Hitbox;

	public AudioClip GrabSound;

	public AudioClip DropSound;

	public BurgerToppingTypes ToppingType;

	public float Offset = 0.3f;
}
