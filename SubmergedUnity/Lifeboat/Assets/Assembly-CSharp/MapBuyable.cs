using System;
using UnityEngine;

[CreateAssetMenu]
public class MapBuyable : ScriptableObject, IBuyable, ISteamBuyable, IEpicBuyable
{
	public StringNames StoreName;

	public string productId;

	public bool IncludeHats;

	public string EpicId;

	public uint SteamId;

	public string Win10Id;

	public Sprite StoreImage;

	public string ProdId
	{
		get
		{
			return this.productId;
		}
	}

	public string SteamPrice
	{
		get
		{
			return "$3.99";
		}
	}

	public string EpicAppId
	{
		get
		{
			return this.EpicId;
		}
	}

	public string EpicPrice
	{
		get
		{
			return "$3.99";
		}
	}

	public uint SteamAppId
	{
		get
		{
			return this.SteamId;
		}
	}
}
