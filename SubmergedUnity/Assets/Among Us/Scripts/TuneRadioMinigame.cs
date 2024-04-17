using UnityEngine;

public class TuneRadioMinigame : Minigame
{
	public RadioWaveBehaviour actualSignal;
	public DialBehaviour dial;
	public SpriteRenderer redLight;
	public SpriteRenderer greenLight;
	public float Tolerance = 0.1f;
	public float targetAngle;
	public bool finished;
	public AudioClip StaticSound;
	public AudioClip RadioSound;
}
