using System;
using UnityEngine;

public class BuildDefineConditionalObject : MonoBehaviour
{
	public bool isDefined;

	private void Awake()
	{
		base.gameObject.SetActive(this.isDefined);
	}

	private void OnEnable()
	{
		if (!this.isDefined)
		{
			base.gameObject.SetActive(this.isDefined);
		}
	}
}
