using System;
using System.Collections;
using Rewired;
using TMPro;
using UnityEngine;

public class SampleMinigame : Minigame
{
	private static StringNames[] ProcessingStrings = new StringNames[]
	{
		StringNames.TakeBreak,
		StringNames.GrabCoffee,
		StringNames.DontNeedWait,
		StringNames.DoSomethingElse
	};

	private const float PanelMoveDuration = 0.75f;

	private const byte TubeMask = 15;

	public TextMeshPro UpperText;

	public TextMeshPro LowerText;

	public float TimePerStep = 15f;

	public FloatRange platformY = new FloatRange(-3.5f, -0.75f);

	public FloatRange dropperX = new FloatRange(-1.25f, 1.25f);

	public SpriteRenderer CenterPanel;

	public SpriteRenderer Dropper;

	public SpriteRenderer[] Tubes;

	public SpriteRenderer[] Buttons;

	public SpriteRenderer[] LowerButtons;

	public AudioClip ButtonSound;

	public AudioClip PanelMoveSound;

	public AudioClip FailSound;

	public AudioClip[] DropSounds;

	private RandomFill<AudioClip> dropSounds;

	public Transform whichButtonSelector;

	public int whichButtonSelected = 2;

	private float selectMoveCooldown;

	private SampleMinigame.States State
	{
		get
		{
			return (SampleMinigame.States)this.MyNormTask.Data[0];
		}
		set
		{
			this.MyNormTask.Data[0] = (byte)value;
		}
	}

	private int AnomalyId
	{
		get
		{
			return (int)this.MyNormTask.Data[1];
		}
		set
		{
			this.MyNormTask.Data[1] = (byte)value;
		}
	}

	public void Awake()
	{
		this.dropSounds = new RandomFill<AudioClip>();
		this.dropSounds.Set(this.DropSounds);
		this.LowerText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MedHello, Array.Empty<object>());
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		SampleMinigame.States state = this.State;
		if (state <= SampleMinigame.States.AwaitingStart)
		{
			if (state != SampleMinigame.States.PrepareSample)
			{
				if (state == SampleMinigame.States.AwaitingStart)
				{
					this.LowerText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SamplesPress, Array.Empty<object>()).PadRight(20, ' ') + "-->";
					this.SetPlatformTop();
				}
			}
			else
			{
				base.StartCoroutine(this.BringPanelUp(true));
			}
		}
		else if (state != SampleMinigame.States.Selection)
		{
			if (state == SampleMinigame.States.Processing)
			{
				for (int i = 0; i < this.Tubes.Length; i++)
				{
					this.Tubes[i].color = Color.blue;
				}
				this.LowerText.text = DestroyableSingleton<TranslationController>.Instance.GetString(SampleMinigame.ProcessingStrings.Random<StringNames>(), Array.Empty<object>());
				this.SetPlatformBottom();
			}
		}
		else
		{
			for (int j = 0; j < this.Tubes.Length; j++)
			{
				this.Tubes[j].color = Color.blue;
			}
			this.Tubes[this.AnomalyId].color = Color.red;
			this.LowerText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SamplesSelect, Array.Empty<object>());
			this.SetPlatformTop();
		}
		base.SetupInput(true);
	}

	private void SetPlatformBottom()
	{
		Vector3 localPosition = this.CenterPanel.transform.localPosition;
		localPosition.y = this.platformY.min;
		this.CenterPanel.transform.localPosition = localPosition;
	}

	private void SetPlatformTop()
	{
		Vector3 localPosition = this.CenterPanel.transform.localPosition;
		localPosition.y = this.platformY.max;
		this.CenterPanel.transform.localPosition = localPosition;
	}

	public void Update()
	{
		Player player = ReInput.players.GetPlayer(0);
		SampleMinigame.States state = this.State;
		if (state != SampleMinigame.States.AwaitingStart)
		{
			if (state != SampleMinigame.States.Selection)
			{
				if (state != SampleMinigame.States.Processing)
				{
					return;
				}
				if (this.whichButtonSelector.gameObject.activeSelf)
				{
					this.whichButtonSelector.gameObject.SetActive(false);
				}
			}
			else
			{
				if (!this.whichButtonSelector.gameObject.activeSelf)
				{
					this.whichButtonSelector.gameObject.SetActive(true);
				}
				float axis = player.GetAxis(13);
				if (Mathf.Abs(axis) > 0.5f)
				{
					if (this.selectMoveCooldown > 0f)
					{
						this.selectMoveCooldown -= Time.deltaTime;
					}
					else
					{
						int num = (int)Mathf.Sign(axis);
						this.whichButtonSelected += num;
						this.whichButtonSelected = Mathf.Clamp(this.whichButtonSelected, 0, 4);
						Vector3 localPosition = this.whichButtonSelector.localPosition;
						Vector3 localPosition2 = this.Buttons[this.whichButtonSelected].transform.localPosition;
						localPosition.x = localPosition2.x;
						localPosition.y = localPosition2.y;
						this.whichButtonSelector.transform.localPosition = localPosition;
						this.selectMoveCooldown = 0.25f;
					}
				}
				else
				{
					this.selectMoveCooldown = 0f;
				}
				if (player.GetButtonDown(11))
				{
					ButtonBehavior component = this.Buttons[this.whichButtonSelected].GetComponent<ButtonBehavior>();
					if (component)
					{
						component.OnClick.Invoke();
						return;
					}
				}
			}
		}
		else
		{
			if (this.whichButtonSelector.gameObject.activeSelf)
			{
				this.whichButtonSelector.gameObject.SetActive(false);
			}
			if (player.GetButtonDown(11))
			{
				this.NextStep();
				return;
			}
		}
	}

	public void FixedUpdate()
	{
		if (this.State == SampleMinigame.States.Processing)
		{
			if (this.MyNormTask.TaskTimer <= 0f)
			{
				this.State = SampleMinigame.States.Selection;
				this.LowerText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SamplesSelect, Array.Empty<object>());
				this.UpperText.text = "";
				this.AnomalyId = this.Tubes.RandomIdx<SpriteRenderer>();
				this.Tubes[this.AnomalyId].color = Color.red;
				SpriteRenderer[] lowerButtons = this.LowerButtons;
				for (int i = 0; i < lowerButtons.Length; i++)
				{
					lowerButtons[i].color = Color.white;
				}
				base.StartCoroutine(this.BringPanelUp(false));
				return;
			}
			this.UpperText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MedETA, new object[]
			{
				this.MyNormTask.TaskTimer
			});
			return;
		}
		else
		{
			if (this.State == SampleMinigame.States.Selection)
			{
				float num = Mathf.Cos(Time.time * 1.5f) - 0.2f;
				Color color = new Color(num, 1f, num, 1f);
				for (int j = 0; j < this.Buttons.Length; j++)
				{
					this.Buttons[j].color = color;
				}
				return;
			}
			if (this.State == SampleMinigame.States.AwaitingStart)
			{
				float num2 = Mathf.Cos(Time.time * 1.5f) - 0.2f;
				Color color2 = new Color(num2, 1f, num2, 1f);
				SpriteRenderer[] lowerButtons = this.LowerButtons;
				for (int i = 0; i < lowerButtons.Length; i++)
				{
					lowerButtons[i].color = color2;
				}
			}
			return;
		}
	}

	public IEnumerator BringPanelUp(bool isBeginning)
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.PanelMoveSound, false, 1f);
		}
		WaitForFixedUpdate wait = new WaitForFixedUpdate();
		Vector3 pos = this.CenterPanel.transform.localPosition;
		for (float i = 0f; i < 0.75f; i += Time.deltaTime)
		{
			pos.y = this.platformY.Lerp(i / 0.75f);
			this.CenterPanel.transform.localPosition = pos;
			yield return wait;
		}
		if (isBeginning)
		{
			this.State = SampleMinigame.States.AwaitingStart;
			this.LowerText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SamplesPress, Array.Empty<object>()).PadRight(19, ' ') + " -->";
		}
		pos.y = this.platformY.max;
		this.CenterPanel.transform.localPosition = pos;
		yield break;
	}

	public IEnumerator BringPanelDown()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.PanelMoveSound, false, 1f);
		}
		WaitForFixedUpdate wait = new WaitForFixedUpdate();
		Vector3 pos = this.CenterPanel.transform.localPosition;
		for (float i = 0f; i < 0.75f; i += Time.deltaTime)
		{
			pos.y = this.platformY.Lerp(1f - i / 0.75f);
			this.CenterPanel.transform.localPosition = pos;
			yield return wait;
		}
		pos.y = this.platformY.min;
		this.CenterPanel.transform.localPosition = pos;
		yield break;
	}

	private IEnumerator DropTube(int id)
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.dropSounds.Get(), false, 1f);
		}
		this.Tubes[id].color = Color.blue;
		yield break;
	}

	public void SelectTube(int tubeId)
	{
		if (this.State != SampleMinigame.States.Selection)
		{
			return;
		}
		this.State = SampleMinigame.States.PrepareSample;
		for (int i = 0; i < this.Buttons.Length; i++)
		{
			this.Buttons[i].color = Color.white;
		}
		base.StartCoroutine(this.CoSelectTube(this.AnomalyId, tubeId));
	}

	private IEnumerator CoSelectTube(int correctTube, int selectedTube)
	{
		if (selectedTube != correctTube)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.FailSound, false, 1f);
			}
			this.UpperText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.BadResult, Array.Empty<object>());
			this.LowerText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.BadResult, Array.Empty<object>());
			for (int i = 0; i < this.Buttons.Length; i++)
			{
				this.Buttons[i].color = Color.red;
			}
			yield return new WaitForSeconds(0.25f);
			for (int j = 0; j < this.Buttons.Length; j++)
			{
				this.Buttons[j].color = Color.white;
			}
			yield return new WaitForSeconds(0.25f);
			for (int k = 0; k < this.Buttons.Length; k++)
			{
				this.Buttons[k].color = Color.red;
			}
			yield return new WaitForSeconds(0.25f);
			for (int l = 0; l < this.Buttons.Length; l++)
			{
				this.Buttons[l].color = Color.white;
			}
			this.UpperText.text = "";
		}
		else
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.ButtonSound, false, 0.6f);
			}
			this.UpperText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SamplesThanks, Array.Empty<object>());
			this.MyNormTask.NextStep();
			if (this.MyNormTask.IsComplete)
			{
				this.State = SampleMinigame.States.Complete;
				base.StartCoroutine(base.CoStartClose(0.75f));
			}
		}
		int num = this.MyNormTask.MaxStep - this.MyNormTask.taskStep;
		if (num == 0)
		{
			this.LowerText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SamplesComplete, Array.Empty<object>());
		}
		else
		{
			this.LowerText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.More, new object[]
			{
				num
			});
		}
		yield return this.BringPanelDown();
		for (int m = 0; m < this.Tubes.Length; m++)
		{
			this.Tubes[m].color = Color.white;
		}
		if (!this.MyNormTask.IsComplete)
		{
			yield return this.BringPanelUp(true);
		}
		yield break;
	}

	public void NextStep()
	{
		if (this.State != SampleMinigame.States.AwaitingStart)
		{
			return;
		}
		this.State = SampleMinigame.States.Processing;
		SpriteRenderer[] lowerButtons = this.LowerButtons;
		for (int i = 0; i < lowerButtons.Length; i++)
		{
			lowerButtons[i].color = Color.white;
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.ButtonSound, false, 1f).volume = 0.6f;
		}
		base.StartCoroutine(this.CoStartProcessing());
	}

	private IEnumerator CoStartProcessing()
	{
		this.MyNormTask.TaskTimer = this.TimePerStep;
		this.MyNormTask.TimerStarted = NormalPlayerTask.TimerState.Started;
		yield return this.DropLiquid();
		this.LowerText.text = DestroyableSingleton<TranslationController>.Instance.GetString(SampleMinigame.ProcessingStrings.Random<StringNames>(), Array.Empty<object>());
		yield return this.BringPanelDown();
		yield break;
	}

	private IEnumerator DropLiquid()
	{
		this.LowerText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SamplesAdding, Array.Empty<object>());
		WaitForSeconds dropWait = new WaitForSeconds(0.25f);
		WaitForFixedUpdate wait = new WaitForFixedUpdate();
		Vector3 pos = this.Dropper.transform.localPosition;
		yield return dropWait;
		yield return this.DropTube(0);
		Vector2 vPositionDelta = new Vector2(-0.25f, 0.25f);
		Vector2 vPosition = new Vector2(1f, 0f);
		VibrationManager.Vibrate(vPosition.x * 2.5f, vPosition.y * 2.5f, 0.2f, VibrationManager.VibrationFalloff.None, this.dropSounds.Get(), false);
		int num;
		for (int step = -2; step < 2; step = num)
		{
			float start = (float)step / 2f * 1.25f;
			float xTarg = (float)(step + 1) / 2f * 1.25f;
			for (float i = 0f; i < 0.15f; i += Time.deltaTime)
			{
				pos.x = Mathf.Lerp(start, xTarg, i / 0.15f);
				this.Dropper.transform.localPosition = pos;
				yield return wait;
			}
			pos.x = xTarg;
			this.Dropper.transform.localPosition = pos;
			yield return dropWait;
			int id = step + 3;
			vPosition += vPositionDelta;
			VibrationManager.Vibrate(vPosition.x * 2.5f, vPosition.y * 2.5f, 0.2f, VibrationManager.VibrationFalloff.None, this.dropSounds.Get(), false);
			yield return this.DropTube(id);
			num = step + 1;
		}
		for (float xTarg = 0f; xTarg < 0.15f; xTarg += Time.deltaTime)
		{
			pos.x = this.dropperX.Lerp(1f - xTarg / 0.15f);
			this.Dropper.transform.localPosition = pos;
			yield return wait;
		}
		pos.x = this.dropperX.min;
		this.Dropper.transform.localPosition = pos;
		yield break;
	}

	public enum States : byte
	{
		PrepareSample,
		Complete = 16,
		AwaitingStart = 32,
		Selection = 64,
		Processing = 128
	}
}
