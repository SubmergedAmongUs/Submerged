using PowerTools;
using UnityEngine;

public class PlainDoor : SomeKindaDoor
{
	public SystemTypes Room;
	public int Id;
	public bool Open;
	public BoxCollider2D myCollider;
	public Collider2D shadowCollider;
	public SpriteAnim animator;
	public AnimationClip OpenDoorAnim;
	public AnimationClip CloseDoorAnim;
	public AudioClip OpenSound;
	public AudioClip CloseSound;
}
