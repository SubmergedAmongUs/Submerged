using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class WeaponsMinigame : Minigame
{
	public FloatRange XSpan = new FloatRange(-1.15f, 1.15f);

	public FloatRange YSpan = new FloatRange(-1.15f, 1.15f);

	public FloatRange TimeToSpawn;

	public ObjectPoolBehavior asteroidPool;

	public TextMeshPro ScoreText;

	public SpriteRenderer TargetReticle;

	public LineRenderer TargetLines;

	private Vector3 TargetCenter;

	public Collider2D BackgroundCol;

	public SpriteRenderer Background;

	public Controller myController = new Controller();

	private float Timer;

	public AudioClip ShootSound;

	public AudioClip[] ExplodeSounds;

	public override void Begin(PlayerTask task)
	{
		base.SetupInput(false);
		base.Begin(task);
		this.ScoreText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.AstDestroyed, new object[]
		{
			this.MyNormTask.taskStep
		});
		this.TimeToSpawn.Next();
	}

	protected override IEnumerator CoAnimateOpen()
	{
		for (float timer = 0f; timer < 0.1f; timer += Time.deltaTime)
		{
			float num = timer / 0.1f;
			base.transform.localScale = new Vector3(num, 0.1f, num);
			yield return null;
		}
		for (float timer = 0.010000001f; timer < 0.1f; timer += Time.deltaTime)
		{
			float num2 = timer / 0.1f;
			base.transform.localScale = new Vector3(1f, num2, 1f);
			yield return null;
		}
		base.transform.localScale = new Vector3(1f, 1f, 1f);
		yield break;
	}

	protected override IEnumerator CoDestroySelf()
	{
		for (float timer = 0.010000001f; timer < 0.1f; timer += Time.deltaTime)
		{
			float num = 1f - timer / 0.1f;
			base.transform.localScale = new Vector3(1f, num, 1f);
			yield return null;
		}
		for (float timer = 0f; timer < 0.1f; timer += Time.deltaTime)
		{
			float num2 = 1f - timer / 0.1f;
			base.transform.localScale = new Vector3(num2, 0.1f, num2);
			yield return null;
		}
		 UnityEngine.Object.Destroy(base.gameObject);
		yield break;
	}

	public void FixedUpdate()
	{
		this.Background.color = Color.Lerp(Palette.ClearWhite, Color.white, Mathf.Sin(Time.time * 3f) * 0.1f + 0.79999995f);
		if (this.MyNormTask && this.MyNormTask.IsComplete)
		{
			return;
		}
		this.Timer += Time.fixedDeltaTime;
		if (this.Timer >= this.TimeToSpawn.Last)
		{
			this.Timer = 0f;
			this.TimeToSpawn.Next();
			if (this.asteroidPool.InUse < this.MyNormTask.MaxStep - this.MyNormTask.TaskStep)
			{
				Asteroid ast = this.asteroidPool.Get<Asteroid>();
				ast.transform.localPosition = new Vector3(this.XSpan.max, this.YSpan.Next(), -1f);
				ast.TargetPosition = new Vector3(this.XSpan.min, this.YSpan.Next(), -1f);
				ast.GetComponent<ButtonBehavior>().OnClick.AddListener(delegate()
				{
					this.BreakApart(ast);
				});
			}
		}
		this.myController.Update();
		bool flag = this.myController.CheckHover(this.BackgroundCol);
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			if (flag)
			{
				Vector3 vector = (Vector3) this.myController.HoverPosition -  base.transform.position;
				vector.z = -2f;
				this.TargetReticle.transform.localPosition = vector;
				vector.z = 0f;
				this.TargetLines.SetPosition(1, vector);
			}
			else
			{
				Bounds bounds = this.BackgroundCol.bounds;
				Vector3 vector2 = this.myController.HoverPosition;
				vector2.x = Mathf.Clamp(vector2.x, bounds.min.x, bounds.max.x);
				vector2.y = Mathf.Clamp(vector2.y, bounds.min.y, bounds.max.y);
				VirtualCursor.instance.SetWorldPosition(vector2);
				vector2 -= base.transform.position;
				vector2.z = -2f;
				this.TargetReticle.transform.localPosition = vector2;
				vector2.z = 0f;
				this.TargetLines.SetPosition(1, vector2);
			}
		}
		if (this.myController.CheckDrag(this.BackgroundCol) == DragState.TouchStart)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.ShootSound, false, 1f);
			}
			Vector3 vector3 = (Vector3) this.myController.DragPosition - base.transform.position;
			vector3.z = -2f;
			this.TargetReticle.transform.localPosition = vector3;
			vector3.z = 0f;
			this.TargetLines.SetPosition(1, vector3);
			if (ShipStatus.Instance.WeaponsImage && !ShipStatus.Instance.WeaponsImage.IsPlaying())
			{
				PlayerControl.LocalPlayer.RpcPlayAnimation(6);
			}
		}
	}

	public void BreakApart(Asteroid ast)
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.ExplodeSounds.Random<AudioClip>(), false, 1f).pitch = FloatRange.Next(0.8f, 1.2f);
		}
		if (!this.MyNormTask.IsComplete)
		{
			base.StartCoroutine(ast.CoBreakApart());
			if (this.MyNormTask)
			{
				this.MyNormTask.NextStep();
				this.ScoreText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.AstDestroyed, new object[]
				{
					this.MyNormTask.taskStep
				});
			}
			if (this.MyNormTask && this.MyNormTask.IsComplete)
			{
				base.StartCoroutine(base.CoStartClose(0.75f));
				foreach (PoolableBehavior poolableBehavior in this.asteroidPool.activeChildren)
				{
					Asteroid asteroid = (Asteroid)poolableBehavior;
					if (!(asteroid == ast))
					{
						base.StartCoroutine(asteroid.CoBreakApart());
					}
				}
			}
		}
	}
}
