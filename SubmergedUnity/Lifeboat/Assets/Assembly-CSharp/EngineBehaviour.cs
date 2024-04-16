using System;
using UnityEngine;

public class EngineBehaviour : MonoBehaviour
{
	public AudioClip ElectricSound;

	public AudioClip SteamSound;

	public float SoundDistance = 5f;

	public void PlayElectricSound()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlayDynamicSound("EngineShock" + base.name, this.ElectricSound, false, new DynamicSound.GetDynamicsFunction(this.GetSoundDistance), false);
		}
	}

	public void PlaySteamSound()
	{
		if (Constants.ShouldPlaySfx())
		{
			float pitch = FloatRange.Next(0.7f, 1.1f);
			SoundManager.Instance.PlayDynamicSound("EngineSteam" + base.name, this.SteamSound, false, delegate(AudioSource p, float d)
			{
				this.GetSoundDistance(p, d, pitch);
			}, false);
			VibrationManager.Vibrate(1f, base.transform.position, this.SoundDistance, 0f, VibrationManager.VibrationFalloff.None, this.SteamSound, false);
		}
	}

	private void GetSoundDistance(AudioSource player, float dt)
	{
		this.GetSoundDistance(player, dt, 1f);
	}

	private void GetSoundDistance(AudioSource player, float dt, float pitch)
	{
		if (!PlayerControl.LocalPlayer)
		{
			player.volume = 0f;
			return;
		}
		float num = Vector2.Distance(base.transform.position, PlayerControl.LocalPlayer.GetTruePosition());
		float num2 = 1f - num / this.SoundDistance;
		player.volume = num2 * 0.8f;
		player.pitch = pitch;
	}
}
