using System;

namespace GooglePlayGames.BasicApi.SavedGame
{
	public enum SelectUIStatus
	{
		SavedGameSelected = 1,
		UserClosedUI,
		InternalError = -1,
		TimeoutError = -2,
		AuthenticationError = -3,
		BadInputError = -4,
		UiBusy = -5
	}
}
