using System;
using PowerTools;
using Rewired;
using UnityEngine;

public class LeafMinigame : Minigame
{
	public LeafBehaviour LeafPrefab;

	public Vector2Range ValidArea;

	public SpriteAnim[] Arrows;

	public AnimationClip[] Inactive;

	public AnimationClip[] Active;

	public AnimationClip[] Complete;

	private Collider2D[] Leaves;

	public AudioClip[] LeaveSounds;

	public AudioClip[] SuckSounds;

	private Controller myController = new Controller();

	public Transform interactionCursor;

	public Transform interactionCursorCenterDot;

	private Collider2D[] overlapResults;

	private bool prevHadOverlaps;

	private bool prevRightStickInput;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		this.Leaves = new Collider2D[this.MyNormTask.MaxStep - this.MyNormTask.taskStep];
		for (int i = 0; i < this.Leaves.Length; i++)
		{
			LeafBehaviour leafBehaviour = UnityEngine.Object.Instantiate<LeafBehaviour>(this.LeafPrefab);
			leafBehaviour.transform.SetParent(base.transform);
			leafBehaviour.Parent = this;
			Vector3 localPosition = this.ValidArea.Next();
			localPosition.z = -1f;
			leafBehaviour.transform.localPosition = localPosition;
			this.Leaves[i] = leafBehaviour.GetComponent<Collider2D>();
		}
		this.overlapResults = new Collider2D[this.Leaves.Length * 3];
		base.SetupInput(true);
	}

	public void FixedUpdate()
	{
		this.myController.Update();
		if (Controller.currentTouchType != Controller.TouchType.Joystick)
		{
			for (int i = 0; i < this.Leaves.Length; i++)
			{
				Collider2D collider2D = this.Leaves[i];
				if (collider2D)
				{
					LeafBehaviour component = collider2D.GetComponent<LeafBehaviour>();
					switch (this.myController.CheckDrag(collider2D))
					{
					case DragState.TouchStart:
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(this.LeaveSounds.Random<AudioClip>(), false, 1f);
						}
						for (int j = 0; j < this.Arrows.Length; j++)
						{
							this.Arrows[j].Play(this.Active[j], 1f);
						}
						component.Held = true;
						break;
					case DragState.Dragging:
					{
						Vector2 vector = this.myController.DragPosition - component.body.position;
						component.body.velocity = vector.normalized * Mathf.Min(3f, vector.magnitude * 3f);
						break;
					}
					case DragState.Released:
						component.Held = false;
						for (int k = 0; k < this.Arrows.Length; k++)
						{
							this.Arrows[k].Play(this.Inactive[k], 1f);
							this.Arrows[k].GetComponent<SpriteRenderer>().sprite = null;
						}
						break;
					}
				}
			}
			return;
		}
		Player player = ReInput.players.GetPlayer(0);
		Vector2 vector2 = new Vector2(player.GetAxis(13), player.GetAxis(14));
		Vector2 normalized = new Vector2(player.GetAxis(16), player.GetAxis(17));
		if (normalized.sqrMagnitude > 1f)
		{
			normalized = normalized.normalized;
		}
		Vector3 localPosition = this.interactionCursor.localPosition;
		Vector3 localPosition2 = this.interactionCursorCenterDot.localPosition;
		localPosition.x += vector2.x * Time.deltaTime * 5f;
		localPosition.y += vector2.y * Time.deltaTime * 5f;
		localPosition.x = Mathf.Clamp(localPosition.x, this.ValidArea.min.x, this.ValidArea.max.x);
		localPosition.y = Mathf.Clamp(localPosition.y, this.ValidArea.min.y, this.ValidArea.max.y);
		localPosition2.x = normalized.x;
		localPosition2.y = normalized.y;
		this.interactionCursor.localPosition = localPosition;
		this.interactionCursorCenterDot.localPosition = localPosition2;
		int num = Physics2D.OverlapCircleNonAlloc(this.interactionCursor.position, this.interactionCursor.transform.localScale.x, this.overlapResults, LayerMask.GetMask(new string[]
		{
			"UI"
		}));
		int num2 = 0;
		bool flag = normalized.sqrMagnitude > 0.5f;
		bool flag2 = flag && !this.prevRightStickInput;
		for (int l = 0; l < num; l++)
		{
			LeafBehaviour component2 = this.overlapResults[l].GetComponent<LeafBehaviour>();
			if (component2)
			{
				num2++;
				if (flag2)
				{
					flag2 = false;
					component2.body.velocity = normalized.normalized * Mathf.Min(3f, normalized.magnitude * 3f);
					VibrationManager.Vibrate(0.15f, 0.15f, 0.15f, VibrationManager.VibrationFalloff.None, null, false);
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(this.LeaveSounds.Random<AudioClip>(), false, 1f);
					}
				}
			}
		}
		this.prevRightStickInput = flag;
		if (num2 > 0)
		{
			if (!this.prevHadOverlaps)
			{
				for (int m = 0; m < this.Arrows.Length; m++)
				{
					this.Arrows[m].Play(this.Active[m], 1f);
				}
			}
			this.prevHadOverlaps = true;
			return;
		}
		if (this.prevHadOverlaps)
		{
			for (int n = 0; n < this.Arrows.Length; n++)
			{
				this.Arrows[n].Play(this.Inactive[n], 1f);
				this.Arrows[n].GetComponent<SpriteRenderer>().sprite = null;
			}
		}
		this.prevHadOverlaps = false;
	}

	public void LeafDone(LeafBehaviour leaf)
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.SuckSounds.Random<AudioClip>(), false, 1f);
		}
		VibrationManager.Vibrate(0.15f, 0f, 0.25f, VibrationManager.VibrationFalloff.None, null, false);
		 UnityEngine.Object.Destroy(leaf.gameObject);
		if (this.MyNormTask)
		{
			this.MyNormTask.NextStep();
			if (this.MyNormTask.IsComplete)
			{
				for (int i = 0; i < this.Arrows.Length; i++)
				{
					this.Arrows[i].Play(this.Complete[i], 1f);
				}
				base.StartCoroutine(base.CoStartClose(0.75f));
			}
		}
	}
}
