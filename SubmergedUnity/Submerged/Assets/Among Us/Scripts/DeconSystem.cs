using JetBrains.Annotations;
using UnityEngine;

public class DeconSystem : MonoBehaviour
{
	[UsedImplicitly]
	public void OpenDoor(bool upper)
	{
	}

	[UsedImplicitly]
	public void OpenFromInside(bool upper)
	{
	}

	public SomeKindaDoor UpperDoor;
	public SomeKindaDoor LowerDoor;
	public float DoorOpenTime = 3f;
	public float DeconTime = 3f;
	public AudioClip SpraySound;
	public ParticleSystem[] Particles;
	public SystemTypes TargetSystem = SystemTypes.Decontamination;
	public Collider2D RoomArea;
	public DecontamNumController FloorText;
}
