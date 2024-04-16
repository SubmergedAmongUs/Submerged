using System;
using UnityEngine;

public class TriggeredSound : MonoBehaviour
{
	public AudioClip[] SoundToPlay;

	public FloatRange PitchRange = new FloatRange(1f, 1f);

	private AudioSource Player;

	public float MaxVolume = 1f;

	public float MaxDist = 6f;

	public float HitModifier = 0.25f;

	private RaycastHit2D[] volumeBuffer = new RaycastHit2D[5];

	public void Start()
	{
		this.Player = SoundManager.Instance.GetNamedAudioSource(base.name + base.GetInstanceID().ToString());
		this.Player.playOnAwake = false;
		this.Player.loop = false;
	}

	public void PlaySound()
	{
		if (!this.Player)
		{
			return;
		}
		if (this.SoundToPlay == null)
		{
			return;
		}
		if (this.PitchRange == null)
		{
			return;
		}
		this.Player.clip = this.SoundToPlay.Random<AudioClip>();
		this.Player.pitch = this.PitchRange.Next();
		this.GetAmbientSoundVolume(this.Player);
		this.Player.Play();
	}

	private void GetAmbientSoundVolume(AudioSource player)
	{
		if (!PlayerControl.LocalPlayer)
		{
			player.volume = 0f;
			return;
		}
		Vector2 vector = base.transform.position;
		Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
		float num = Vector2.Distance(vector, truePosition);
		if (num > this.MaxDist)
		{
			player.volume = 0f;
			return;
		}
		ContactFilter2D contactFilter2D = default(ContactFilter2D);
		contactFilter2D.useTriggers = false;
		contactFilter2D.layerMask = Constants.ShipOnlyMask;
		contactFilter2D.useLayerMask = true;
		Vector2 vector2 = truePosition - vector;
		int num2 = Physics2D.Raycast(vector, vector2, contactFilter2D, this.volumeBuffer, num);
		float num3 = 1f - num / this.MaxDist - (float)num2 * this.HitModifier;
		player.volume = num3 * this.MaxVolume;
	}
}
