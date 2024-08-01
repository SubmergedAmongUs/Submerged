using UnityEngine;

public class AmbientSoundPlayer : MonoBehaviour
{
	public AudioClip AmbientSound;
	public Collider2D[] HitAreas;
	public float MaxVolume = 1f;
	public float DistanceFallOff = -1f;
	public float FallOffRate = 1f;
}
