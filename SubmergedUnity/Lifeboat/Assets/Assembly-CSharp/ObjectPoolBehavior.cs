using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolBehavior : IObjectPool
{
	public int poolSize = 20;

	[SerializeField]
	private List<PoolableBehavior> inactiveChildren = new List<PoolableBehavior>();

	[SerializeField]
	public List<PoolableBehavior> activeChildren = new List<PoolableBehavior>();

	public PoolableBehavior Prefab;

	public bool AutoInit;

	public bool DetachOnGet;

	private int childIndex;

	public override int InUse
	{
		get
		{
			return this.activeChildren.Count;
		}
	}

	public override int NotInUse
	{
		get
		{
			return this.inactiveChildren.Count;
		}
	}

	public virtual void Awake()
	{
		if (this.AutoInit)
		{
			this.InitPool(this.Prefab);
		}
	}

	public void InitPool(PoolableBehavior prefab)
	{
		this.AutoInit = false;
		for (int i = 0; i < this.poolSize; i++)
		{
			this.CreateOneInactive(prefab);
		}
	}

	private void CreateOneInactive(PoolableBehavior prefab)
	{
		PoolableBehavior poolableBehavior = UnityEngine.Object.Instantiate<PoolableBehavior>(prefab);
		poolableBehavior.transform.SetParent(base.transform);
		poolableBehavior.gameObject.SetActive(false);
		poolableBehavior.OwnerPool = this;
		this.inactiveChildren.Add(poolableBehavior);
	}

	public void ReclaimOldest()
	{
		if (this.activeChildren.Count > 0)
		{
			this.Reclaim(this.activeChildren[0]);
			return;
		}
		this.InitPool(this.Prefab);
	}

	public void ReclaimAll()
	{
		foreach (PoolableBehavior obj in this.activeChildren.ToArray())
		{
			this.Reclaim(obj);
		}
	}

	public override T Get<T>()
	{
		List<PoolableBehavior> obj = this.inactiveChildren;
		PoolableBehavior poolableBehavior;
		lock (obj)
		{
			if (this.inactiveChildren.Count == 0)
			{
				if (this.activeChildren.Count == 0)
				{
					this.InitPool(this.Prefab);
				}
				else
				{
					this.CreateOneInactive(this.Prefab);
				}
			}
			poolableBehavior = this.inactiveChildren[this.inactiveChildren.Count - 1];
			this.inactiveChildren.RemoveAt(this.inactiveChildren.Count - 1);
			this.activeChildren.Add(poolableBehavior);
			PoolableBehavior poolableBehavior2 = poolableBehavior;
			int num = this.childIndex;
			this.childIndex = num + 1;
			poolableBehavior2.PoolIndex = num;
			if (this.childIndex > this.poolSize)
			{
				this.childIndex = 0;
			}
		}
		if (this.DetachOnGet)
		{
			poolableBehavior.transform.SetParent(null, false);
		}
		poolableBehavior.gameObject.SetActive(true);
		poolableBehavior.Reset();
		return poolableBehavior as T;
	}

	public override void Reclaim(PoolableBehavior obj)
	{
		if (!this)
		{
			DefaultPool.Instance.Reclaim(obj);
			return;
		}
		obj.gameObject.SetActive(false);
		obj.transform.SetParent(base.transform);
		List<PoolableBehavior> obj2 = this.inactiveChildren;
		lock (obj2)
		{
			if (this.activeChildren.Remove(obj))
			{
				this.inactiveChildren.Add(obj);
			}
			else if (this.inactiveChildren.Contains(obj))
			{
				Debug.Log("ObjectPoolBehavior: :| Something was reclaimed without being gotten");
			}
			else
			{
				Debug.Log("ObjectPoolBehavior: Destroying this thing I don't own");
				 UnityEngine.Object.Destroy(obj.gameObject);
			}
		}
	}
}
