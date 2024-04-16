using System;
using System.Linq;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

public class VitalsMinigame : Minigame
{
	public VitalsPanel PanelPrefab;

	public TextMeshPro SabText;

	public float XStart = -0.8f;

	public float YStart = 2.15f;

	public float XOffset = 1.95f;

	public float YOffset = -0.65f;

	private VitalsPanel[] vitals;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		DeadBody[] source = Object.FindObjectsOfType<DeadBody>();
		this.vitals = new VitalsPanel[GameData.Instance.AllPlayers.Count];
		for (int i = 0; i < GameData.Instance.AllPlayers.Count; i++)
		{
			GameData.PlayerInfo player = GameData.Instance.AllPlayers[i];
			int num = i % 3;
			int num2 = i / 3;
			VitalsPanel vitalsPanel = UnityEngine.Object.Instantiate<VitalsPanel>(this.PanelPrefab, base.transform);
			vitalsPanel.transform.localPosition = new Vector3(this.XStart + (float)num * this.XOffset, this.YStart + (float)num2 * this.YOffset, -1f);
			vitalsPanel.SetPlayer(i, player);
			if (!source.Any((DeadBody b) => b.ParentId == player.PlayerId) && (player.Disconnected || player.IsDead))
			{
				vitalsPanel.SetDisconnected();
			}
			else if (player.IsDead)
			{
				vitalsPanel.SetDead();
			}
			else
			{
				vitalsPanel.SetAlive();
			}
			this.vitals[i] = vitalsPanel;
		}
		base.SetupInput(true);
	}

	private void Update()
	{
		if (this.SabText.isActiveAndEnabled && !PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			this.SabText.gameObject.SetActive(false);
			for (int i = 0; i < this.vitals.Length; i++)
			{
				this.vitals[i].gameObject.SetActive(true);
			}
		}
		else if (!this.SabText.isActiveAndEnabled && PlayerTask.PlayerHasTaskOfType<HudOverrideTask>(PlayerControl.LocalPlayer))
		{
			this.SabText.gameObject.SetActive(true);
			for (int j = 0; j < this.vitals.Length; j++)
			{
				this.vitals[j].gameObject.SetActive(false);
			}
		}
		for (int k = 0; k < this.vitals.Length; k++)
		{
			VitalsPanel vitalsPanel = this.vitals[k];
			if (!vitalsPanel.PlayerInfo.IsDead && vitalsPanel.PlayerInfo.Disconnected && !vitalsPanel.IsDiscon)
			{
				vitalsPanel.SetDisconnected();
			}
			else if (vitalsPanel.PlayerInfo.IsDead && !vitalsPanel.IsDead && !vitalsPanel.IsDiscon)
			{
				vitalsPanel.SetDead();
			}
		}
	}
}
