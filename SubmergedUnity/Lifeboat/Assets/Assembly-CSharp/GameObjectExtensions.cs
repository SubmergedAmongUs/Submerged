using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions
{
	public static void SetAllGameObjectsActive<T>(this IList<T> self, bool isActive) where T : MonoBehaviour
	{
		for (int i = 0; i < self.Count; i++)
		{
			self[i].gameObject.SetActive(isActive);
		}
	}

	public static T Find<T>(this List<T> self, GameObject toFind) where T : MonoBehaviour
	{
		for (int i = 0; i < self.Count; i++)
		{
			T t = self[i];
			if (t.gameObject == toFind)
			{
				return t;
			}
		}
		return default(T);
	}

	public static void SetLocalX(this Transform self, float x)
	{
		Vector3 localPosition = self.localPosition;
		localPosition.x = x;
		self.localPosition = localPosition;
	}

	public static void SetLocalY(this Transform self, float y)
	{
		Vector3 localPosition = self.localPosition;
		localPosition.y = y;
		self.localPosition = localPosition;
	}

	public static void SetLocalZ(this Transform self, float z)
	{
		Vector3 localPosition = self.localPosition;
		localPosition.z = z;
		self.localPosition = localPosition;
	}

	public static void SetWorldZ(this Transform self, float z)
	{
		Vector3 position = self.position;
		position.z = z;
		self.position = position;
	}

	public static void LookAt2d(this Transform self, Vector3 target)
	{
		Vector3 vector = target - self.transform.position;
		vector.Normalize();
		float num = Mathf.Atan2(vector.y, vector.x);
		if (self.transform.lossyScale.x < 0f)
		{
			num += 3.1415927f;
		}
		self.transform.rotation = Quaternion.Euler(0f, 0f, num * 57.29578f);
	}

	public static void LookAt2d(this Transform self, Transform target)
	{
		self.LookAt2d(target.transform.position);
	}

	public static void DestroyChildren(this Transform self)
	{
		for (int i = self.childCount - 1; i > -1; i--)
		{
			Transform child = self.GetChild(i);
			child.transform.SetParent(null);
			 UnityEngine.Object.Destroy(child.gameObject);
		}
	}

	public static void DestroyChildren(this MonoBehaviour self)
	{
		for (int i = self.transform.childCount - 1; i > -1; i--)
		{
			 UnityEngine.Object.Destroy(self.transform.GetChild(i).gameObject);
		}
	}

	public static void ForEachChild(this GameObject self, Action<GameObject> todo)
	{
		for (int i = self.transform.childCount - 1; i > -1; i--)
		{
			todo(self.transform.GetChild(i).gameObject);
		}
	}

	public static void ForEachChildBehavior<T>(this MonoBehaviour self, Action<T> todo) where T : MonoBehaviour
	{
		for (int i = self.transform.childCount - 1; i > -1; i--)
		{
			T component = self.transform.GetChild(i).GetComponent<T>();
			if (component)
			{
				todo(component);
			}
		}
	}
}
