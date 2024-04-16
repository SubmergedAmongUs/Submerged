using UnityEngine;

public class WireMinigame : Minigame
{
	public Sprite[] Symbols;
	public Wire[] LeftNodes;
	public WireNode[] RightNodes;
	public SpriteRenderer[] LeftLights;
	public SpriteRenderer[] RightLights;
	public AudioClip[] WireSounds;
	public Vector2 controllerWirePos = Vector2.zero;
	public GameObject[] selectingWireGlyphs;
	public GameObject[] movingWireGlyphs;
	public Transform selectedWireUI;
}
