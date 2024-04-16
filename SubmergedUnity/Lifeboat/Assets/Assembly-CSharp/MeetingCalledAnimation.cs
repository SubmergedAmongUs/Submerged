using System;
using System.Collections;
using UnityEngine;

public class MeetingCalledAnimation : OverlayAnimation
{
	public AspectPosition emergencyText;

	public PoolablePlayer playerParts;

	public AudioClip Stinger;

	public float StingerVolume = 0.6f;

	public AnimationCurve CrewmateSlide;

	public void Initialize(GameData.PlayerInfo reportInfo)
	{
		PlayerControl.SetPlayerMaterialColors(reportInfo.ColorId, this.playerParts.Body);
		this.playerParts.Hands.ForEach(delegate(SpriteRenderer b)
		{
			PlayerControl.SetPlayerMaterialColors(reportInfo.ColorId, b);
		});
		if (this.playerParts.HatSlot)
		{
			this.playerParts.HatSlot.SetHat(reportInfo.HatId, reportInfo.ColorId);
		}
	}

	public override IEnumerator CoShow(KillOverlay parent)
	{
		base.gameObject.SetActive(true);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.Stinger, false, 1f).volume = this.StingerVolume;
		}
		AspectPosition playerTransform = this.playerParts.GetComponent<AspectPosition>();
		Vector3 distanceFromEdge = playerTransform.DistanceFromEdge;
		yield return Effects.All(new IEnumerator[]
		{
			Effects.Lerp(2.5f, delegate(float t)
			{
				this.emergencyText.SetNormalizedX(this.CrewmateSlide.Evaluate(t), 0.5f);
			}),
			Effects.Lerp(2.5f, delegate(float t)
			{
				playerTransform.SetNormalizedX(this.CrewmateSlide.Evaluate(t), 0.5f);
			})
		});
		base.gameObject.SetActive(false);
		yield break;
	}
}
