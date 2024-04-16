using System;
using System.Collections.Generic;
using UnityEngine;

public class RaycastAmbientSoundPlayer : MonoBehaviour
{
	public AudioClip AmbientSound;

	public float AmbientVolume = 1f;

	public float AmbientMaxDist = 8f;

	public float HitModifier = 0.25f;

	public static List<RaycastAmbientSoundPlayer> players = new List<RaycastAmbientSoundPlayer>();

	public float ambientVolume;

	public float t;

	private RaycastHit2D[] volumeBuffer = new RaycastHit2D[5];

	private void OnEnable()
	{
		RaycastAmbientSoundPlayer.players.Add(this);
	}

	private void OnDisable()
	{
		RaycastAmbientSoundPlayer.players.Remove(this);
	}

	public void Start()
	{
		if (Constants.ShouldPlaySfx() && this.AmbientSound)
		{
			SoundManager.Instance.PlayDynamicSound("Amb " + base.name, this.AmbientSound, true, delegate(AudioSource player, float dt)
			{
				this.GetAmbientSoundVolume(player, dt);
			}, false);
		}
	}

	private void GetAmbientSoundVolume(AudioSource player, float dt)
	{
		this.ambientVolume = 0f;
		if (!PlayerControl.LocalPlayer)
		{
			player.volume = 0f;
			return;
		}
		Vector2 vector = base.transform.position;
		Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
		float num = Vector2.Distance(vector, truePosition);
		if (num > this.AmbientMaxDist)
		{
			player.volume = 0f;
			return;
		}
		Vector2 vector2 = truePosition - vector;
		int num2 = Physics2D.RaycastNonAlloc(vector, vector2, this.volumeBuffer, num, Constants.ShipOnlyMask);
		float num3 = 1f - num / this.AmbientMaxDist - (float)num2 * this.HitModifier;
		player.volume = Mathf.Lerp(player.volume, num3 * this.AmbientVolume, dt);
		this.ambientVolume = player.volume;
	}
}
