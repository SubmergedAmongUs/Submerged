using System;
using UnityEngine;

public class SkeldShipRoom : PlainShipRoom, IStepWatcher
{
	public AudioClip AmbientSound;

	public float AmbientVolume = 0.7f;

	public float AmbientMaxDist = 8f;

	public Vector2 AmbientOffset;

	public SoundGroup FootStepSounds;

	private RaycastHit2D[] volumeBuffer = new RaycastHit2D[5];

	public int Priority { get; }

	public void Start()
	{
		if (Constants.ShouldPlaySfx() && this.AmbientSound)
		{
			SoundManager.Instance.PlayDynamicSound("Amb " + this.RoomId.ToString(), this.AmbientSound, true, delegate(AudioSource player, float dt)
			{
				this.GetAmbientSoundVolume(player, dt);
			}, false);
		}
	}

	public SoundGroup MakeFootstep(PlayerControl player)
	{
		if (!DestroyableSingleton<HudManager>.InstanceExists)
		{
			return null;
		}
		RoomTracker roomTracker = DestroyableSingleton<HudManager>.Instance.roomTracker;
		if (roomTracker && roomTracker.LastRoom == this)
		{
			return this.FootStepSounds;
		}
		return null;
	}

	private void GetAmbientSoundVolume(AudioSource player, float dt)
	{
		if (!PlayerControl.LocalPlayer)
		{
			player.volume = 0f;
			return;
		}
		Vector2 vector = (Vector2) base.transform.position + this.AmbientOffset;
		Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
		float num = Vector2.Distance(vector, truePosition);
		if (num > this.AmbientMaxDist)
		{
			player.volume = 0f;
			return;
		}
		Vector2 vector2 = truePosition - vector;
		int num2 = Physics2D.RaycastNonAlloc(vector, vector2, this.volumeBuffer, num, Constants.ShipOnlyMask);
		float num3 = 1f - num / this.AmbientMaxDist - (float)num2 * 0.25f;
		player.volume = Mathf.Lerp(player.volume, num3 * this.AmbientVolume, dt);
	}
}
