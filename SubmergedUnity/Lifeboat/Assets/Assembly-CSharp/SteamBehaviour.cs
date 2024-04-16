using System;
using System.Collections;
using PowerTools;
using UnityEngine;

public class SteamBehaviour : MonoBehaviour
{
	public SpriteAnim anim;

	public FloatRange PlayRate = new FloatRange(0.5f, 1f);

	public void OnEnable()
	{
		base.StartCoroutine(this.Run());
	}

	private IEnumerator Run()
	{
		for (;;)
		{
			float time = this.PlayRate.Next();
			while (time > 0f)
			{
				time -= Time.deltaTime;
				yield return null;
			}
			this.anim.Play(null, 1f);
			while (this.anim.IsPlaying())
			{
				yield return null;
			}
		}
		yield break;
	}
}
