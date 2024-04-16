using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MeetingIntroAnimation : MonoBehaviour
{
	public Transform VoteButtonParent;

	public Transform OverlayParent;

	public Transform DeadParent;

	public TextMeshPro DeadBodiesText;

	public SpriteRenderer BloodSplat;

	public Vector3 VoteButtonParentPos = new Vector3(0.25f, 0f, 0f);

	public Vector3 ReporterPos;

	public SpriteRenderer background;

	public AnimationCurve SlamCurve;

	public PlayerVoteArea VoteAreaPrefab;

	public AudioClip PlayerDeadSound;

	private List<PlayerVoteArea> deadCards = new List<PlayerVoteArea>();

	public SpriteRenderer[] OutsideMasks;

	public void Start()
	{
		this.VoteButtonParent.localPosition = Vector3.right * 20f;
		this.DeadParent.localPosition = Vector3.right * 20f;
		SpriteRenderer[] outsideMasks = this.OutsideMasks;
		for (int i = 0; i < outsideMasks.Length; i++)
		{
			outsideMasks[i].material.SetInt("_MaskLayer", 254);
		}
	}

	public void Init(GameData.PlayerInfo reporter, GameData.PlayerInfo[] deadBodies)
	{
		PlayerVoteArea playerVoteArea = UnityEngine.Object.Instantiate<PlayerVoteArea>(this.VoteAreaPrefab, this.OverlayParent);
		playerVoteArea.transform.localPosition = this.ReporterPos;
		playerVoteArea.SetMaskLayer(0);
		playerVoteArea.SetCosmetics(reporter);
		playerVoteArea.SetDead(true, false);
		playerVoteArea.NameText.text = reporter.PlayerName;
		float num = this.background.size.x / 2f;
		float num2 = this.VoteAreaPrefab.Background.bounds.size.y + 0.2f;
		float x = this.VoteAreaPrefab.Background.bounds.extents.x;
		int num3 = Mathf.CeilToInt((float)deadBodies.Length / 3f);
		for (int i = 0; i < deadBodies.Length; i += 3)
		{
			float num4 = (float)(num3 / 2 - i / 3) * num2;
			int num5 = Mathf.Min(deadBodies.Length - i, 3);
			for (int j = 0; j < num5; j++)
			{
				int num6 = i + j;
				float num7 = FloatRange.SpreadEvenly(-num - x, num + x, j, num5);
				PlayerVoteArea playerVoteArea2 = UnityEngine.Object.Instantiate<PlayerVoteArea>(this.VoteAreaPrefab, this.DeadParent);
				playerVoteArea2.transform.localPosition = new Vector3(num7, num4, 0f);
				playerVoteArea2.SetMaskLayer(num6);
				playerVoteArea2.SetCosmetics(deadBodies[num6]);
				playerVoteArea2.SetDead(false, true);
				playerVoteArea2.NameText.text = deadBodies[num6].PlayerName;
				this.deadCards.Add(playerVoteArea2);
			}
		}
		if (deadBodies.Length == 0)
		{
			this.DeadBodiesText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NoDeadBodiesFound, Array.Empty<object>());
			this.DeadBodiesText.transform.localPosition = Vector3.zero;
			this.BloodSplat.enabled = false;
		}
		this.DeadBodiesText.renderer.material.SetInt("_Stencil", 0);
		this.DeadBodiesText.renderer.material.SetInt("_StencilComp", 3);
	}

	public IEnumerator CoRun()
	{
		base.gameObject.SetActive(true);
		foreach (PlayerVoteArea playerVoteArea in this.deadCards)
		{
			base.StartCoroutine(playerVoteArea.CoAnimateOverlay());
		}
		this.DeadParent.localPosition = Vector3.zero;
		yield return Effects.Lerp(0.2f, delegate(float t)
		{
			this.DeadParent.transform.localScale = Vector3.one * Mathf.LerpUnclamped(8f, 1f, this.SlamCurve.Evaluate(t));
		});
		if (this.deadCards.Count > 0)
		{
			SoundManager.Instance.PlaySound(this.PlayerDeadSound, false, 1f);
		}
		yield return Effects.Wait(3f);
		yield return Effects.All(new IEnumerator[]
		{
			Effects.Slide2D(this.VoteButtonParent, Vector2.right * 20f, this.VoteButtonParentPos, 0.75f),
			Effects.Slide2D(this.OverlayParent, Vector2.zero, Vector2.left * 20f, 0.75f)
		});
		base.gameObject.SetActive(false);
		yield break;
	}
}
