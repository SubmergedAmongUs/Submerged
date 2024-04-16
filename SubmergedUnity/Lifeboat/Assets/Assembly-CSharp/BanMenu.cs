using System;
using System.Collections.Generic;
using System.Linq;
using InnerNet;
using UnityEngine;
using Object = UnityEngine.Object;

public class BanMenu : MonoBehaviour
{
	public BanButton BanButtonPrefab;

	public SpriteRenderer Background;

	public SpriteRenderer BanButton;

	public SpriteRenderer KickButton;

	public SpriteRenderer ReportButton;

	public GameObject hotkeyGlyph;

	public GameObject ContentParent;

	public ReportReasonScreen ReportReason;

	public int selectedClientId = -1;

	public List<BanButton> allButtons = new List<BanButton>();

	private List<ClientData> recentClients = new List<ClientData>();

	private const float BackgroundTailWidth = 0.4f;

	private const float BorderPadding = 0.15f;

	private const float HeightPerButton = 0.4f;

	private const float WidthPerButton = 3f;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	public void SetVisible(bool show)
	{
		bool flag;
		if (PlayerControl.LocalPlayer)
		{
			GameData.PlayerInfo data = PlayerControl.LocalPlayer.Data;
			flag = (data != null && !data.IsDead);
		}
		else
		{
			flag = false;
		}
		bool flag2 = flag;
		show &= (PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data != null);
		this.BanButton.gameObject.SetActive(AmongUsClient.Instance.CanBan() && flag2);
		this.KickButton.gameObject.SetActive(AmongUsClient.Instance.CanKick() && flag2);
		base.GetComponent<SpriteRenderer>().enabled = show;
		base.GetComponent<PassiveButton>().enabled = show;
		this.hotkeyGlyph.SetActive(show);
	}

	private void Update()
	{
		if (!AmongUsClient.Instance)
		{
			return;
		}
		AmongUsClient.Instance.GetRecentClients(this.recentClients);
		using (List<ClientData>.Enumerator enumerator = this.recentClients.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				ClientData client = enumerator.Current;
				if (client.Id != AmongUsClient.Instance.ClientId)
				{
					try
					{
						int[] source;
						if (VoteBanSystem.Instance.HasMyVote(client.Id) && VoteBanSystem.Instance.Votes.TryGetValue(client.Id, out source))
						{
							int num = source.Count((int c) => c != 0);
							BanButton banButton = this.allButtons.FirstOrDefault((BanButton b) => b.TargetClientId == client.Id);
							if (banButton && banButton.numVotes != num)
							{
								banButton.SetVotes(num);
							}
						}
					}
					catch
					{
						break;
					}
				}
			}
		}
	}

	public void Show()
	{
		if (this.ContentParent.activeSelf)
		{
			this.Hide();
			return;
		}
		this.selectedClientId = -1;
		this.KickButton.GetComponent<ButtonRolloverHandler>().SetDisabledColors();
		this.BanButton.GetComponent<ButtonRolloverHandler>().SetDisabledColors();
		this.ReportButton.GetComponent<ButtonRolloverHandler>().SetDisabledColors();
		this.ContentParent.SetActive(true);
		if (AmongUsClient.Instance)
		{
			List<ClientData> list = new List<ClientData>();
			AmongUsClient.Instance.GetRecentClients(list);
			foreach (ClientData clientData in list)
			{
				if (clientData.Id != AmongUsClient.Instance.ClientId)
				{
					string text = clientData.PlayerName ?? "???";
					if (!clientData.Character)
					{
						text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.UserLeftGame, new object[]
						{
							text
						});
					}
					BanButton banButton = UnityEngine.Object.Instantiate<BanButton>(this.BanButtonPrefab, this.Background.transform);
					banButton.Parent = this;
					banButton.NameText.text = text;
					banButton.TargetClientId = clientData.Id;
					banButton.Unselect();
					this.allButtons.Add(banButton);
					this.ControllerSelectable.AddUnique(banButton.GetComponent<UiElement>());
				}
			}
		}
		this.AlignAllButtons();
		ConsoleJoystick.SetMode_Menu();
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton, this.DefaultButtonSelected, this.ControllerSelectable, false);
	}

	[ContextMenu("AlignAllButtons")]
	private void AlignAllButtons()
	{
		int num = (this.allButtons.Count > 10) ? 2 : 1;
		int num2 = Mathf.CeilToInt((float)(this.allButtons.Count / num)) + 1;
		float num3 = 3.2f + 1.1f * (float)(num - 1) + 0.4f;
		float num4 = 0.3f + (float)num2 * 0.4f;
		this.Background.size = new Vector2(num3, num4);
		this.Background.GetComponent<BoxCollider2D>().size = new Vector2(num3, num4);
		this.Background.transform.localPosition = new Vector3(-0.4f, -num4 / 2f + 0.15f, 0.1f);
		for (int i = 0; i < this.allButtons.Count; i++)
		{
			int i2 = i % num;
			int num5 = i / num;
			float num6 = FloatRange.SpreadEvenly(-num3 / 2f - 0.6f, num3 / 2f + 0.6f, i2, num) - 0.2f;
			float num7 = num4 / 2f - 0.15f - 0.4f * (float)num5 - 0.1f;
			this.allButtons[i].transform.localPosition = new Vector3(num6, num7, -1f);
		}
		float num8 = -(num4 / 2f) + 0.2f;
		this.KickButton.transform.localPosition = new Vector3(num3 / 2f - 0.4f - 2.6f, num8, -0.1f);
		this.BanButton.transform.localPosition = new Vector3(num3 / 2f - 0.4f - 1.6f, num8, -0.1f);
		this.ReportButton.transform.localPosition = new Vector3(num3 / 2f - 0.4f - 0.6f, num8, -0.1f);
	}

	public void Hide()
	{
		this.selectedClientId = -1;
		this.ContentParent.SetActive(false);
		for (int i = 0; i < this.allButtons.Count; i++)
		{
			 UnityEngine.Object.Destroy(this.allButtons[i].gameObject);
		}
		this.allButtons.Clear();
		ConsoleJoystick.SetMode_QuickChat();
		ControllerManager.Instance.CloseOverlayMenu(base.name);
		this.ReportReason.Hide();
	}

	public void Select(int clientId)
	{
		if (VoteBanSystem.Instance.HasMyVote(clientId))
		{
			return;
		}
		this.selectedClientId = clientId;
		ClientData recentClient = AmongUsClient.Instance.GetRecentClient(clientId);
		if (recentClient == null)
		{
			this.KickButton.GetComponent<ButtonRolloverHandler>().SetDisabledColors();
			this.BanButton.GetComponent<ButtonRolloverHandler>().SetDisabledColors();
			this.ReportButton.GetComponent<ButtonRolloverHandler>().SetDisabledColors();
		}
		else
		{
			if (recentClient.Character)
			{
				this.KickButton.GetComponent<ButtonRolloverHandler>().SetEnabledColors();
				this.BanButton.GetComponent<ButtonRolloverHandler>().SetEnabledColors();
			}
			else
			{
				this.KickButton.GetComponent<ButtonRolloverHandler>().SetDisabledColors();
				this.BanButton.GetComponent<ButtonRolloverHandler>().SetDisabledColors();
			}
			if (!recentClient.HasBeenReported)
			{
				this.ReportButton.GetComponent<ButtonRolloverHandler>().SetEnabledColors();
			}
			else
			{
				this.ReportButton.GetComponent<ButtonRolloverHandler>().SetDisabledColors();
			}
		}
		for (int i = 0; i < this.allButtons.Count; i++)
		{
			BanButton banButton = this.allButtons[i];
			if (banButton.TargetClientId != clientId)
			{
				banButton.Unselect();
			}
		}
	}

	public void Kick(bool ban)
	{
		if (this.selectedClientId >= 0)
		{
			if (AmongUsClient.Instance.CanBan())
			{
				AmongUsClient.Instance.KickPlayer(this.selectedClientId, ban);
				this.Hide();
			}
			else
			{
				VoteBanSystem.Instance.CmdAddVote(this.selectedClientId);
			}
			this.Select(-1);
		}
	}

	public void PickReportReason()
	{
		ClientData recentClient = AmongUsClient.Instance.GetRecentClient(this.selectedClientId);
		if (recentClient == null || recentClient.HasBeenReported)
		{
			return;
		}
		this.ReportReason.Show(recentClient.PlayerName ?? "???", recentClient.ColorId);
	}

	public void ReportPlayer(ReportReasons reason)
	{
		ClientData recentClient = AmongUsClient.Instance.GetRecentClient(this.selectedClientId);
		if (recentClient != null && !recentClient.HasBeenReported)
		{
			AmongUsClient.Instance.ReportPlayer(this.selectedClientId, reason);
			this.Hide();
		}
		this.Select(-1);
	}
}
