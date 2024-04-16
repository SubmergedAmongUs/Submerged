using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class DestroyableSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance;

	public bool DontDestroy;

	public static bool InstanceExists
	{
		get
		{
			return DestroyableSingleton<T>._instance;
		}
	}

	public virtual void Awake()
	{
		if (!DestroyableSingleton<T>._instance)
		{
			DestroyableSingleton<T>._instance = (this as T);
			if (this.DontDestroy)
			{
				Object.DontDestroyOnLoad(base.gameObject);
				return;
			}
		}
		else if (DestroyableSingleton<T>._instance != this)
		{
			 UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public static T Instance
	{
		get
		{
			if (!DestroyableSingleton<T>._instance)
			{
				DestroyableSingleton<T>._instance = UnityEngine.Object.FindObjectOfType<T>();
				if (!DestroyableSingleton<T>._instance)
				{
					DestroyableSingleton<T>._instance = new GameObject().AddComponent<T>();
				}
			}
			return DestroyableSingleton<T>._instance;
		}
	}

	public virtual void OnDestroy()
	{
		if (!this.DontDestroy && DestroyableSingleton<T>._instance == this)
		{
			DestroyableSingleton<T>._instance = default(T);
		}
	}
}
