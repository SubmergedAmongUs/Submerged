using System;

namespace GooglePlayGames.BasicApi
{
	public enum ResponseStatus
	{
		Success = 1,
		SuccessWithStale,
		LicenseCheckFailed = -1,
		InternalError = -2,
		NotAuthorized = -3,
		VersionUpdateRequired = -4,
		Timeout = -5,
		ResolutionRequired = -6
	}
}
