using System;
using System.Linq;
using Rewired;
using UnityEngine;

public class PolishRubyGame : Minigame
{
	public PassiveButton[] Buttons;

	public SpriteRenderer[] Sparkles;

	public int[] swipes;

	public Vector2[] directions;

	public int swipesToClean = 6;

	public AudioClip[] rubSounds;

	public AudioClip sparkleSound;

	public Transform cursorObject;

	public Transform handWipeObject;

	public SpriteRenderer[] handSprites;

	private Controller cont = new Controller();

	private bool oldCursorOverlapsSmudge;

	public void Start()
	{
		this.swipes = new int[this.Buttons.Length];
		this.directions = new Vector2[this.Buttons.Length];
		this.Buttons.ForEach(delegate(PassiveButton b)
		{
			b.gameObject.SetActive(false);
		});
		int num = IntRange.Next(3, 5);
		for (int i = 0; i < num; i++)
		{
			(from t in this.Buttons
			where !t.isActiveAndEnabled
			select t).Random<PassiveButton>().gameObject.SetActive(true);
		}
		base.SetupInput(true);
		foreach (SpriteRenderer playerMaterialColors in this.handSprites)
		{
			PlayerControl.LocalPlayer.SetPlayerMaterialColors(playerMaterialColors);
		}
		this.UpdateSpriteColor(false);
	}

	private void UpdateSpriteColor(bool cursorOverlapsSmudge)
	{
		Color white = Color.white;
		white.a = (cursorOverlapsSmudge ? 1f : 0.5f);
		SpriteRenderer[] array = this.handSprites;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].color = white;
		}
	}

	public void PlaySparkleSound()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySoundImmediate(this.sparkleSound, false, 1f, 1f);
		}
	}

	public void Update()
	{
		if (this.amClosing != Minigame.CloseState.None)
		{
			return;
		}
		this.cont.Update();
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			this.cursorObject.gameObject.SetActive(true);
			Player player = ReInput.players.GetPlayer(0);
			Vector2 axis2DRaw = player.GetAxis2DRaw(13, 14);
			Vector2 axis2DRaw2 = player.GetAxis2DRaw(16, 17);
			Vector3 localPosition = this.cursorObject.localPosition;
			Vector3 localPosition2 = this.handWipeObject.localPosition;
			localPosition.x += axis2DRaw.x * Time.deltaTime * 3f;
			localPosition.y += axis2DRaw.y * Time.deltaTime * 3f;
			localPosition2.x = axis2DRaw2.x * 0.35f;
			localPosition2.y = axis2DRaw2.y * 0.35f;
			this.cursorObject.transform.localPosition = localPosition;
			this.handWipeObject.transform.localPosition = localPosition2;
			Vector3 position = this.cursorObject.position;
			bool flag = false;
			for (int i = 0; i < this.Buttons.Length; i++)
			{
				PassiveButton passiveButton = this.Buttons[i];
				if (passiveButton.isActiveAndEnabled && passiveButton.Colliders[0].OverlapPoint(position))
				{
					if (!this.Sparkles[i].enabled)
					{
						flag = true;
					}
					Vector2 vector = this.directions[i];
					float y = vector.y;
					vector.y = localPosition2.x - vector.x;
					vector.x = localPosition2.x;
					this.directions[i] = vector;
					if (Mathf.Sign(y) != Mathf.Sign(vector.y) && vector.y != 0f)
					{
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySoundImmediate(this.rubSounds.Random<AudioClip>(), false, 1f, 1f);
						}
						VibrationManager.Vibrate(0.1f, 0.1f, 0.05f, VibrationManager.VibrationFalloff.Linear, null, false);
						int num = this.swipes[i] = this.swipes[i] + 1;
						if (num <= this.swipesToClean)
						{
							passiveButton.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, Palette.ClearWhite, (float)num / (float)this.swipesToClean);
							if (num == this.swipesToClean)
							{
								this.Sparkles[i].enabled = true;
							}
						}
					}
				}
			}
			if (this.oldCursorOverlapsSmudge != flag)
			{
				this.oldCursorOverlapsSmudge = flag;
				this.UpdateSpriteColor(flag);
			}
		}
		else
		{
			this.cursorObject.gameObject.SetActive(false);
			Controller controller = DestroyableSingleton<PassiveButtonManager>.Instance.controller;
			if (controller.AnyTouch)
			{
				Vector2 position2 = controller.Touches[0].Position;
				for (int j = 0; j < this.Buttons.Length; j++)
				{
					PassiveButton passiveButton2 = this.Buttons[j];
					if (passiveButton2.isActiveAndEnabled && passiveButton2.Colliders[0].OverlapPoint(position2))
					{
						Vector2 vector2 = this.directions[j];
						float y2 = vector2.y;
						vector2.y = position2.x - vector2.x;
						vector2.x = position2.x;
						this.directions[j] = vector2;
						if (Mathf.Sign(y2) != Mathf.Sign(vector2.y) && vector2.y != 0f)
						{
							if (Constants.ShouldPlaySfx())
							{
								SoundManager.Instance.PlaySoundImmediate(this.rubSounds.Random<AudioClip>(), false, 1f, 1f);
							}
							int num2 = this.swipes[j] = this.swipes[j] + 1;
							if (num2 <= this.swipesToClean)
							{
								passiveButton2.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, Palette.ClearWhite, (float)num2 / (float)this.swipesToClean);
								if (num2 == this.swipesToClean)
								{
									this.Sparkles[j].enabled = true;
								}
							}
						}
					}
				}
			}
		}
		bool flag2 = true;
		for (int k = 0; k < this.Buttons.Length; k++)
		{
			if (this.Buttons[k].isActiveAndEnabled && this.swipes[k] < this.swipesToClean)
			{
				flag2 = false;
				break;
			}
		}
		if (flag2)
		{
			this.MyNormTask.NextStep();
			this.Close();
		}
	}
}
