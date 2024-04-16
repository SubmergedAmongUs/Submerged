using System;
using System.Collections;
using UnityEngine;

public class FollowerCamera : MonoBehaviour
{
	public MonoBehaviour Target;

	public Vector2 Offset;

	public bool Locked;

	public float shakeAmount;

	public float shakePeriod = 1f;

	private Vector2 centerPosition;

	public void Update()
	{
		if (this.Target && !this.Locked)
		{
			this.centerPosition = Vector3.Lerp(this.centerPosition, this.Target.transform.position + (Vector3) this.Offset, 5f * Time.deltaTime);
			Vector2 vector = this.centerPosition;
			if (this.shakeAmount > 0f)
			{
				float num = Time.fixedTime * this.shakePeriod;
				float num2 = Mathf.PerlinNoise(0.5f, num) * 2f - 1f;
				float num3 = Mathf.PerlinNoise(num, 0.5f) * 2f - 1f;
				vector.x += num2 * this.shakeAmount;
				vector.y += num3 * this.shakeAmount;
			}
			base.transform.position = vector;
		}
	}

	public void ShakeScreen(float duration, float severity)
	{
		base.StartCoroutine(this.CoShakeScreen(duration, severity));
	}

	private IEnumerator CoShakeScreen(float duration, float severity)
	{
		WaitForFixedUpdate wait = new WaitForFixedUpdate();
		for (float t = duration; t > 0f; t -= Time.fixedDeltaTime)
		{
			float num = t / duration;
			this.Offset = UnityEngine.Random.insideUnitCircle * num * severity;
			yield return wait;
		}
		this.Offset = Vector2.zero;
		yield break;
	}

	internal void SetTarget(MonoBehaviour target)
	{
		this.Target = target;
		this.SnapToTarget();
	}

	public void SnapToTarget()
	{
		base.transform.position = this.Target.transform.position + (Vector3) this.Offset;
	}
}
