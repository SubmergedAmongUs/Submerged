using System;

namespace GooglePlayGames.BasicApi.SavedGame
{
	public enum ConflictResolutionStrategy
	{
		UseLongestPlaytime,
		UseOriginal,
		UseUnmerged,
		UseManual,
		UseLastKnownGood,
		UseMostRecentlySaved
	}
}
