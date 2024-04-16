using System;

namespace GooglePlayGames.BasicApi
{
	public enum LoadFriendsStatus
	{
		Unknown,
		Completed,
		LoadMore,
		ResolutionRequired = -3,
		InternalError = -4,
		NotAuthorized = -5,
		NetworkError = -6
	}
}
