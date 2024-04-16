using System;

namespace GooglePlayGames.BasicApi.Video
{
	public class VideoCaptureState
	{
		private bool mIsCapturing;

		private VideoCaptureMode mCaptureMode;

		private VideoQualityLevel mQualityLevel;

		private bool mIsOverlayVisible;

		private bool mIsPaused;

		internal VideoCaptureState(bool isCapturing, VideoCaptureMode captureMode, VideoQualityLevel qualityLevel, bool isOverlayVisible, bool isPaused)
		{
			this.mIsCapturing = isCapturing;
			this.mCaptureMode = captureMode;
			this.mQualityLevel = qualityLevel;
			this.mIsOverlayVisible = isOverlayVisible;
			this.mIsPaused = isPaused;
		}

		public bool IsCapturing
		{
			get
			{
				return this.mIsCapturing;
			}
		}

		public VideoCaptureMode CaptureMode
		{
			get
			{
				return this.mCaptureMode;
			}
		}

		public VideoQualityLevel QualityLevel
		{
			get
			{
				return this.mQualityLevel;
			}
		}

		public bool IsOverlayVisible
		{
			get
			{
				return this.mIsOverlayVisible;
			}
		}

		public bool IsPaused
		{
			get
			{
				return this.mIsPaused;
			}
		}

		public override string ToString()
		{
			return string.Format("[VideoCaptureState: mIsCapturing={0}, mCaptureMode={1}, mQualityLevel={2}, mIsOverlayVisible={3}, mIsPaused={4}]", new object[]
			{
				this.mIsCapturing,
				this.mCaptureMode.ToString(),
				this.mQualityLevel.ToString(),
				this.mIsOverlayVisible,
				this.mIsPaused
			});
		}
	}
}
