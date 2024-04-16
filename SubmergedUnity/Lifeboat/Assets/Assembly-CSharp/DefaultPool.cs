using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class DefaultPool : IObjectPool
{
	private static DefaultPool _instance;

	private static object _lock = new object();

	public override int InUse
	{
		get
		{
			return 0;
		}
	}

	public override int NotInUse
	{
		get
		{
			return 0;
		}
	}

	public static bool InstanceExists
	{
		get
		{
			return DefaultPool._instance;
		}
	}

	public static DefaultPool Instance
	{
		get
		{
			object @lock = DefaultPool._lock;
			DefaultPool instance;
			lock (@lock)
			{
				if (DefaultPool._instance == null)
				{
					DefaultPool._instance = UnityEngine.Object.FindObjectOfType<DefaultPool>();
					if (Object.FindObjectsOfType<DefaultPool>().Length > 1)
					{
						Debug.LogError("[Singleton] Something went really wrong  - there should never be more than 1 singleton! Reopening the scene might fix it.");
						return DefaultPool._instance;
					}
					if (DefaultPool._instance == null)
					{
						GameObject gameObject = new GameObject();
						DefaultPool._instance = gameObject.AddComponent<DefaultPool>();
						gameObject.name = "(singleton) DefaultPool";
					}
				}
				instance = DefaultPool._instance;
			}
			return instance;
		}
	}

	public void OnDestroy()
	{
		object @lock = DefaultPool._lock;
		lock (@lock)
		{
			DefaultPool._instance = null;
		}
	}

	public override T Get<T>()
	{
		throw new NotImplementedException();
	}

	public override void Reclaim(PoolableBehavior obj)
	{
		Debug.Log("Default Pool: Destroying this thing.");
		 UnityEngine.Object.Destroy(obj.gameObject);
	}
}
