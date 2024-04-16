using System;
using Beebyte.Obfuscator;
using Hazel;
using InnerNet;
using UnityEngine;

[SkipRename]
public class LobbyBehaviour : InnerNetObject
{
	public static LobbyBehaviour Instance;

	public AudioClip SpawnSound;

	public AnimationClip SpawnInClip;

	public Vector2[] SpawnPositions;

	public AudioClip DropShipSound;

	public SkeldShipRoom[] AllRooms;

	private float timer;

	public void Start()
	{
		LobbyBehaviour.Instance = this;
		SoundManager.Instance.StopAllSound();
		AudioSource audioSource = SoundManager.Instance.PlayNamedSound("DropShipAmb", this.DropShipSound, true, false);
		audioSource.loop = true;
		audioSource.pitch = 1.2f;
		Camera main = Camera.main;
		if (main)
		{
			FollowerCamera component = main.GetComponent<FollowerCamera>();
			if (component)
			{
				component.shakeAmount = 0.03f;
				component.shakePeriod = 400f;
			}
		}
		foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
		{
			playerControl.UpdatePlatformIcon();
		}
	}

	public void FixedUpdate()
	{
		this.timer += Time.deltaTime;
		if (this.timer < 0.25f)
		{
			return;
		}
		this.timer = 0f;
		if (PlayerControl.GameOptions != null)
		{
			int numPlayers = GameData.Instance ? GameData.Instance.PlayerCount : 10;
			DestroyableSingleton<HudManager>.Instance.GameSettings.text = PlayerControl.GameOptions.ToHudString(numPlayers);
			DestroyableSingleton<HudManager>.Instance.GameSettings.gameObject.SetActive(true);
		}
	}

	public override void OnDestroy()
	{
		SoundManager.Instance.StopNamedSound("DropShipAmb");
		base.OnDestroy();
	}

	public override void HandleRpc(byte callId, MessageReader reader)
	{
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		return false;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
	}
}
