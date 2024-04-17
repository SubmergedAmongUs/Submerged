using UnityEngine;

public class AlphaBlink : MonoBehaviour
{
	public float Period = 1f;
	public float Ratio = 0.5f;
	public FloatRange AlphaRange = new FloatRange(0.2f, 0.5f);
	public Color baseColor = Color.white;
}
