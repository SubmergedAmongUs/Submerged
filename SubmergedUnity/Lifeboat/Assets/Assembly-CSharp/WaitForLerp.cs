using System;
using System.Collections;
using UnityEngine;

public class WaitForLerp : IEnumerator
{
	private float duration;

	private float timer;

	private Action<float> act;

	public WaitForLerp(float seconds, Action<float> act)
	{
		this.duration = seconds;
		this.act = act;
	}

	public object Current
	{
		get
		{
			return null;
		}
	}

	public bool MoveNext()
	{
		this.timer = Mathf.Min(this.timer + Time.deltaTime, this.duration);
		this.act(this.timer / this.duration);
		return this.timer < this.duration;
	}

	public void Reset()
	{
		this.timer = 0f;
	}
}
