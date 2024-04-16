using System;
using GooglePlayGames.OurUtils;

namespace GooglePlayGames.BasicApi.SavedGame
{
	public struct SavedGameMetadataUpdate
	{
		private readonly bool mDescriptionUpdated;

		private readonly string mNewDescription;

		private readonly bool mCoverImageUpdated;

		private readonly byte[] mNewPngCoverImage;

		private readonly TimeSpan? mNewPlayedTime;

		private SavedGameMetadataUpdate(SavedGameMetadataUpdate.Builder builder)
		{
			this.mDescriptionUpdated = builder.mDescriptionUpdated;
			this.mNewDescription = builder.mNewDescription;
			this.mCoverImageUpdated = builder.mCoverImageUpdated;
			this.mNewPngCoverImage = builder.mNewPngCoverImage;
			this.mNewPlayedTime = builder.mNewPlayedTime;
		}

		public bool IsDescriptionUpdated
		{
			get
			{
				return this.mDescriptionUpdated;
			}
		}

		public string UpdatedDescription
		{
			get
			{
				return this.mNewDescription;
			}
		}

		public bool IsCoverImageUpdated
		{
			get
			{
				return this.mCoverImageUpdated;
			}
		}

		public byte[] UpdatedPngCoverImage
		{
			get
			{
				return this.mNewPngCoverImage;
			}
		}

		public bool IsPlayedTimeUpdated
		{
			get
			{
				return this.mNewPlayedTime != null;
			}
		}

		public TimeSpan? UpdatedPlayedTime
		{
			get
			{
				return this.mNewPlayedTime;
			}
		}

		public struct Builder
		{
			internal bool mDescriptionUpdated;

			internal string mNewDescription;

			internal bool mCoverImageUpdated;

			internal byte[] mNewPngCoverImage;

			internal TimeSpan? mNewPlayedTime;

			public SavedGameMetadataUpdate.Builder WithUpdatedDescription(string description)
			{
				this.mNewDescription = Misc.CheckNotNull<string>(description);
				this.mDescriptionUpdated = true;
				return this;
			}

			public SavedGameMetadataUpdate.Builder WithUpdatedPngCoverImage(byte[] newPngCoverImage)
			{
				this.mCoverImageUpdated = true;
				this.mNewPngCoverImage = newPngCoverImage;
				return this;
			}

			public SavedGameMetadataUpdate.Builder WithUpdatedPlayedTime(TimeSpan newPlayedTime)
			{
				if (newPlayedTime.TotalMilliseconds > 1.8446744073709552E+19)
				{
					throw new InvalidOperationException("Timespans longer than ulong.MaxValue milliseconds are not allowed");
				}
				this.mNewPlayedTime = new TimeSpan?(newPlayedTime);
				return this;
			}

			public SavedGameMetadataUpdate Build()
			{
				return new SavedGameMetadataUpdate(this);
			}
		}
	}
}
