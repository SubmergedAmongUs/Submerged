using System;
using UnityEngine;

[CreateAssetMenu]
public class SoundGroup : ScriptableObject
{
	public AudioClip[] Clips;

	public AudioClip Random()
	{
		return this.Clips.Random<AudioClip>();
	}
}
