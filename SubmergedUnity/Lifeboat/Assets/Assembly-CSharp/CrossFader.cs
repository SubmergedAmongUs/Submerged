using System;
using UnityEngine;

public class CrossFader : ISoundPlayer
{
	public float MaxVolume = 1f;

	public AudioClip target;

	public float Duration = 1.5f;

	private float timer;

	private bool didSwitch;

	public string Name { get; set; }

	public AudioSource Player { get; set; }

	public void Update(float dt)
	{
		if (this.timer < this.Duration)
		{
			this.timer += dt;
			float num = this.timer / this.Duration;
			if (num < 0.5f)
			{
				this.Player.volume = (1f - num * 2f) * this.MaxVolume;
				return;
			}
			if (!this.didSwitch)
			{
				this.didSwitch = true;
				this.Player.Stop();
				this.Player.clip = this.target;
				if (this.target)
				{
					this.Player.Play();
				}
			}
			this.Player.volume = (num - 0.5f) * 2f * this.MaxVolume;
		}
	}

	public void SetTarget(AudioClip clip)
	{
		if (!this.Player.clip)
		{
			this.didSwitch = false;
			this.Player.volume = 0f;
			this.timer = 0.5f;
		}
		else
		{
			if (this.Player.clip == clip)
			{
				return;
			}
			if (this.didSwitch)
			{
				this.didSwitch = false;
				if (this.timer >= this.Duration)
				{
					this.timer = 0f;
				}
				else
				{
					this.timer = this.Duration - this.timer;
				}
			}
		}
		this.target = clip;
	}
}
