using System;
using UnityEngine;
using UnityEngine.Events;

public class DelayedFunctionCall : MonoBehaviour
{
	public UnityEvent onTimerElapsed;

	public float delayDuration = 1f;

	private float t;

	private void Update()
	{
		if (this.t >= this.delayDuration)
		{
			base.enabled = false;
			if (this.onTimerElapsed != null)
			{
				this.onTimerElapsed.Invoke();
			}
		}
		this.t += Time.deltaTime;
	}
}
