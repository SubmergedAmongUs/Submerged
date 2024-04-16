using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class SnowAttacher : MonoBehaviour
{
	public GameObject SnowPrefab;

	public void Start()
	{
		Object.Instantiate<GameObject>(this.SnowPrefab, DestroyableSingleton<HudManager>.Instance.transform).transform.localPosition = new Vector3(0f, 3f, 0f);
	}
}
