using System;

namespace GooglePlayGames.BasicApi
{
	public enum UIStatus
	{
		Valid = 1,
		InternalError = -2,
		NotAuthorized = -3,
		VersionUpdateRequired = -4,
		Timeout = -5,
		UserClosedUI = -6,
		UiBusy = -12,
		NetworkError = -20
	}
}
