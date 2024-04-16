using System;
using Rewired;
using TMPro;
using UnityEngine;

public class TempMinigame : Minigame
{
	public TextMeshPro LogText;

	public TextMeshPro ReadingText;

	public IntRange LogRange;

	public IntRange ReadingRange;

	private int logValue;

	private int readingValue;

	public AudioClip ButtonSound;

	private float deltaSinceLastChangeNumber;

	public const float CHANGE_NUMBER_UPDATE_THRESHOLD_MIN = 0.05f;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		this.logValue = this.LogRange.Next();
		this.readingValue = this.ReadingRange.Next();
		this.ReadingText.text = this.readingValue.ToString();
		this.ChangeNumber(0);
		base.SetupInput(true);
	}

	public void ChangeNumber(int dir)
	{
		if (this.logValue == this.readingValue)
		{
			return;
		}
		if (dir != 0 && Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.ButtonSound, false, 1f);
		}
		this.logValue += dir;
		this.LogText.text = this.logValue.ToString();
		if (this.logValue == this.readingValue)
		{
			this.MyNormTask.NextStep();
			base.StartCoroutine(base.CoStartClose(0.75f));
		}
		this.deltaSinceLastChangeNumber = 0f;
	}

	private void Update()
	{
		float axisRaw = ReInput.players.GetPlayer(0).GetAxisRaw(14);
		this.deltaSinceLastChangeNumber += Time.deltaTime;
		float num = 0.05f;
		int num2 = 0;
		if ((double)axisRaw > 0.9)
		{
			num = 0.05f;
			num2 = 1;
		}
		else if ((double)axisRaw > 0.7)
		{
			num = 0.1f;
			num2 = 1;
		}
		else if ((double)axisRaw > 0.5)
		{
			num = 0.2f;
			num2 = 1;
		}
		else if ((double)axisRaw > 0.4)
		{
			num = 0.3f;
			num2 = 1;
		}
		else if (axisRaw < -0.9f)
		{
			num = 0.05f;
			num2 = -1;
		}
		else if (axisRaw < -0.7f)
		{
			num = 0.1f;
			num2 = -1;
		}
		else if (axisRaw < -0.5f)
		{
			num = 0.2f;
			num2 = -1;
		}
		else if (axisRaw < -0.4f)
		{
			num = 0.3f;
			num2 = -1;
		}
		if (num2 != 0 && this.deltaSinceLastChangeNumber > num)
		{
			this.ChangeNumber(num2);
		}
	}
}
