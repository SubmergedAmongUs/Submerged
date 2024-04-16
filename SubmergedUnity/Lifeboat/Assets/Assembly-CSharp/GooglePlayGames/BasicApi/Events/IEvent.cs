using System;

namespace GooglePlayGames.BasicApi.Events
{
	public interface IEvent
	{
		string Id { get; }

		string Name { get; }

		string Description { get; }

		string ImageUrl { get; }

		ulong CurrentCount { get; }

		EventVisibility Visibility { get; }
	}
}
