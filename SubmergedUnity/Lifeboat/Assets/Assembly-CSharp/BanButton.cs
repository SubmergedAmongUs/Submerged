using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class BanButton : MonoBehaviour
{
	public TextMeshPro NameText;

	public SpriteRenderer Background;

	public int TargetClientId;

	public int numVotes;

	public BanMenu Parent { get; set; }

	public void Start()
	{
		this.Background.SetCooldownNormalizedUvs();
	}

	public void Select()
	{
		this.Background.color = new Color(1f, 1f, 1f, 1f);
		this.Parent.Select(this.TargetClientId);
	}

	public void Unselect()
	{
		this.Background.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
	}

	public void SetVotes(int newVotes)
	{
		base.StopAllCoroutines();
		base.StartCoroutine(this.CoSetVotes(this.numVotes, newVotes));
		this.numVotes = newVotes;
	}

	private IEnumerator CoSetVotes(int oldNum, int newNum)
	{
		float num = (float)oldNum / 3f;
		float end = (float)newNum / 3f;
		for (float timer = 0f; timer < 0.2f; timer += Time.deltaTime)
		{
			this.Background.material.SetFloat("_Percent", Mathf.SmoothStep(end, end, timer / 0.2f));
			yield return null;
		}
		this.Background.material.SetFloat("_Percent", end);
		yield break;
	}
}
