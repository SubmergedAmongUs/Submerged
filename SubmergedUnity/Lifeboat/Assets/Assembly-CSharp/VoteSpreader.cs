using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoteSpreader : MonoBehaviour
{
	public List<SpriteRenderer> Votes = new List<SpriteRenderer>();

	public FloatRange VoteRange = new FloatRange(-0.5f, 1.15f);

	public int maxVotesBeforeSmoosh = 7;

	public float CounterY = -0.16f;

	public float adjustRate = 4f;

	private void Update()
	{
		int num = this.Votes.Count((SpriteRenderer v) => v.transform.localScale.magnitude > 0.0001f);
		for (int i = 0; i < num; i++)
		{
			SpriteRenderer spriteRenderer = this.Votes[i];
			Vector2 vector = new Vector3(0f, this.CounterY);
			vector.x = this.VoteRange.SpreadToEdges(i, Mathf.Max(num, this.maxVotesBeforeSmoosh));
			Vector3 vector2 = spriteRenderer.transform.localPosition;
			vector2 = Vector2.Lerp(vector2, vector, Time.deltaTime * this.adjustRate);
			vector2.z = (float)(-(float)i) / 50f;
			spriteRenderer.transform.localPosition = vector2;
		}
	}

	public void AddVote(SpriteRenderer newVote)
	{
		newVote.transform.localPosition = new Vector3(this.VoteRange.max, this.CounterY, 0f);
		this.Votes.Add(newVote);
	}
}
