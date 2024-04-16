using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

public class SoundManager : MonoBehaviour
{
	private static SoundManager _Instance;

	public AudioMixerGroup musicMixer;

	public AudioMixerGroup sfxMixer;

	public static float MusicVolume = 1f;

	public static float SfxVolume = 1f;

	private Dictionary<AudioClip, AudioSource> allSources = new Dictionary<AudioClip, AudioSource>();

	private List<ISoundPlayer> soundPlayers = new List<ISoundPlayer>();

	public static SoundManager Instance
	{
		get
		{
			if (!SoundManager._Instance)
			{
				SoundManager._Instance = (Object.FindObjectOfType<SoundManager>() ?? new GameObject("SoundManager").AddComponent<SoundManager>());
			}
			return SoundManager._Instance;
		}
	}

	public void Start()
	{
		if (SoundManager._Instance && SoundManager._Instance != this)
		{
			 UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		SoundManager._Instance = this;
		this.UpdateVolume();
		Object.DontDestroyOnLoad(base.gameObject);
	}

	public void Update()
	{
		for (int i = 0; i < this.soundPlayers.Count; i++)
		{
			this.soundPlayers[i].Update(Time.deltaTime);
		}
	}

	private void UpdateVolume()
	{
		this.ChangeSfxVolume(SaveManager.SfxVolume);
		this.ChangeMusicVolume(SaveManager.MusicVolume);
	}

	public void ChangeSfxVolume(float volume)
	{
		if (volume <= 0f)
		{
			SoundManager.SfxVolume = -80f;
		}
		else
		{
			SoundManager.SfxVolume = Mathf.Log10(volume) * 20f;
		}
		this.musicMixer.audioMixer.SetFloat("SfxVolume", SoundManager.SfxVolume);
	}

	public void ChangeMusicVolume(float volume)
	{
		if (volume <= 0f)
		{
			SoundManager.MusicVolume = -80f;
		}
		else
		{
			SoundManager.MusicVolume = Mathf.Log10(volume) * 20f;
		}
		this.musicMixer.audioMixer.SetFloat("MusicVolume", SoundManager.MusicVolume);
	}

	public void StopNamedSound(string name)
	{
		for (int i = 0; i < this.soundPlayers.Count; i++)
		{
			ISoundPlayer soundPlayer = this.soundPlayers[i];
			if (soundPlayer.Name.Equals(name))
			{
				 UnityEngine.Object.Destroy(soundPlayer.Player);
				this.soundPlayers.RemoveAt(i);
				return;
			}
		}
	}

	public void StopSound(AudioClip clip)
	{
		AudioSource audioSource;
		if (this.allSources.TryGetValue(clip, out audioSource))
		{
			this.allSources.Remove(clip);
			audioSource.Stop();
			 UnityEngine.Object.Destroy(audioSource);
		}
		for (int i = 0; i < this.soundPlayers.Count; i++)
		{
			ISoundPlayer soundPlayer = this.soundPlayers[i];
			if (soundPlayer.Player.clip == clip)
			{
				 UnityEngine.Object.Destroy(soundPlayer.Player);
				this.soundPlayers.RemoveAt(i);
				return;
			}
		}
	}

	public void StopAllSound()
	{
		for (int i = 0; i < this.soundPlayers.Count; i++)
		{
			 UnityEngine.Object.Destroy(this.soundPlayers[i].Player);
		}
		this.soundPlayers.Clear();
		foreach (KeyValuePair<AudioClip, AudioSource> keyValuePair in this.allSources)
		{
			AudioSource value = keyValuePair.Value;
			value.volume = 0f;
			value.Stop();
			 UnityEngine.Object.Destroy(keyValuePair.Value);
		}
		this.allSources.Clear();
	}

	public AudioSource PlayNamedSound(string name, AudioClip sound, bool loop, bool playAsSfx = false)
	{
		return this.PlayDynamicSound(name, sound, loop, delegate(AudioSource a, float b)
		{
		}, playAsSfx);
	}

	public AudioSource GetNamedAudioSource(string name)
	{
		return this.PlayDynamicSound(name, null, false, delegate(AudioSource a, float b)
		{
		}, true);
	}

	public AudioSource PlayDynamicSound(string name, AudioClip clip, bool loop, DynamicSound.GetDynamicsFunction volumeFunc, bool playAsSfx = false)
	{
		DynamicSound dynamicSound = null;
		for (int i = 0; i < this.soundPlayers.Count; i++)
		{
			ISoundPlayer soundPlayer = this.soundPlayers[i];
			if (soundPlayer.Name == name && soundPlayer is DynamicSound)
			{
				dynamicSound = (DynamicSound)soundPlayer;
				break;
			}
		}
		if (dynamicSound == null)
		{
			dynamicSound = new DynamicSound();
			dynamicSound.Name = name;
			dynamicSound.Player = base.gameObject.AddComponent<AudioSource>();
			dynamicSound.Player.outputAudioMixerGroup = ((loop && !playAsSfx) ? this.musicMixer : this.sfxMixer);
			dynamicSound.Player.playOnAwake = false;
			this.soundPlayers.Add(dynamicSound);
		}
		dynamicSound.Player.loop = loop;
		dynamicSound.SetTarget(clip, volumeFunc);
		return dynamicSound.Player;
	}

	public void CrossFadeSound(string name, AudioClip clip, float maxVolume, float duration = 1.5f)
	{
		CrossFader crossFader = null;
		for (int i = 0; i < this.soundPlayers.Count; i++)
		{
			ISoundPlayer soundPlayer = this.soundPlayers[i];
			if (soundPlayer.Name == name && soundPlayer is CrossFader)
			{
				crossFader = (CrossFader)soundPlayer;
				break;
			}
		}
		if (crossFader == null)
		{
			crossFader = new CrossFader();
			crossFader.Name = name;
			crossFader.MaxVolume = maxVolume;
			crossFader.Player = base.gameObject.AddComponent<AudioSource>();
			crossFader.Player.outputAudioMixerGroup = this.musicMixer;
			crossFader.Player.playOnAwake = false;
			crossFader.Player.loop = true;
			this.soundPlayers.Add(crossFader);
		}
		crossFader.SetTarget(clip);
	}

	public AudioSource PlaySoundImmediate(AudioClip clip, bool loop, float volume = 1f, float pitch = 1f)
	{
		if (clip == null)
		{
			Debug.LogWarning("Missing audio clip");
			return null;
		}
		AudioSource audioSource;
		if (this.allSources.TryGetValue(clip, out audioSource))
		{
			audioSource.pitch = pitch;
			audioSource.loop = loop;
			audioSource.Play();
		}
		else
		{
			audioSource = base.gameObject.AddComponent<AudioSource>();
			audioSource.outputAudioMixerGroup = (loop ? this.musicMixer : this.sfxMixer);
			audioSource.playOnAwake = false;
			audioSource.volume = volume;
			audioSource.pitch = pitch;
			audioSource.loop = loop;
			audioSource.clip = clip;
			audioSource.Play();
			this.allSources.Add(clip, audioSource);
		}
		return audioSource;
	}

	public bool SoundIsPlaying(AudioClip clip)
	{
		AudioSource audioSource;
		return this.allSources.TryGetValue(clip, out audioSource) && !audioSource.isPlaying;
	}

	public AudioSource PlaySound(AudioClip clip, bool loop, float volume = 1f)
	{
		if (clip == null)
		{
			Debug.LogWarning("Missing audio clip");
			return null;
		}
		AudioSource audioSource;
		if (this.allSources.TryGetValue(clip, out audioSource))
		{
			if (!audioSource.isPlaying)
			{
				audioSource.volume = volume;
				audioSource.loop = loop;
				audioSource.Play();
			}
		}
		else
		{
			audioSource = base.gameObject.AddComponent<AudioSource>();
			audioSource.outputAudioMixerGroup = (loop ? this.musicMixer : this.sfxMixer);
			audioSource.playOnAwake = false;
			audioSource.volume = volume;
			audioSource.loop = loop;
			audioSource.clip = clip;
			audioSource.Play();
			this.allSources.Add(clip, audioSource);
		}
		return audioSource;
	}
}
