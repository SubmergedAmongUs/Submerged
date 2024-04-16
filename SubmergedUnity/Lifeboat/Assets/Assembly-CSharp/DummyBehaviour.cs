using System;
using System.Collections;
using UnityEngine;

public class DummyBehaviour : MonoBehaviour
{
	private PlayerControl myPlayer;

	private FloatRange voteTime = new FloatRange(3f, 8f);

	private bool voted;

	public void Start()
	{
		this.myPlayer = base.GetComponent<PlayerControl>();
	}

	public void Update()
	{
		GameData.PlayerInfo data = this.myPlayer.Data;
		if (data == null || data.IsDead)
		{
			return;
		}
		if (MeetingHud.Instance)
		{
			if (!this.voted)
			{
				this.voted = true;
				base.StartCoroutine(this.DoVote());
				return;
			}
		}
		else
		{
			this.voted = false;
		}
	}

	private IEnumerator DoVote()
	{
		yield return new WaitForSeconds(this.voteTime.Next());
		byte suspectIdx = 253;
		for (int i = 0; i < 100; i++)
		{
			int num = IntRange.Next(-1, GameData.Instance.PlayerCount);
			if (num < 0)
			{
				break;
			}
			GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[num];
			if (!playerInfo.IsDead)
			{
				suspectIdx = playerInfo.PlayerId;
				break;
			}
		}
		MeetingHud.Instance.CmdCastVote(this.myPlayer.PlayerId, suspectIdx);
		yield break;
	}
}
