using System;
using TMPro;
using UnityEngine;

public class SecurityLogGame : Minigame
{
	private SecurityLogBehaviour Logger;

	public ObjectPoolBehavior EntryPool;

	public Scroller scroller;

	public float ScreenHeight = 4f;

	public float EntryHeight = 0.4f;

	public Sprite[] LocationBgs;

	public TextMeshPro SabText;

	public void Awake()
	{
		base.SetupInput(false);
		this.Logger = ShipStatus.Instance.GetComponent<SecurityLogBehaviour>();
		if (!PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			this.RefreshScreen();
			return;
		}
		this.SabText.gameObject.SetActive(true);
	}

	public void Update()
	{
		if (!PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			if (this.SabText.isActiveAndEnabled)
			{
				this.SabText.gameObject.SetActive(false);
				this.RefreshScreen();
				return;
			}
			if (this.Logger.HasNew)
			{
				this.Logger.HasNew = false;
				this.RefreshScreen();
				return;
			}
		}
		else if (!this.SabText.isActiveAndEnabled)
		{
			this.EntryPool.ReclaimAll();
			this.SabText.gameObject.SetActive(true);
		}
	}

	private void RefreshScreen()
	{
		this.EntryPool.ReclaimAll();
		int num = 0;
		for (int i = 0; i < this.Logger.LogEntries.Count; i++)
		{
			SecurityLogBehaviour.SecurityLogEntry securityLogEntry = this.Logger.LogEntries[i];
			LogEntryBubble logEntryBubble = this.EntryPool.Get<LogEntryBubble>();
			GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(securityLogEntry.PlayerId);
			if (playerById == null)
			{
				Debug.Log(string.Format("Couldn't find player {0} for log", securityLogEntry.PlayerId));
			}
			else
			{
				PlayerControl.SetPlayerMaterialColors(playerById.ColorId, logEntryBubble.HeadImage);
				string @string = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.LogNorth + (int)securityLogEntry.Location, Array.Empty<object>());
				string text = logEntryBubble.Text.text;
				logEntryBubble.Text.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SecLogEntry, new object[]
				{
					playerById.PlayerName,
					@string
				});
				if (!logEntryBubble.Text.text.Equals(text))
				{
					logEntryBubble.Text.ForceMeshUpdate(false, false);
				}
				logEntryBubble.Background.sprite = this.LocationBgs[(int)((byte)securityLogEntry.Location)];
				logEntryBubble.transform.localPosition = new Vector3(0f, (float)num * -this.EntryHeight, 0f);
				num++;
			}
		}
		float max = Mathf.Max(0f, (float)num * this.EntryHeight - this.ScreenHeight);
		float scrollPercY = this.scroller.GetScrollPercY();
		this.scroller.YBounds = new FloatRange(0f, max);
		if (scrollPercY > 0.95f)
		{
			this.scroller.ScrollPercentY(1f);
		}
	}
}
