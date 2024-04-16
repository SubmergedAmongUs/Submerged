using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class EmergencyMinigame : Minigame
{
	public SpriteRenderer ClosedLid;

	public SpriteRenderer OpenLid;

	public Transform meetingButton;

	public TextMeshPro StatusText;

	public TextMeshPro NumberText;

	public bool ButtonActive = true;

	public AudioClip ButtonSound;

	private int state;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public const int MinEmergencyTime = 15;

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		this.Update();
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton, this.DefaultButtonSelected);
	}

	public void Update()
	{
		if (ShipStatus.Instance.Timer < 15f || ShipStatus.Instance.EmergencyCooldown > 0f)
		{
			int num = Mathf.CeilToInt(15f - ShipStatus.Instance.Timer);
			num = Mathf.Max(Mathf.CeilToInt(ShipStatus.Instance.EmergencyCooldown), num);
			this.ButtonActive = false;
			this.StatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.EmergencyNotReady, Array.Empty<object>());
			this.NumberText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SecondsAbbv, new object[]
			{
				num
			});
			this.ClosedLid.gameObject.SetActive(true);
			this.OpenLid.gameObject.SetActive(false);
			return;
		}
		if (!PlayerControl.LocalPlayer.myTasks.Any(new Func<PlayerTask, bool>(PlayerTask.TaskIsEmergency)))
		{
			if (this.state == 1)
			{
				return;
			}
			this.state = 1;
			int remainingEmergencies = PlayerControl.LocalPlayer.RemainingEmergencies;
			this.StatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.EmergencyCount, new object[]
			{
				PlayerControl.LocalPlayer.Data.PlayerName
			});
			this.NumberText.text = remainingEmergencies.ToString();
			this.ButtonActive = (remainingEmergencies > 0);
			this.ClosedLid.gameObject.SetActive(!this.ButtonActive);
			this.OpenLid.gameObject.SetActive(this.ButtonActive);
			return;
		}
		else
		{
			if (this.state == 2)
			{
				return;
			}
			this.state = 2;
			this.ButtonActive = false;
			this.StatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.EmergencyDuringCrisis, Array.Empty<object>());
			this.NumberText.text = string.Empty;
			this.ClosedLid.gameObject.SetActive(true);
			this.OpenLid.gameObject.SetActive(false);
			return;
		}
	}

	public void CallMeeting()
	{
		if (!PlayerControl.LocalPlayer.myTasks.Any(new Func<PlayerTask, bool>(PlayerTask.TaskIsEmergency)) && PlayerControl.LocalPlayer.RemainingEmergencies > 0 && this.ButtonActive)
		{
			this.StatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.EmergencyRequested, Array.Empty<object>());
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.ButtonSound, false, 1f);
			}
			PlayerControl.LocalPlayer.CmdReportDeadBody(null);
			this.ButtonActive = false;
			VibrationManager.Vibrate(1f, 1f, 0.2f, VibrationManager.VibrationFalloff.None, null, false);
		}
	}

	private float easeOutElastic(float t)
	{
		float num = 0.3f;
		return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - num / 4f) * 6.2831855f / num) + 1f;
	}

	protected override IEnumerator CoAnimateOpen()
	{
		for (float timer = 0f; timer < 0.2f; timer += Time.deltaTime)
		{
			float num = timer / 0.2f;
			base.transform.localPosition = new Vector3(0f, Mathf.SmoothStep(-8f, 0f, num), -50f);
			yield return null;
		}
		base.transform.localPosition = new Vector3(0f, 0f, -50f);
		Vector3 meetingPos = this.meetingButton.localPosition;
		for (float timer = 0f; timer < 0.1f; timer += Time.deltaTime)
		{
			float num2 = timer / 0.1f;
			meetingPos.y = Mathf.Sin(3.1415927f * num2) * 1f / (num2 * 5f + 4f) - 0.882f;
			this.meetingButton.localPosition = meetingPos;
			yield return null;
		}
		meetingPos.y = -0.882f;
		this.meetingButton.localPosition = meetingPos;
		yield break;
	}
}
