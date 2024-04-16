using System;

namespace GooglePlayGames.BasicApi
{
	public enum FriendsListVisibilityStatus
	{
		Unknown,
		Visible,
		ResolutionRequired,
		Unavailable,
		NetworkError = -4,
		NotAuthorized = -5
	}
}
