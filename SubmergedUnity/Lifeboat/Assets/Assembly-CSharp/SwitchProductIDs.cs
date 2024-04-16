using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-15000)]
public class SwitchProductIDs : MonoBehaviour
{
	[SerializeField]
	private List<SwitchProductIDs.SwitchProductID> productIDs = new List<SwitchProductIDs.SwitchProductID>();

	private void Awake()
	{
		 UnityEngine.Object.Destroy(this);
	}

	[Serializable]
	public class SwitchProductID
	{
		public int aocIndex;

		public string correspondingProductID;
	}
}
