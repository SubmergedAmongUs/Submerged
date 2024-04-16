using System;
using System.Collections.Generic;
using InnerNet;

public interface IGameListHandler
{
	void HandleList(InnerNetClient.TotalGameData totalGames, List<GameListing> availableGames);
}
