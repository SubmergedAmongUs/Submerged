using PowerTools;
using UnityEngine;

public class PetBehaviour : MonoBehaviour
{
	public bool Free;
	public bool NotInStore;
	public string ProductId;
	public string StoreName;
	public uint SteamId;
	public string EpicId;
	public int ItchId;
	public string ItchUrl;
	public string Win10Id;
	public float YOffset = -0.25f;
	public SpriteAnim animator;
	public SpriteRenderer rend;
	public SpriteRenderer shadowRend;
	public Rigidbody2D body;
	public Collider2D Collider;
	public AnimationClip idleClip;
	public AnimationClip sadClip;
	public AnimationClip scaredClip;
	public AnimationClip walkClip;
}
