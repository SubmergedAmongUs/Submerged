using System;
using UnityEngine;

public abstract class IObjectPool : MonoBehaviour
{
	public abstract T Get<T>() where T : PoolableBehavior;

	public abstract void Reclaim(PoolableBehavior obj);

	public abstract int InUse { get; }

	public abstract int NotInUse { get; }
}
