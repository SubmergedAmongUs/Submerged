using System;
using UnityEngine;

public class PoolableBehavior : MonoBehaviour
{
	[HideInInspector]
	public IObjectPool OwnerPool;

	[HideInInspector]
	public int PoolIndex;

	public virtual void Reset()
	{
	}

	public void Awake()
	{
		this.OwnerPool = DefaultPool.Instance;
	}
}
