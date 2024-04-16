using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolBehavior : MonoBehaviour
{
	public int poolSize = 20;
	[SerializeField]
	private List<PoolableBehavior> inactiveChildren = new List<PoolableBehavior>();
	[SerializeField]
	public List<PoolableBehavior> activeChildren = new List<PoolableBehavior>();
	public PoolableBehavior Prefab;
	public bool AutoInit;
	public bool DetachOnGet;
}
