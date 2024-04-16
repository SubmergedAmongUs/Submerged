using System;
using System.Collections;
using System.Collections.Generic;
using Hazel;
using InnerNet;
using TMPro;
using UnityEngine;

public class PlayerVoteArea : MonoBehaviour
{
	public byte TargetPlayerId;

	public const byte HasNotVoted = 255;

	public const byte MissedVote = 254;

	public const byte SkippedVote = 253;

	public const byte DeadVote = 252;

	public GameObject Buttons;

	public UiElement ConfirmButton;

	public UiElement CancelButton;

	public UiElement PlayerButton;

	public PoolablePlayer PlayerIcon;

	public SpriteRenderer Background;

	public SpriteRenderer Flag;

	public SpriteRenderer Megaphone;

	public SpriteRenderer Overlay;

	public SpriteRenderer XMark;

	public TextMeshPro NameText;

	public TextMeshPro skipVoteText;

	public PlatformIdentifierIcon PlatformIcon;

	public bool AnimateButtonsFromLeft;

	public bool AmDead;

	public bool DidReport;

	public byte VotedFor = byte.MaxValue;

	public bool voteComplete;

	public bool resultsShowing;

	public MeetingHud Parent { get; set; }

	public bool DidVote
	{
		get
		{
			return this.VotedFor != byte.MaxValue;
		}
	}

	public void Start()
	{
		this.Buttons.SetActive(false);
	}

	public void SetTargetPlayerId(byte targetId)
	{
		this.TargetPlayerId = targetId;
		if (this.PlayerIcon)
		{
			this.SetMaskLayer((int)targetId);
		}
		if (this.PlatformIcon)
		{
			PlayerControl playerControl = PlayerControl.AllPlayerControls.Find((PlayerControl pc) => pc.PlayerId == targetId);
			if (playerControl)
			{
				ClientData client = AmongUsClient.Instance.GetClient(playerControl.OwnerId);
				if (client != null && client.platformID != (RuntimePlatform) (-1))
				{
					this.PlatformIcon.SetIcon(client.platformID);
				}
			}
		}
	}

	public void SetMaskLayer(int maskLayer)
	{
		SpriteRenderer[] componentsInChildren = base.GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].material.SetInt("_MaskLayer", maskLayer + 2);
		}
		this.NameText.renderer.material.SetInt("_Stencil", maskLayer + 2);
		this.NameText.renderer.material.SetInt("_StencilComp", 3);
	}

	public void SetDead(bool didReport, bool isDead)
	{
		this.AmDead = isDead;
		this.DidReport = didReport;
		this.Megaphone.enabled = didReport;
		this.Overlay.gameObject.SetActive(isDead);
		this.XMark.gameObject.SetActive(isDead);
	}

	public void SetDisabled()
	{
		if (this.AmDead)
		{
			return;
		}
		if (this.Overlay)
		{
			this.Overlay.gameObject.SetActive(true);
			this.XMark.gameObject.SetActive(false);
			return;
		}
		base.gameObject.SetActive(false);
	}

	public void SetEnabled()
	{
		if (this.AmDead)
		{
			return;
		}
		if (this.Overlay)
		{
			this.Overlay.gameObject.SetActive(false);
			return;
		}
		base.gameObject.SetActive(true);
	}

	public void UpdateOverlay()
	{
		this.Overlay.gameObject.SetActive(this.AmDead);
		if (this.AmDead)
		{
			this.Overlay.color = Color.white;
			this.XMark.transform.localScale = Vector3.one;
			return;
		}
		if (this.DidReport)
		{
			this.Megaphone.transform.localEulerAngles = Vector3.zero;
			this.Megaphone.transform.localScale = Vector3.one;
		}
	}

	public IEnumerator CoAnimateOverlay()
	{
		if (this.DidReport)
		{
			float duration = 1f;
			for (float time = 0f; time < duration; time += Time.deltaTime)
			{
				float num = time / duration;
				float num2 = PlayerVoteArea.TriangleWave(num * 3f) * 2f - 1f;
				this.Megaphone.transform.localEulerAngles = new Vector3(0f, 0f, num2 * 30f);
				num2 = Mathf.Lerp(0.7f, 1.2f, PlayerVoteArea.TriangleWave(num * 2f));
				this.Megaphone.transform.localScale = new Vector3(num2, num2, num2);
				yield return null;
			}
			this.Megaphone.transform.localEulerAngles = Vector3.zero;
			this.Megaphone.transform.localScale = Vector3.one;
		}
		yield break;
	}

	private static float TriangleWave(float t)
	{
		t -= (float)((int)t);
		if (t < 0.5f)
		{
			return t * 2f;
		}
		return 1f - (t - 0.5f) * 2f;
	}

	internal void SetVote(byte suspectIdx)
	{
		this.VotedFor = suspectIdx;
		this.Flag.enabled = true;
	}

	public void UnsetVote()
	{
		this.Flag.enabled = false;
		this.VotedFor = byte.MaxValue;
	}

	public void ClearButtons()
	{
		base.StopAllCoroutines();
		this.Buttons.SetActive(false);
	}

	public void ClearForResults()
	{
		this.resultsShowing = true;
		this.Flag.enabled = false;
	}

	public void VoteForMe()
	{
		if (!this.voteComplete)
		{
			this.Parent.Confirm(this.TargetPlayerId);
			ControllerManager.Instance.CloseOverlayMenu(base.name);
		}
	}

	public void Select()
	{
		if (PlayerControl.LocalPlayer.Data.IsDead)
		{
			return;
		}
		if (this.AmDead)
		{
			return;
		}
		if (!this.Parent)
		{
			return;
		}
		if (!this.voteComplete && this.Parent.Select((int)this.TargetPlayerId))
		{
			this.Buttons.SetActive(true);
			float startPos = this.AnimateButtonsFromLeft ? 0.2f : 1.95f;
			base.StartCoroutine(Effects.All(new IEnumerator[]
			{
				Effects.Lerp(0.25f, delegate(float t)
				{
					this.CancelButton.transform.localPosition = Vector2.Lerp(Vector2.right * startPos, Vector2.right * 1.3f, Effects.ExpOut(t));
				}),
				Effects.Lerp(0.35f, delegate(float t)
				{
					this.ConfirmButton.transform.localPosition = Vector2.Lerp(Vector2.right * startPos, Vector2.right * 0.65f, Effects.ExpOut(t));
				})
			}));
			List<UiElement> selectableElements = new List<UiElement>
			{
				this.CancelButton,
				this.ConfirmButton
			};
			ControllerManager.Instance.OpenOverlayMenu(base.name, this.CancelButton, this.ConfirmButton, selectableElements, false);
		}
	}

	public void Cancel()
	{
		this.ClearButtons();
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	public void Serialize(MessageWriter writer)
	{
		writer.Write(this.VotedFor);
		writer.Write(this.DidReport);
	}

	public void Deserialize(MessageReader reader)
	{
		this.VotedFor = reader.ReadByte();
		this.DidReport = reader.ReadBoolean();
		this.Flag.enabled = (this.DidVote && !this.resultsShowing);
		this.Overlay.gameObject.SetActive(this.AmDead);
		this.Megaphone.enabled = this.DidReport;
	}

	public void SetCosmetics(GameData.PlayerInfo playerInfo)
	{
		PlayerControl.SetPlayerMaterialColors(playerInfo.ColorId, this.PlayerIcon.Body);
		this.PlayerIcon.HatSlot.SetHat(playerInfo.HatId, playerInfo.ColorId);
		this.PlayerIcon.SetSkin(playerInfo.SkinId);
	}

	private void OnDestroy()
	{
		if (this.Buttons.activeSelf)
		{
			ControllerManager.Instance.CloseOverlayMenu(base.name);
		}
	}
}
