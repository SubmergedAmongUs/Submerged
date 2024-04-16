using System;
using UnityEngine;

public class SimpleSoundPlayer : MonoBehaviour
{
	public AudioClip[] clips;

	private AudioSource soundSource;

	private void OnEnable()
	{
		this.soundSource = base.GetComponent<AudioSource>();
	}

	public void PlaySound()
	{
		this.soundSource.Play();
	}

	public void PlaySpecificSound(int index)
	{
		this.soundSource.clip = this.clips[index];
		this.soundSource.Play();
	}
}
