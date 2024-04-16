using System;

namespace GooglePlayGames.BasicApi.SavedGame
{
	public interface ISavedGameMetadata
	{
		bool IsOpen { get; }

		string Filename { get; }

		string Description { get; }

		string CoverImageURL { get; }

		TimeSpan TotalTimePlayed { get; }

		DateTime LastModifiedTimestamp { get; }
	}
}
