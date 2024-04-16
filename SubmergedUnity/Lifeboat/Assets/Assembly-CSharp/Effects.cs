using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Effects
{
	private static HashSet<Transform> activeShakes = new HashSet<Transform>();

	public static IEnumerator Action(Action todo)
	{
		todo();
		yield break;
	}

	public static IEnumerator Wait(float duration)
	{
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			yield return null;
		}
		yield break;
	}

	public static IEnumerator Sequence(params IEnumerator[] items)
	{
		int num;
		for (int i = 0; i < items.Length; i = num)
		{
			yield return items[i];
			num = i + 1;
		}
		yield break;
	}

	public static IEnumerator All(params IEnumerator[] items)
	{
		Stack<IEnumerator>[] enums = new Stack<IEnumerator>[items.Length];
		for (int i = 0; i < items.Length; i++)
		{
			enums[i] = new Stack<IEnumerator>();
			enums[i].Push(items[i]);
		}
		int num;
		for (int cap = 0; cap < 100000; cap = num)
		{
			bool flag = false;
			for (int j = 0; j < enums.Length; j++)
			{
				if (enums[j].Count > 0)
				{
					flag = true;
					IEnumerator enumerator = enums[j].Peek();
					if (enumerator.MoveNext())
					{
						if (enumerator.Current is IEnumerator)
						{
							enums[j].Push((IEnumerator)enumerator.Current);
						}
					}
					else
					{
						enums[j].Pop();
					}
				}
			}
			if (!flag)
			{
				break;
			}
			yield return null;
			num = cap + 1;
		}
		yield break;
	}

	internal static IEnumerator Lerp(float duration, Action<float> action)
	{
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			action(t / duration);
			yield return null;
		}
		action(1f);
		yield break;
	}

	internal static IEnumerator Overlerp(float duration, Action<float> action, float overextend = 0.05f)
	{
		float d = duration * 0.95f;
		for (float t = 0f; t < d; t += Time.deltaTime)
		{
			action(Mathf.Lerp(0f, 1f + overextend, t / d));
			yield return null;
		}
		float d2 = duration * 0.050000012f;
		for (float t = 0f; t < d2; t += Time.deltaTime)
		{
			action(Mathf.Lerp(1f + overextend, 1f, t / d2));
			yield return null;
		}
		action(1f);
		yield break;
	}

	internal static IEnumerator ScaleIn(Transform self, float source, float target, float duration)
	{
		if (!self)
		{
			yield break;
		}
		Vector3 localScale = default(Vector3);
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			localScale.x = (localScale.y = (localScale.z = Mathf.SmoothStep(source, target, t / duration)));
			self.localScale = localScale;
			yield return null;
		}
		localScale.z = target;
		localScale.y = target;
		localScale.x = target;
		self.localScale = localScale;
		yield break;
	}

	internal static IEnumerator CycleColors(SpriteRenderer self, Color source, Color target, float rate, float duration)
	{
		if (!self)
		{
			yield break;
		}
		self.enabled = true;
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			float num = Mathf.Sin(t * 3.1415927f / rate) / 2f + 0.5f;
			self.color = Color.Lerp(source, target, num);
			yield return null;
		}
		self.color = source;
		yield break;
	}

	internal static IEnumerator PulseColor(SpriteRenderer self, Color source, Color target, float duration = 0.5f)
	{
		if (!self)
		{
			yield break;
		}
		self.enabled = true;
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			self.color = Color.Lerp(target, source, t / duration);
			yield return null;
		}
		self.color = source;
		yield break;
	}

	internal static IEnumerator PulseColor(TextMeshPro self, Color source, Color target, float duration = 0.5f)
	{
		if (!self)
		{
			yield break;
		}
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			self.color = Color.Lerp(target, source, t / duration);
			yield return null;
		}
		self.color = source;
		yield break;
	}

	public static IEnumerator ColorFade(TextMeshPro self, Color source, Color target, float duration)
	{
		if (!self)
		{
			yield break;
		}
		self.enabled = true;
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			self.color = Color.Lerp(source, target, t / duration);
			yield return null;
		}
		self.color = target;
		yield break;
	}

	public static IEnumerator ColorFade(SpriteRenderer self, Color source, Color target, float duration)
	{
		if (!self)
		{
			yield break;
		}
		self.enabled = true;
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			self.color = Color.Lerp(source, target, t / duration);
			yield return null;
		}
		self.color = target;
		yield break;
	}

	public static IEnumerator Rotate2D(Transform target, float source, float dest, float duration = 0.75f)
	{
		Vector3 temp = target.localEulerAngles;
		for (float time = 0f; time < duration; time += Time.deltaTime)
		{
			float num = time / duration;
			temp.z = Mathf.SmoothStep(source, dest, num);
			target.localEulerAngles = temp;
			yield return null;
		}
		temp.z = dest;
		target.localEulerAngles = temp;
		yield break;
	}

	public static IEnumerator Slide3D(Transform target, Vector3 source, Vector3 dest, float duration = 0.75f)
	{
		Vector3 localPosition = default(Vector3);
		for (float time = 0f; time < duration; time += Time.deltaTime)
		{
			float num = time / duration;
			localPosition.x = Mathf.SmoothStep(source.x, dest.x, num);
			localPosition.y = Mathf.SmoothStep(source.y, dest.y, num);
			localPosition.z = Mathf.Lerp(source.z, dest.z, num);
			target.localPosition = localPosition;
			yield return null;
		}
		target.localPosition = dest;
		yield break;
	}

	public static IEnumerator Slide2D(Transform target, Vector2 source, Vector2 dest, float duration = 0.75f)
	{
		Vector3 temp = default(Vector3);
		temp.z = target.localPosition.z;
		for (float time = 0f; time < duration; time += Time.deltaTime)
		{
			float num = time / duration;
			temp.x = Mathf.SmoothStep(source.x, dest.x, num);
			temp.y = Mathf.SmoothStep(source.y, dest.y, num);
			target.localPosition = temp;
			yield return null;
		}
		temp.x = dest.x;
		temp.y = dest.y;
		target.localPosition = temp;
		yield break;
	}

	public static IEnumerator Slide2DWorld(Transform target, Vector2 source, Vector2 dest, float duration = 0.75f)
	{
		Vector3 temp = default(Vector3);
		temp.z = target.position.z;
		for (float time = 0f; time < duration; time += Time.deltaTime)
		{
			float num = time / duration;
			temp.x = Mathf.SmoothStep(source.x, dest.x, num);
			temp.y = Mathf.SmoothStep(source.y, dest.y, num);
			target.position = temp;
			yield return null;
		}
		temp.x = dest.x;
		temp.y = dest.y;
		target.position = temp;
		yield break;
	}

	public static IEnumerator Bounce(Transform target, float duration = 0.3f, float height = 0.15f)
	{
		if (!target)
		{
			yield break;
		}
		Vector3 origin = target.localPosition;
		Vector3 temp = origin;
		for (float timer = 0f; timer < duration; timer += Time.deltaTime)
		{
			float num = timer / duration;
			float num2 = 1f - num;
			temp.y = origin.y + height * Mathf.Abs(Mathf.Sin(num * 3.1415927f * 3f)) * num2;
			if (!target)
			{
				yield break;
			}
			target.localPosition = temp;
			yield return null;
		}
		if (target)
		{
			target.transform.localPosition = origin;
		}
		yield break;
	}

	public static IEnumerator Shake(Transform target, float duration, float halfWidth, bool taper)
	{
		Vector3 localPosition = target.localPosition;
		for (float timer = 0f; timer < duration; timer += Time.deltaTime)
		{
			float num = timer / duration;
			Vector3 vector = UnityEngine.Random.insideUnitCircle * halfWidth;
			if (taper)
			{
				vector *= 1f - num;
			}
			target.localPosition += vector;
			yield return null;
		}
		yield break;
	}

	public static IEnumerator SwayX(Transform target, float duration = 0.75f, float halfWidth = 0.25f)
	{
		if (Effects.activeShakes.Add(target))
		{
			Vector3 origin = target.localPosition;
			for (float timer = 0f; timer < duration; timer += Time.deltaTime)
			{
				float num = timer / duration;
				target.localPosition = origin + Vector3.right * (halfWidth * Mathf.Sin(num * 30f) * (1f - num));
				yield return null;
			}
			target.transform.localPosition = origin;
			Effects.activeShakes.Remove(target);
			origin = default(Vector3);
		}
		yield break;
	}

	public static IEnumerator Bloop(float delay, Transform target, float finalSize = 1f, float duration = 0.5f)
	{
		for (float t = 0f; t < delay; t += Time.deltaTime)
		{
			yield return null;
		}
		Vector3 localScale = default(Vector3);
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			float z = Effects.ElasticOut(t, duration) * finalSize;
			localScale.x = (localScale.y = (localScale.z = z));
			target.localScale = localScale;
			yield return null;
		}
		localScale.z = finalSize;
		localScale.y = finalSize;
		localScale.x = finalSize;
		target.localScale = localScale;
		yield break;
	}

	public static IEnumerator ArcSlide(float duration, Transform target, Vector2 sourcePos, Vector2 targetPos, float anchorDistance)
	{
		Vector2 vector = (targetPos - sourcePos) / 2f;
		Vector2 anchor = sourcePos + vector + vector.Rotate(90f).normalized * anchorDistance;
		float z = target.localPosition.z;
		for (float timer = 0f; timer < duration; timer += Time.deltaTime)
		{
			Vector3 localPosition = Effects.Bezier(timer / duration, sourcePos, targetPos, anchor);
			localPosition.z = z;
			target.localPosition = localPosition;
			yield return null;
		}
		target.transform.localPosition = targetPos;
		yield break;
	}

	public static Vector3 Bezier(float t, Vector3 src, Vector3 dest, Vector3 anchor)
	{
		t = Mathf.Clamp(t, 0f, 1f);
		float num = 1f - t;
		return num * num * src + 2f * num * t * anchor + t * t * dest;
	}

	public static Vector2 Bezier(float t, Vector2 src, Vector2 dest, Vector2 anchor)
	{
		t = Mathf.Clamp(t, 0f, 1f);
		float num = 1f - t;
		return num * num * src + 2f * num * t * anchor + t * t * dest;
	}

	private static float ElasticOut(float time, float duration)
	{
		time /= duration;
		float num = time * time;
		float num2 = num * time;
		return 33f * num2 * num + -106f * num * num + 126f * num2 + -67f * num + 15f * time;
	}

	public static float ExpOut(float t)
	{
		return Mathf.Clamp(1f - Mathf.Pow(2f, -10f * t), 0f, 1f);
	}
}
