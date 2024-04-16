using System;
using System.Collections.Generic;
using UnityEngine;

public class CounterArea : MonoBehaviour
{
	public SystemTypes RoomType;

	public ObjectPoolBehavior pool;

	private List<PoolableBehavior> myIcons = new List<PoolableBehavior>();

	public float XOffset = 0.3f;

	public float YOffset = 0.3f;

	public int MaxWidth = 5;

	public void UpdateCount(int cnt)
	{
		bool flag = this.myIcons.Count != cnt;
		while (this.myIcons.Count < cnt)
		{
			PoolableBehavior item = this.pool.Get<PoolableBehavior>();
			this.myIcons.Add(item);
		}
		while (this.myIcons.Count > cnt)
		{
			PoolableBehavior poolableBehavior = this.myIcons[this.myIcons.Count - 1];
			this.myIcons.RemoveAt(this.myIcons.Count - 1);
			poolableBehavior.OwnerPool.Reclaim(poolableBehavior);
		}
		if (flag)
		{
			for (int i = 0; i < this.myIcons.Count; i++)
			{
				int num = i % 5;
				int num2 = i / 5;
				float num3 = (float)(Mathf.Min(cnt - num2 * 5, 5) - 1) * this.XOffset / -2f;
				this.myIcons[i].transform.position = base.transform.position + new Vector3(num3 + (float)num * this.XOffset, (float)num2 * this.YOffset, -1f);
			}
		}
	}
}
