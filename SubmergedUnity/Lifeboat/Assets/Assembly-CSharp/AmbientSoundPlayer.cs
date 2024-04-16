using System;
using UnityEngine;

public class AmbientSoundPlayer : MonoBehaviour
{
	public AudioClip AmbientSound;

	public Collider2D[] HitAreas;

	public float MaxVolume = 1f;

	public float DistanceFallOff = -1f;

	public float FallOffRate = 1f;

	public void Start()
	{
		SoundManager.Instance.PlayDynamicSound(base.name + base.GetInstanceID().ToString(), this.AmbientSound, true, new DynamicSound.GetDynamicsFunction(this.Dynamics), false);
	}

	private void Dynamics(AudioSource source, float dt)
	{
		if (!PlayerControl.LocalPlayer)
		{
			source.volume = 0f;
			return;
		}
		Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
		bool flag = false;
		for (int i = 0; i < this.HitAreas.Length; i++)
		{
			if (this.HitAreas[i].OverlapPoint(truePosition))
			{
				flag = true;
				break;
			}
		}
		float num = 0f;
		if (flag)
		{
			num = 1f;
		}
		else if (this.DistanceFallOff >= 0f)
		{
			float num2 = Vector2.Distance(truePosition, base.transform.position);
			num = 1f - Mathf.Clamp(num2 / this.DistanceFallOff, 0f, 1f);
		}
		source.volume = Mathf.Lerp(source.volume, num * this.MaxVolume, dt * this.FallOffRate);
	}

	public void OnDestroy()
	{
		SoundManager.Instance.StopNamedSound(base.name + base.GetInstanceID().ToString());
	}
}
