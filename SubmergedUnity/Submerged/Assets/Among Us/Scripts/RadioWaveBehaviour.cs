using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RadioWaveBehaviour : MonoBehaviour
{
	public int NumPoints = 128;
	public FloatRange Width;
	public FloatRange Height;
	public float TickRate = 0.1f;
	public int Skip = 2;
	public float Frequency = 5f;
	public bool Random;
	[Range(0f, 1f)]
	public float NoiseLevel;
}
