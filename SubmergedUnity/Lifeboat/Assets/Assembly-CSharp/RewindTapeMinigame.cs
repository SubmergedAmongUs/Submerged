using System;
using TMPro;
using UnityEngine;

public class RewindTapeMinigame : Minigame
{
	private const float MaxTime = 83544f;

	private float targetTime;

	private float currentTime;

	public SpriteRenderer LeftWheel;

	public SpriteRenderer LeftTape;

	public SpriteRenderer RightWheel;

	public SpriteRenderer RightTape;

	public TextMeshPro TargetText;

	public TextMeshPro CurrentText;

	public SpriteRenderer RewindButton;

	public Sprite RewindNormal;

	public Sprite RewindDown;

	public SpriteRenderer FastFwdButton;

	public Sprite FastFwdNormal;

	public Sprite FastFwdDown;

	public SpriteRenderer PlayButton;

	public Sprite PlayNormal;

	public Sprite PlayDown;

	public SpriteRenderer PauseButton;

	public Sprite PauseNormal;

	public Sprite PauseDown;

	public SpriteRenderer RewindGlyph;

	public SpriteRenderer FastFwdGlyph;

	public SpriteRenderer PlayGlyph;

	public SpriteRenderer PauseGlyph;

	public float upGlyphYPos;

	public float pressedGlyphYPos;

	public Color upGlyphColor;

	public Color pressedGlyphColor;

	private float direction;

	public AudioClip buttonSound;

	public AudioClip playStartSound;

	public AudioClip playLoopSound;

	public AudioClip playStopSound;

	private AudioSource loopSound;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		this.targetTime = BitConverter.ToSingle(this.MyNormTask.Data, 0);
		this.currentTime = BitConverter.ToSingle(this.MyNormTask.Data, 4);
		this.UpdateText(this.TargetText, this.targetTime);
		this.UpdateText(this.CurrentText, this.currentTime);
		this.loopSound = SoundManager.Instance.GetNamedAudioSource("rewindLoop");
		this.loopSound.volume = 0f;
		this.loopSound.pitch = 0.5f;
		this.loopSound.loop = true;
		this.loopSound.clip = this.playLoopSound;
		this.loopSound.Play();
		base.SetupInput(true);
	}

	private void UpdateText(TextMeshPro targetText, float targetTime)
	{
		int num = (int)(targetTime / 3600f);
		int num2 = (int)((targetTime - (float)(num * 3600)) / 60f);
		int num3 = (int)(targetTime % 60f);
		targetText.text = string.Format("{0}:{1:00}:{2:00}", num, num2, num3);
	}

	public void Update()
	{
		if (this.amClosing != Minigame.CloseState.None)
		{
			return;
		}
		if (this.direction != 0f)
		{
			this.currentTime += this.direction * Time.deltaTime;
			if (this.currentTime < 0f)
			{
				this.currentTime = 0f;
				this.Pause();
			}
			if (this.currentTime > 83544f)
			{
				this.currentTime = 83544f;
				this.Pause();
			}
			if (Mathf.Abs(this.direction) < 120f)
			{
				if (this.direction < -1.05f)
				{
					this.direction -= 0.1f;
				}
				if (this.direction > 1.05f)
				{
					this.direction += 0.1f;
				}
			}
			this.UpdateText(this.CurrentText, this.currentTime);
			this.LeftWheel.transform.Rotate(0f, 0f, 5f * this.direction);
			this.RightWheel.transform.Rotate(0f, 0f, 5f * this.direction);
			float num = FloatRange.ReverseLerp(this.currentTime, 0f, 83544f);
			float num2 = Mathf.Lerp(0.52f, 1f, num);
			this.LeftTape.transform.localScale = new Vector3(num2, num2, num2);
			num2 = Mathf.Lerp(0.52f, 1f, 1f - num);
			this.LeftTape.transform.localScale = new Vector3(num2, num2, num2);
			if (Constants.ShouldPlaySfx())
			{
				this.loopSound.volume = Mathf.Lerp(this.loopSound.volume, 1f, Time.deltaTime);
				float num3 = Mathf.Lerp(0.5f, 1.25f, Mathf.Abs(this.direction) / 2f);
				this.loopSound.pitch = Mathf.Lerp(this.loopSound.pitch, num3, Time.deltaTime);
				return;
			}
		}
		else if (Mathf.Abs(this.targetTime - this.currentTime) <= 1f)
		{
			this.MyNormTask.NextStep();
			base.StartCoroutine(base.CoStartClose(0.5f));
		}
	}

	private void SetYPos(Transform t, float newYPos)
	{
		Vector3 localPosition = t.localPosition;
		localPosition.y = newYPos;
		t.localPosition = localPosition;
	}

	public void Rewind()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.buttonSound, false, 1f);
		}
		this.direction = -1.1f;
		this.RewindButton.sprite = this.RewindDown;
		this.PlayButton.sprite = this.PlayNormal;
		this.PauseButton.sprite = this.PauseNormal;
		this.FastFwdButton.sprite = this.FastFwdNormal;
		this.RewindGlyph.color = this.pressedGlyphColor;
		this.PlayGlyph.color = this.upGlyphColor;
		this.PauseGlyph.color = this.upGlyphColor;
		this.FastFwdGlyph.color = this.upGlyphColor;
		this.SetYPos(this.RewindGlyph.transform, this.pressedGlyphYPos);
		this.SetYPos(this.PlayGlyph.transform, this.upGlyphYPos);
		this.SetYPos(this.PauseGlyph.transform, this.upGlyphYPos);
		this.SetYPos(this.FastFwdGlyph.transform, this.upGlyphYPos);
	}

	public void FastForward()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.buttonSound, false, 1f);
		}
		this.direction = 1.1f;
		this.RewindButton.sprite = this.RewindNormal;
		this.PlayButton.sprite = this.PlayNormal;
		this.PauseButton.sprite = this.PauseNormal;
		this.FastFwdButton.sprite = this.FastFwdDown;
		this.RewindGlyph.color = this.upGlyphColor;
		this.PlayGlyph.color = this.upGlyphColor;
		this.PauseGlyph.color = this.upGlyphColor;
		this.FastFwdGlyph.color = this.pressedGlyphColor;
		this.SetYPos(this.RewindGlyph.transform, this.upGlyphYPos);
		this.SetYPos(this.PlayGlyph.transform, this.upGlyphYPos);
		this.SetYPos(this.PauseGlyph.transform, this.upGlyphYPos);
		this.SetYPos(this.FastFwdGlyph.transform, this.pressedGlyphYPos);
	}

	public void Pause()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.buttonSound, false, 1f);
		}
		this.direction = 0f;
		this.RewindButton.sprite = this.RewindNormal;
		this.PlayButton.sprite = this.PlayNormal;
		this.PauseButton.sprite = this.PauseDown;
		this.FastFwdButton.sprite = this.FastFwdNormal;
		this.loopSound.volume = 0f;
		this.loopSound.pitch = 0.5f;
		this.RewindGlyph.color = this.upGlyphColor;
		this.PlayGlyph.color = this.upGlyphColor;
		this.PauseGlyph.color = this.pressedGlyphColor;
		this.FastFwdGlyph.color = this.upGlyphColor;
		this.SetYPos(this.RewindGlyph.transform, this.upGlyphYPos);
		this.SetYPos(this.PlayGlyph.transform, this.upGlyphYPos);
		this.SetYPos(this.PauseGlyph.transform, this.pressedGlyphYPos);
		this.SetYPos(this.FastFwdGlyph.transform, this.upGlyphYPos);
	}

	public void Play()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.buttonSound, false, 1f);
		}
		this.direction = 1f;
		this.RewindButton.sprite = this.RewindNormal;
		this.PlayButton.sprite = this.PlayDown;
		this.PauseButton.sprite = this.PauseNormal;
		this.FastFwdButton.sprite = this.FastFwdNormal;
		this.RewindGlyph.color = this.upGlyphColor;
		this.PlayGlyph.color = this.pressedGlyphColor;
		this.PauseGlyph.color = this.upGlyphColor;
		this.FastFwdGlyph.color = this.upGlyphColor;
		this.SetYPos(this.RewindGlyph.transform, this.upGlyphYPos);
		this.SetYPos(this.PlayGlyph.transform, this.pressedGlyphYPos);
		this.SetYPos(this.PauseGlyph.transform, this.upGlyphYPos);
		this.SetYPos(this.FastFwdGlyph.transform, this.upGlyphYPos);
	}

	public override void Close()
	{
		SoundManager.Instance.StopNamedSound("rewindLoop");
		BitConverter.GetBytes(this.targetTime).CopyTo(this.MyNormTask.Data, 0);
		BitConverter.GetBytes(this.currentTime).CopyTo(this.MyNormTask.Data, 4);
		base.Close();
	}
}
