using System;
using System.Collections;
using System.Linq;
using Rewired;
using UnityEngine;

public class EmptyGarbageMinigame : Minigame
{
	private const float GrinderVolume = 0.8f;

	public FloatRange HandleRange = new FloatRange(-0.65f, 0.65f);

	public Vector2Range SpawnRange;

	public Collider2D Blocker;

	public AreaEffector2D Popper;

	public Collider2D Handle;

	public SpriteRenderer Bars;

	private Controller controller = new Controller();

	private bool finished;

	public int NumObjects = 15;

	private SpriteRenderer[] Objects;

	public SpriteRenderer[] GarbagePrefabs;

	public SpriteRenderer[] LeafPrefabs;

	public SpriteRenderer[] SpecialObjectPrefabs;

	public AudioClip LeverDown;

	public AudioClip LeverUp;

	public AudioClip GrinderStart;

	public AudioClip GrinderLoop;

	public AudioClip GrinderEnd;

	private TouchpadBehavior touchpad;

	private bool hadInput;

	private float leverInput;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		int i = 0;
		this.Objects = new SpriteRenderer[this.NumObjects];
		RandomFill<SpriteRenderer> randomFill = new RandomFill<SpriteRenderer>();
		if (this.MyNormTask.StartAt == SystemTypes.LifeSupp)
		{
			randomFill.Set(this.GarbagePrefabs.Union(this.LeafPrefabs));
		}
		else
		{
			NormalPlayerTask myNormTask = this.MyNormTask;
			if (myNormTask != null && myNormTask.taskStep == 0)
			{
				if (this.MyNormTask.TaskType == TaskTypes.EmptyChute)
				{
					randomFill.Set(this.GarbagePrefabs);
				}
				else
				{
					randomFill.Set(this.LeafPrefabs);
				}
			}
			else
			{
				randomFill.Set(this.GarbagePrefabs.Union(this.LeafPrefabs));
				while (i < this.SpecialObjectPrefabs.Length)
				{
					SpriteRenderer spriteRenderer = this.Objects[i] = UnityEngine.Object.Instantiate<SpriteRenderer>(this.SpecialObjectPrefabs[i]);
					spriteRenderer.transform.SetParent(base.transform);
					spriteRenderer.transform.localPosition = this.SpawnRange.Next();
					i++;
				}
			}
		}
		while (i < this.Objects.Length)
		{
			SpriteRenderer spriteRenderer2 = this.Objects[i] = UnityEngine.Object.Instantiate<SpriteRenderer>(randomFill.Get());
			spriteRenderer2.transform.SetParent(base.transform);
			Vector3 vector = this.SpawnRange.Next();
			vector.z = FloatRange.Next(-0.5f, 0.5f);
			spriteRenderer2.transform.localPosition = vector;
			spriteRenderer2.color = Color.Lerp(Color.white, Color.black, (vector.z + 0.5f) * 0.7f);
			i++;
		}
		this.touchpad = base.GetComponent<TouchpadBehavior>();
		base.SetupInput(true);
	}

	public void Update()
	{
		if (this.amClosing != Minigame.CloseState.None)
		{
			return;
		}
		this.controller.Update();
		Vector3 localPosition = this.Handle.transform.localPosition;
		float num = this.HandleRange.ReverseLerp(localPosition.y);
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			if (!this.finished)
			{
				Player player = ReInput.players.GetPlayer(0);
				if (this.touchpad.IsTouching())
				{
					this.leverInput = -this.touchpad.GetTouchVector().y;
				}
				else
				{
					this.leverInput = Mathf.Clamp01(-player.GetAxis(17));
				}
				localPosition.y = this.HandleRange.Lerp(1f - this.leverInput);
				num = this.HandleRange.ReverseLerp(localPosition.y);
				if (this.leverInput >= 0.01f)
				{
					this.hadInput = true;
					if (num <= 0.5f && this.Blocker.enabled)
					{
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(this.LeverDown, false, 1f);
							SoundManager.Instance.PlaySound(this.GrinderStart, false, 0.8f);
							SoundManager.Instance.StopSound(this.GrinderEnd);
							SoundManager.Instance.StopSound(this.GrinderLoop);
						}
						this.Blocker.enabled = false;
						base.StopAllCoroutines();
						base.StartCoroutine(this.PopObjects());
						base.StartCoroutine(this.AnimateObjects());
					}
				}
				else
				{
					if (this.hadInput)
					{
						if (!this.Blocker.enabled)
						{
							this.Blocker.enabled = true;
							if (Constants.ShouldPlaySfx())
							{
								SoundManager.Instance.PlaySound(this.LeverUp, false, 1f);
								SoundManager.Instance.StopSound(this.GrinderStart);
								SoundManager.Instance.StopSound(this.GrinderLoop);
								SoundManager.Instance.PlaySound(this.GrinderEnd, false, 0.8f);
							}
						}
						if (!this.finished)
						{
							if (this.Objects.All((SpriteRenderer o) => !o))
							{
								this.finished = true;
								this.MyNormTask.NextStep();
								base.StartCoroutine(base.CoStartClose(0.75f));
							}
						}
					}
					this.hadInput = false;
				}
			}
		}
		else
		{
			switch (this.controller.CheckDrag(this.Handle))
			{
			case DragState.NoTouch:
				localPosition.y = Mathf.Lerp(localPosition.y, this.HandleRange.max, num + Time.deltaTime * 15f);
				break;
			case DragState.Dragging:
				if (!this.finished)
				{
					if (num > 0.5f)
					{
						Vector2 vector = this.controller.DragPosition - (Vector2) base.transform.position;
						float num2 = this.HandleRange.ReverseLerp(this.HandleRange.Clamp(vector.y));
						localPosition.y = this.HandleRange.Lerp(num2 / 2f + 0.5f);
					}
					else
					{
						localPosition.y = Mathf.Lerp(localPosition.y, this.HandleRange.min, num + Time.deltaTime * 15f);
						if (this.Blocker.enabled)
						{
							if (Constants.ShouldPlaySfx())
							{
								SoundManager.Instance.PlaySound(this.LeverDown, false, 1f);
								SoundManager.Instance.PlaySound(this.GrinderStart, false, 0.8f);
								SoundManager.Instance.StopSound(this.GrinderEnd);
								SoundManager.Instance.StopSound(this.GrinderLoop);
							}
							this.Blocker.enabled = false;
							base.StopAllCoroutines();
							base.StartCoroutine(this.PopObjects());
							base.StartCoroutine(this.AnimateObjects());
						}
					}
				}
				break;
			case DragState.Released:
				if (!this.Blocker.enabled)
				{
					this.Blocker.enabled = true;
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(this.LeverUp, false, 1f);
						SoundManager.Instance.StopSound(this.GrinderStart);
						SoundManager.Instance.StopSound(this.GrinderLoop);
						SoundManager.Instance.PlaySound(this.GrinderEnd, false, 0.8f);
					}
				}
				if (!this.finished)
				{
					if (this.Objects.All((SpriteRenderer o) => !o))
					{
						this.finished = true;
						this.MyNormTask.NextStep();
						base.StartCoroutine(base.CoStartClose(0.75f));
					}
				}
				break;
			}
		}
		if (Constants.ShouldPlaySfx() && !this.Blocker.enabled && !SoundManager.Instance.SoundIsPlaying(this.GrinderStart))
		{
			SoundManager.Instance.PlaySound(this.GrinderLoop, true, 0.8f);
		}
		this.Handle.transform.localPosition = localPosition;
		Vector3 localScale = this.Bars.transform.localScale;
		localScale.y = this.HandleRange.ChangeRange(localPosition.y, -1f, 1f);
		this.Bars.transform.localScale = localScale;
	}

	private IEnumerator PopObjects()
	{
		this.Popper.enabled = true;
		yield return new WaitForSeconds(0.05f);
		this.Popper.enabled = false;
		yield break;
	}

	private IEnumerator AnimateObjects()
	{
		Vector3 pos = base.transform.localPosition;
		for (float t = 3f; t > 0f; t -= Time.deltaTime)
		{
			float num = t / 3f;
			float num2 = num * 0.1f * 3f;
			VibrationManager.Vibrate(num2, num2, 0.01f, VibrationManager.VibrationFalloff.None, null, false);
			base.transform.localPosition = pos + (Vector3) Vector2Range.NextEdge() * num * 0.1f;
			yield return null;
		}
		yield break;
	}

	public override void Close()
	{
		SoundManager.Instance.StopSound(this.GrinderStart);
		SoundManager.Instance.StopSound(this.GrinderLoop);
		SoundManager.Instance.StopSound(this.GrinderEnd);
		if (this.MyNormTask && this.MyNormTask.IsComplete)
		{
			ShipStatus.Instance.OpenHatch();
			PlayerControl.LocalPlayer.RpcPlayAnimation((byte)this.MyTask.TaskType);
		}
		base.Close();
	}
}
