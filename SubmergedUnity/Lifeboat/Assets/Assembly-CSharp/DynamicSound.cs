using System;
using UnityEngine;

public class DynamicSound : ISoundPlayer
{
	public DynamicSound.GetDynamicsFunction volumeFunc;

	public string Name { get; set; }

	public AudioSource Player { get; set; }

	public void Update(float dt)
	{
		if (!this.Player.isPlaying)
		{
			return;
		}
		this.volumeFunc(this.Player, dt);
	}

	public void SetTarget(AudioClip clip, DynamicSound.GetDynamicsFunction volumeFunc)
	{
		this.volumeFunc = volumeFunc;
		this.Player.clip = clip;
		this.volumeFunc(this.Player, 1f);
		this.Player.Play();
	}

	public delegate void GetDynamicsFunction(AudioSource source, float dt);
}
