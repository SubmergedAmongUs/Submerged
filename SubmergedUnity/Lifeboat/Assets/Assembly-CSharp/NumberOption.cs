using System;
using TMPro;
using UnityEngine;

public class NumberOption : OptionBehaviour
{
	public TextMeshPro TitleText;

	public TextMeshPro ValueText;

	public float Value = 1f;

	private float oldValue = float.MaxValue;

	public float Increment;

	public FloatRange ValidRange = new FloatRange(0f, 2f);

	public string FormatString = "0.0";

	public bool ZeroIsInfinity;

	public NumberSuffixes SuffixType = NumberSuffixes.Multiplier;

	public void OnEnable()
	{
		this.TitleText.text = DestroyableSingleton<TranslationController>.Instance.GetString(this.Title, Array.Empty<object>());
		this.FixedUpdate();
		GameOptionsData gameOptions = PlayerControl.GameOptions;
		StringNames title = this.Title;
		switch (title)
		{
		case StringNames.GameNumImpostors:
			this.Value = (float)gameOptions.NumImpostors;
			return;
		case StringNames.GameNumMeetings:
			this.Value = (float)gameOptions.NumEmergencyMeetings;
			return;
		case StringNames.GameDiscussTime:
			this.Value = (float)gameOptions.DiscussionTime;
			return;
		case StringNames.GameVotingTime:
			this.Value = (float)gameOptions.VotingTime;
			return;
		case StringNames.GamePlayerSpeed:
			this.Value = gameOptions.PlayerSpeedMod;
			return;
		case StringNames.GameCrewLight:
			this.Value = gameOptions.CrewLightMod;
			return;
		case StringNames.GameImpostorLight:
			this.Value = gameOptions.ImpostorLightMod;
			return;
		case StringNames.GameKillCooldown:
			this.Value = gameOptions.KillCooldown;
			return;
		case StringNames.GameKillDistance:
			break;
		case StringNames.GameCommonTasks:
			this.Value = (float)gameOptions.NumCommonTasks;
			return;
		case StringNames.GameLongTasks:
			this.Value = (float)gameOptions.NumLongTasks;
			return;
		case StringNames.GameShortTasks:
			this.Value = (float)gameOptions.NumShortTasks;
			return;
		default:
			if (title == StringNames.GameEmergencyCooldown)
			{
				this.Value = (float)gameOptions.EmergencyCooldown;
				return;
			}
			break;
		}
		Debug.Log("Ono, unrecognized setting: " + this.Title.ToString());
	}

	private void FixedUpdate()
	{
		if (this.oldValue != this.Value)
		{
			this.oldValue = this.Value;
			if (this.ZeroIsInfinity && Mathf.Abs(this.Value) < 0.0001f)
			{
				this.ValueText.text = "∞";
				return;
			}
			if (this.SuffixType == NumberSuffixes.None)
			{
				this.ValueText.text = this.Value.ToString(this.FormatString);
				return;
			}
			if (this.SuffixType == NumberSuffixes.Multiplier)
			{
				this.ValueText.text = this.Value.ToString(this.FormatString) + "x";
				return;
			}
			this.ValueText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameSecondsAbbrev, new object[]
			{
				this.Value.ToString(this.FormatString)
			});
		}
	}

	public void Increase()
	{
		this.Value = this.ValidRange.Clamp(this.Value + this.Increment);
		this.OnValueChanged(this);
	}

	public void Decrease()
	{
		this.Value = this.ValidRange.Clamp(this.Value - this.Increment);
		this.OnValueChanged(this);
	}

	public override float GetFloat()
	{
		return this.Value;
	}

	public override int GetInt()
	{
		return (int)this.Value;
	}
}
