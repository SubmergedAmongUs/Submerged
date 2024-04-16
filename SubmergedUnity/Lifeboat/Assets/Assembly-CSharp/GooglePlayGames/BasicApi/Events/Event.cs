using System;

namespace GooglePlayGames.BasicApi.Events
{
	internal class Event : IEvent
	{
		private string mId;

		private string mName;

		private string mDescription;

		private string mImageUrl;

		private ulong mCurrentCount;

		private EventVisibility mVisibility;

		internal Event(string id, string name, string description, string imageUrl, ulong currentCount, EventVisibility visibility)
		{
			this.mId = id;
			this.mName = name;
			this.mDescription = description;
			this.mImageUrl = imageUrl;
			this.mCurrentCount = currentCount;
			this.mVisibility = visibility;
		}

		public string Id
		{
			get
			{
				return this.mId;
			}
		}

		public string Name
		{
			get
			{
				return this.mName;
			}
		}

		public string Description
		{
			get
			{
				return this.mDescription;
			}
		}

		public string ImageUrl
		{
			get
			{
				return this.mImageUrl;
			}
		}

		public ulong CurrentCount
		{
			get
			{
				return this.mCurrentCount;
			}
		}

		public EventVisibility Visibility
		{
			get
			{
				return this.mVisibility;
			}
		}
	}
}
