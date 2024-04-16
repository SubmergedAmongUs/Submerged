using System;
using System.Collections;
using UnityEngine;

public abstract class Minigame : MonoBehaviour
{
	public static Minigame Instance;

	public const float Depth = -50f;

	public TransitionType TransType;

	protected PlayerTask MyTask;

	protected NormalPlayerTask MyNormTask;

	protected Minigame.CloseState amClosing;

	protected bool amOpening;

	public AudioClip OpenSound;

	public AudioClip CloseSound;

	[NonSerialized]
	public SpecialInputHandler inputHandler;

	public TaskTypes TaskType
	{
		get
		{
			return this.MyTask.TaskType;
		}
	}

	public global::Console Console { get; set; }

	protected int ConsoleId
	{
		get
		{
			if (!this.Console)
			{
				return 0;
			}
			return this.Console.ConsoleId;
		}
	}

	public virtual void Begin(PlayerTask task)
	{
		Minigame.Instance = this;
		this.MyTask = task;
		this.MyNormTask = (task as NormalPlayerTask);
		if (PlayerControl.LocalPlayer)
		{
			if (MapBehaviour.Instance)
			{
				MapBehaviour.Instance.Close();
			}
			PlayerControl.LocalPlayer.NetTransform.Halt();
		}
		base.StartCoroutine(this.CoAnimateOpen());
	}

	protected IEnumerator CoStartClose(float duration = 0.75f)
	{
		if (this.amClosing != Minigame.CloseState.None)
		{
			yield break;
		}
		this.amClosing = Minigame.CloseState.Waiting;
		yield return Effects.Wait(duration);
		this.Close();
		yield break;
	}

	[Obsolete("Don't use, I just don't want to reselect the close button event handlers", true)]
	public void Close(bool allowMovement)
	{
		this.Close();
	}

	public void ForceClose()
	{
		this.Close();
		 UnityEngine.Object.Destroy(base.gameObject);
	}

	public virtual void Close()
	{
		if (this.amClosing != Minigame.CloseState.Closing)
		{
			if (this.CloseSound && Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.CloseSound, false, 1f);
			}
			ConsoleJoystick.SetMode_Menu();
			if (PlayerControl.LocalPlayer)
			{
				PlayerControl.HideCursorTemporarily();
			}
			this.amClosing = Minigame.CloseState.Closing;
			base.StartCoroutine(this.CoDestroySelf());
			return;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	protected virtual IEnumerator CoAnimateOpen()
	{
		this.amOpening = true;
		if (this.OpenSound && Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.OpenSound, false, 1f);
		}
		float depth = base.transform.localPosition.z;
		switch (this.TransType)
		{
		case TransitionType.SlideBottom:
			for (float timer = 0f; timer < 0.25f; timer += Time.deltaTime)
			{
				float num = timer / 0.25f;
				base.transform.localPosition = new Vector3(0f, Mathf.SmoothStep(-8f, 0f, num), depth);
				yield return null;
			}
			base.transform.localPosition = new Vector3(0f, 0f, depth);
			break;
		case TransitionType.Alpha:
		{
			SpriteRenderer[] rends = base.GetComponentsInChildren<SpriteRenderer>();
			for (float timer = 0f; timer < 0.25f; timer += Time.deltaTime)
			{
				float num2 = timer / 0.25f;
				for (int i = 0; i < rends.Length; i++)
				{
					rends[i].color = Color.Lerp(Palette.ClearWhite, Color.white, num2);
				}
				yield return null;
			}
			for (int j = 0; j < rends.Length; j++)
			{
				rends[j].color = Color.white;
			}
			rends = null;
			break;
		}
		case TransitionType.None:
			base.transform.localPosition = new Vector3(0f, 0f, depth);
			break;
		}
		this.amOpening = false;
		yield break;
	}

	protected virtual IEnumerator CoDestroySelf()
	{
		switch (this.TransType)
		{
		case TransitionType.SlideBottom:
			for (float timer = 0f; timer < 0.25f; timer += Time.deltaTime)
			{
				float num = timer / 0.25f;
				base.transform.localPosition = new Vector3(0f, Mathf.SmoothStep(0f, -8f, num), -50f);
				yield return null;
			}
			break;
		case TransitionType.Alpha:
		{
			SpriteRenderer[] rends = base.GetComponentsInChildren<SpriteRenderer>();
			for (float timer = 0f; timer < 0.25f; timer += Time.deltaTime)
			{
				float num2 = timer / 0.25f;
				for (int i = 0; i < rends.Length; i++)
				{
					rends[i].color = Color.Lerp(Color.white, Palette.ClearWhite, num2);
				}
				yield return null;
			}
			rends = null;
			break;
		}
		}
		 UnityEngine.Object.Destroy(base.gameObject);
		yield break;
	}

	public void SetupInput(bool disableCursor)
	{
		this.inputHandler = base.GetComponentInChildren<SpecialInputHandler>(true);
		if (!this.inputHandler)
		{
			this.inputHandler = base.gameObject.AddComponent<SpecialInputHandler>();
		}
		if (disableCursor)
		{
			this.inputHandler.disableVirtualCursor = true;
		}
		ConsoleJoystick.SetMode_Task();
	}

	protected enum CloseState
	{
		None,
		Waiting,
		Closing
	}
}
