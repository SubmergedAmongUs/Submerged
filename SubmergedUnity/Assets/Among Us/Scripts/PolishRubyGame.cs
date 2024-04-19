using UnityEngine;

public class PolishRubyGame : Minigame
{
	public PassiveButton[] Buttons;
	public SpriteRenderer[] Sparkles;
	public int[] swipes;
	public Vector2[] directions;
	public int swipesToClean = 6;
	public AudioClip[] rubSounds;
	public AudioClip sparkleSound;
	public Transform cursorObject;
	public Transform handWipeObject;
	public SpriteRenderer[] handSprites;
}
