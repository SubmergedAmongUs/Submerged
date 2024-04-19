using TMPro;
using UnityEngine;

public class ExileController : MonoBehaviour
{
	public TextMeshPro ImpostorText;
	public TextMeshPro Text;
	public PoolablePlayer Player;
    public AnimationCurve LerpCurve;
	public float Duration = 7f;
	public AudioClip TextSound;
}
