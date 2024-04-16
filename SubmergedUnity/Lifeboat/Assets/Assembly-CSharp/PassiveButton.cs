using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class PassiveButton : PassiveUiElement
{
	public Button.ButtonClickedEvent OnClick = new Button.ButtonClickedEvent();

	public AudioClip ClickSound;

	public bool OnUp = true;

	public bool OnDown;

	public bool OnRepeat;

	public float RepeatDuration = 0.3f;

	private float repeatTimer;

	private bool checkedClickEvent;

	public override bool HandleUp
	{
		get
		{
			return this.OnUp;
		}
	}

	public override bool HandleDown
	{
		get
		{
			return this.OnDown;
		}
	}

	public override bool HandleRepeat
	{
		get
		{
			return this.OnRepeat;
		}
	}

	private void OnEnable()
	{
		if (!this.checkedClickEvent)
		{
			this.checkedClickEvent = true;
			int persistentEventCount = this.OnClick.GetPersistentEventCount();
			Object component = base.GetComponent<CloseButtonConsoleBehaviour>();
			string text = base.gameObject.name.ToLower();
			if (!component)
			{
				for (int i = 0; i < persistentEventCount; i++)
				{
					string persistentMethodName = this.OnClick.GetPersistentMethodName(i);
					if (text.Contains("close") && persistentMethodName.ToLower().Contains("close"))
					{
						base.gameObject.AddComponent<CloseButtonConsoleBehaviour>();
						return;
					}
				}
			}
		}
	}

	public override void ReceiveClickDown()
	{
		if (this.ClickSound)
		{
			SoundManager.Instance.PlaySound(this.ClickSound, false, 1f);
		}
		this.OnClick.Invoke();
	}

	public override void ReceiveRepeatDown()
	{
		this.repeatTimer += Time.deltaTime;
		if (this.repeatTimer < this.RepeatDuration)
		{
			return;
		}
		this.repeatTimer = 0f;
		if (this.ClickSound)
		{
			SoundManager.Instance.PlaySound(this.ClickSound, false, 1f);
		}
		this.OnClick.Invoke();
	}

	public override void ReceiveClickUp()
	{
		this.ReceiveClickDown();
	}
}
