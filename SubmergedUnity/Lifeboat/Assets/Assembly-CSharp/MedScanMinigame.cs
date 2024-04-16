using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

public class MedScanMinigame : Minigame
{
	private const int SomeKindOfPrimeNumber = 7;

	private static readonly string[] BloodTypes = new string[]
	{
		"O-",
		"A-",
		"B-",
		"AB-",
		"O+",
		"A+",
		"B+",
		"AB+"
	};

	public TextMeshPro text;

	public TextMeshPro charStats;

	public HorizontalGauge gauge;

	private MedScanSystem medscan;

	public float ScanDuration = 10f;

	public float ScanTimer;

	private string completeString;

	public AudioClip ScanSound;

	public AudioClip TextSound;

	private Coroutine walking;

	private MedScanMinigame.PositionState state;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		this.medscan = (ShipStatus.Instance.Systems[SystemTypes.MedBay] as MedScanSystem);
		this.gauge.Value = 0f;
		base.transform.position = new Vector3(100f, 0f, 0f);
		GameData.PlayerInfo data = PlayerControl.LocalPlayer.Data;
		int playerId = (int)data.PlayerId;
		int num = data.ColorId.Wrap(Palette.ColorNames.Length);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MedID, Array.Empty<object>()) + " ");
		string text = DestroyableSingleton<TranslationController>.Instance.GetString(Palette.ColorNames[num], Array.Empty<object>()).ToUpperInvariant();
		if (text.Length > 3)
		{
			text = text.Substring(0, 3);
		}
		stringBuilder.Append(text);
		stringBuilder.Append("P" + playerId.ToString());
		stringBuilder.Append(new string(' ', 8));
		stringBuilder.Append(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MedHT, Array.Empty<object>()) + " 3' 6\"");
		stringBuilder.Append(new string(' ', 8));
		stringBuilder.Append(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MedWT, Array.Empty<object>()) + " 92lb");
		stringBuilder.AppendLine();
		stringBuilder.Append(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MedC, Array.Empty<object>()) + " ");
		stringBuilder.Append(DestroyableSingleton<TranslationController>.Instance.GetString(Palette.ColorNames[num], Array.Empty<object>()).PadRight(17));
		stringBuilder.Append(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MedBT, Array.Empty<object>()) + " ");
		stringBuilder.Append(MedScanMinigame.BloodTypes[playerId * 7 % MedScanMinigame.BloodTypes.Length]);
		this.completeString = stringBuilder.ToString();
		this.charStats.text = string.Empty;
		ShipStatus.Instance.RpcRepairSystem(SystemTypes.MedBay, playerId | 128);
		this.walking = base.StartCoroutine(this.WalkToOffset());
		base.SetupInput(true);
	}

	private IEnumerator WalkToOffset()
	{
		float num = 24f;
		this.state = MedScanMinigame.PositionState.WalkingToOffset;
		PlayerPhysics myPhysics = PlayerControl.LocalPlayer.MyPhysics;
		Vector2 vector = ShipStatus.Instance.MedScanner.Position;
		Vector2 vector2 = Vector2.left.Rotate((float)PlayerControl.LocalPlayer.PlayerId * num);
		vector += vector2 / 2f;
		Camera.main.GetComponent<FollowerCamera>().Locked = false;
		yield return myPhysics.WalkPlayerTo(vector, 0.001f, 1f);
		yield return new WaitForSeconds(0.1f);
		Camera.main.GetComponent<FollowerCamera>().Locked = true;
		this.walking = null;
		yield break;
	}

	private IEnumerator WalkToPad()
	{
		this.state = MedScanMinigame.PositionState.WalkingToPad;
		PlayerPhysics myPhysics = PlayerControl.LocalPlayer.MyPhysics;
		Vector2 worldPos = ShipStatus.Instance.MedScanner.Position;
		Camera.main.GetComponent<FollowerCamera>().Locked = false;
		yield return myPhysics.WalkPlayerTo(worldPos, 0.001f, 1f);
		yield return new WaitForSeconds(0.1f);
		Camera.main.GetComponent<FollowerCamera>().Locked = true;
		this.walking = null;
		yield break;
	}

	private void FixedUpdate()
	{
		if (this.MyNormTask.IsComplete)
		{
			return;
		}
		byte playerId = PlayerControl.LocalPlayer.PlayerId;
		if (this.medscan.CurrentUser != playerId)
		{
			if (this.medscan.CurrentUser == 255)
			{
				this.text.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MedscanRequested, Array.Empty<object>());
				return;
			}
			GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(this.medscan.CurrentUser);
			this.text.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MedscanWaitingFor, new object[]
			{
				playerById.PlayerName
			});
			return;
		}
		else
		{
			if (this.state != MedScanMinigame.PositionState.WalkingToPad)
			{
				if (this.walking != null)
				{
					base.StopCoroutine(this.walking);
				}
				this.walking = base.StartCoroutine(this.WalkToPad());
				return;
			}
			if (this.walking != null)
			{
				return;
			}
			if (this.ScanTimer == 0f)
			{
				PlayerControl.LocalPlayer.RpcSetScanner(true);
				SoundManager.Instance.PlaySound(this.ScanSound, false, 1f);
				VibrationManager.Vibrate(0.3f, 0.3f, 0f, VibrationManager.VibrationFalloff.None, this.ScanSound, false);
			}
			this.ScanTimer += Time.fixedDeltaTime;
			this.gauge.Value = this.ScanTimer / this.ScanDuration;
			int num = (int)(Mathf.Min(1f, this.ScanTimer / this.ScanDuration * 1.25f) * (float)this.completeString.Length);
			if (num > this.charStats.text.Length)
			{
				this.charStats.text = this.completeString.Substring(0, num);
				if (this.completeString[num - 1] != ' ')
				{
					SoundManager.Instance.PlaySoundImmediate(this.TextSound, false, 0.7f, 0.3f);
				}
			}
			if (this.ScanTimer >= this.ScanDuration)
			{
				PlayerControl.LocalPlayer.RpcSetScanner(false);
				this.text.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MedscanCompleted, Array.Empty<object>());
				this.MyNormTask.NextStep();
				ShipStatus.Instance.RpcRepairSystem(SystemTypes.MedBay, (int)(playerId | 64));
				base.StartCoroutine(base.CoStartClose(0.75f));
				return;
			}
			this.text.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MedscanCompleteIn, new object[]
			{
				(int)(this.ScanDuration - this.ScanTimer)
			});
			return;
		}
	}

	public override void Close()
	{
		base.StopAllCoroutines();
		byte playerId = PlayerControl.LocalPlayer.PlayerId;
		SoundManager.Instance.StopSound(this.TextSound);
		SoundManager.Instance.StopSound(this.ScanSound);
		PlayerControl.LocalPlayer.RpcSetScanner(false);
		ShipStatus.Instance.RpcRepairSystem(SystemTypes.MedBay, (int)(playerId | 64));
		Camera.main.GetComponent<FollowerCamera>().Locked = false;
		VibrationManager.CancelVibration(this.ScanSound);
		base.Close();
	}

	private enum PositionState
	{
		None,
		WalkingToPad,
		WalkingToOffset
	}
}
