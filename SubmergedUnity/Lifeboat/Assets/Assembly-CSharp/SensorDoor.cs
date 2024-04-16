using System;
using UnityEngine;

public class SensorDoor : MonoBehaviour
{
	public SpriteRenderer LeftSide;

	public SpriteRenderer RightSide;

	public float ActivationDistance = 2f;

	public bool Opening;

	public float OpenDuration;

	private float openTimer;

	public AudioClip OpenSound;

	public AudioClip CloseSound;

	private const float slideVibrationIntensity = 3f;

	public void OnEnable()
	{
		base.InvokeRepeating("CheckDoor", 0.1f, 0.1f);
		this.LeftSide.SetCooldownNormalizedUvs();
		this.RightSide.SetCooldownNormalizedUvs();
	}

	[ContextMenu("Set Right Uvs")]
	public void SetUvs()
	{
		this.RightSide.SetCooldownNormalizedUvs();
	}

	private void Update()
	{
		if (this.Opening && this.openTimer < this.OpenDuration)
		{
			this.openTimer += Time.deltaTime;
			float num = Mathf.SmoothStep(0f, 1f, this.openTimer / this.OpenDuration);
			this.LeftSide.material.SetFloat("_Percent", num);
			this.RightSide.material.SetFloat("_Percent", num);
			return;
		}
		if (!this.Opening && this.openTimer > 0f)
		{
			this.openTimer -= Time.deltaTime;
			float num2 = Mathf.SmoothStep(0f, 1f, this.openTimer / this.OpenDuration);
			this.LeftSide.material.SetFloat("_Percent", num2);
			this.RightSide.material.SetFloat("_Percent", num2);
		}
	}

	private void CheckDoor()
	{
		bool opening = this.Opening;
		this.Opening = PhysicsHelpers.CircleContains(base.transform.position, this.ActivationDistance, Constants.PlayersOnlyMask);
		if (opening && !this.Opening)
		{
			SoundManager.Instance.StopSound(this.OpenSound);
			if (Vector2.Distance(base.transform.position, PlayerControl.LocalPlayer.GetTruePosition()) < 3f)
			{
				SoundManager.Instance.PlaySound(this.CloseSound, false, 1f);
			}
			VibrationManager.Vibrate(3f, base.transform.position, 3f, this.OpenDuration, VibrationManager.VibrationFalloff.None, this.CloseSound, false);
			return;
		}
		if (!opening && this.Opening)
		{
			SoundManager.Instance.StopSound(this.CloseSound);
			if (Vector2.Distance(base.transform.position, PlayerControl.LocalPlayer.GetTruePosition()) < 3f)
			{
				SoundManager.Instance.PlaySound(this.OpenSound, false, 1f);
			}
			VibrationManager.Vibrate(3f, base.transform.position, 3f, this.OpenDuration, VibrationManager.VibrationFalloff.None, this.OpenSound, false);
		}
	}
}
