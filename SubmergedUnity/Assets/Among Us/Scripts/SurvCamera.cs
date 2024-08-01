using PowerTools;
using UnityEngine;

public class SurvCamera : MonoBehaviour
{
	public string CamName;
	public StringNames NewName;
	public SpriteAnim[] Images;
	public float CamSize = 3f;
	public float CamAspect = 1f;
	public Vector3 Offset;
	public AnimationClip OnAnim;
	public AnimationClip OffAnim;
	public StringNames camNameString;
}
