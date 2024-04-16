using System;

namespace GooglePlayGames.BasicApi
{
	public enum SignInStatus
	{
		Success,
		UiSignInRequired,
		DeveloperError,
		NetworkError,
		InternalError,
		Canceled,
		AlreadyInProgress,
		Failed,
		NotAuthenticated
	}
}
