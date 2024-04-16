using System;
using System.Collections;
using Rewired;
using TMPro;
using UnityEngine;

public class WifiGame : Minigame
{
	private const int WaitDuration = 60;

	public SlideBar Slider;

	public TextMeshPro StatusText;

	public SpriteRenderer[] Lights;

	public Sprite LightOn;

	public Sprite LightOff;

	public AudioClip SliderClick;

	private bool WifiOff;

	private TouchpadBehavior touchpad;

	private float initialSlider;

	private Controller controller = new Controller();

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		if (this.MyNormTask.TimerStarted == NormalPlayerTask.TimerState.NotStarted)
		{
			this.TurnOn(true);
		}
		else
		{
			this.TurnOff(true);
		}
		this.touchpad = base.GetComponent<TouchpadBehavior>();
		base.SetupInput(true);
	}

	public void Update()
	{
		if (this.MyNormTask.IsComplete)
		{
			return;
		}
		this.controller.Update();
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			float num = ReInput.players.GetPlayer(0).GetAxis(17);
			if (this.touchpad.IsTouching())
			{
				num = this.touchpad.GetTouchVector().y;
			}
			if (!this.WifiOff && this.MyNormTask.TimerStarted == NormalPlayerTask.TimerState.NotStarted)
			{
				float value;
				if (this.touchpad.IsTouching())
				{
					if (this.touchpad.IsFirstTouch())
					{
						this.initialSlider = this.Slider.Value;
					}
					value = this.initialSlider + num;
				}
				else
				{
					value = this.Slider.Value + num * Time.deltaTime * 3f;
				}
				this.Slider.SetValue(value);
			}
			else if (this.WifiOff && this.MyNormTask.TimerStarted == NormalPlayerTask.TimerState.Finished)
			{
				float value2 = this.Slider.Value + num * Time.deltaTime * 3f;
				this.Slider.SetValue(value2);
			}
		}
		if (this.WifiOff)
		{
			if (this.MyNormTask.TimerStarted == NormalPlayerTask.TimerState.Finished)
			{
				this.StatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.WifiPleasePowerOn, Array.Empty<object>());
			}
			else if (this.MyNormTask.TimerStarted == NormalPlayerTask.TimerState.Started)
			{
				this.StatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.WifiPleaseReturnIn, new object[]
				{
					Mathf.CeilToInt(this.MyNormTask.TaskTimer)
				});
			}
		}
		if (!this.WifiOff && (double)this.Slider.Value < 0.1)
		{
			this.TurnOff(false);
			return;
		}
		if (this.WifiOff && (double)this.Slider.Value > 0.9)
		{
			this.TurnOn(false);
		}
	}

	private void TurnOn(bool first = false)
	{
		if (Constants.ShouldPlaySfx() && !first)
		{
			SoundManager.Instance.PlaySound(this.SliderClick, false, 1f);
		}
		this.Slider.Value = 1f;
		this.Slider.UpdateValue();
		this.WifiOff = false;
		if (!first)
		{
			base.StopAllCoroutines();
		}
		base.StartCoroutine(this.RunLights(this.MyNormTask.TimerStarted == NormalPlayerTask.TimerState.Finished));
	}

	private void TurnOff(bool first = false)
	{
		if (Constants.ShouldPlaySfx() && !first)
		{
			SoundManager.Instance.PlaySound(this.SliderClick, false, 1f);
		}
		this.Slider.Value = 0f;
		this.Slider.UpdateValue();
		this.WifiOff = true;
		if (!first)
		{
			base.StopAllCoroutines();
		}
		this.Lights.ForEach(delegate(SpriteRenderer s)
		{
			s.sprite = this.LightOff;
		});
		if (this.MyNormTask.TimerStarted == NormalPlayerTask.TimerState.NotStarted)
		{
			this.StatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.WifiPleaseReturnIn, Array.Empty<object>()) + " " + 60.ToString();
			this.MyNormTask.TaskTimer = 60f;
			this.MyNormTask.TimerStarted = NormalPlayerTask.TimerState.Started;
		}
	}

	private IEnumerator RunLights(bool finishing)
	{
		if (!finishing)
		{
			this.StatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.WifiRebootRequired, Array.Empty<object>());
			this.Lights.ForEach(delegate(SpriteRenderer s)
			{
				s.sprite = this.LightOn;
			});
			this.Lights[1].sprite = this.LightOff;
			this.Lights[3].sprite = this.LightOff;
			this.Lights[4].sprite = this.LightOff;
			this.Lights[6].sprite = this.LightOff;
			yield return Effects.All(new IEnumerator[]
			{
				this.CoBlinkLight(this.Lights[2], 0.3f)
			});
		}
		else
		{
			this.StatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.WifiPleaseWait, Array.Empty<object>());
			for (float timer = 0f; timer < 3f; timer += Time.deltaTime)
			{
				float num = timer / 3f;
				for (int i = 0; i < this.Lights.Length; i++)
				{
					float num2 = 0.75f * (float)i / (float)this.Lights.Length;
					float num3 = 0.75f * (float)(i + 1) / (float)this.Lights.Length;
					this.Lights[i].sprite = ((num > num2 && num < num3) ? this.LightOn : this.LightOff);
				}
				yield return null;
			}
			this.StatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.WifiRebootComplete, Array.Empty<object>());
			this.Lights.ForEach(delegate(SpriteRenderer s)
			{
				s.sprite = this.LightOn;
			});
			base.StartCoroutine(Effects.All(new IEnumerator[]
			{
				this.CoBlinkLight(this.Lights[3], 0.1f),
				this.CoBlinkLight(this.Lights[4], 0.09f),
				this.CoBlinkLight(this.Lights[5], 0.1f),
				this.CoBlinkLight(this.Lights[6], 0.05f),
				this.CoBlinkLight(this.Lights[7], 0.5f)
			}));
			this.MyNormTask.NextStep();
			base.StartCoroutine(base.CoStartClose(0.75f));
		}
		yield break;
	}

	private IEnumerator CoBlinkLight(SpriteRenderer light, float delay)
	{
		for (;;)
		{
			light.sprite = this.LightOn;
			yield return Effects.Wait(delay);
			light.sprite = this.LightOff;
			yield return Effects.Wait(delay);
		}
		yield break;
	}
}
