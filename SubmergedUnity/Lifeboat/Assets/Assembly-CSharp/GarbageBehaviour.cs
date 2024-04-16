using System;
using UnityEngine;

public class GarbageBehaviour : MonoBehaviour
{
	public void FixedUpdate()
	{
		if (base.transform.localPosition.y < -3.49f)
		{
			 UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
