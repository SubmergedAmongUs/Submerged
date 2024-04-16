using System;

namespace GooglePlayGames.BasicApi.SavedGame
{
	public enum SavedGameRequestStatus
	{
		Success = 1,
		TimeoutError = -1,
		InternalError = -2,
		AuthenticationError = -3,
		BadInputError = -4
	}
}
