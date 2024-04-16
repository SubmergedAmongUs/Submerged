using System;

internal interface ISteamBuyable
{
	uint SteamAppId { get; }

	string SteamPrice { get; }
}
