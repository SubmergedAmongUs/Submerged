using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class BoughtAdsBuyable : Object, IBuyable
{
	public string ProdId
	{
		get
		{
			return "bought_ads";
		}
	}
}
