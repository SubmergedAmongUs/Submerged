using System;
using System.Linq;
using GooglePlayGames.OurUtils;

namespace GooglePlayGames.BasicApi.Video
{
	public class VideoCapabilities
	{
		private bool mIsCameraSupported;

		private bool mIsMicSupported;

		private bool mIsWriteStorageSupported;

		private bool[] mCaptureModesSupported;

		private bool[] mQualityLevelsSupported;

		internal VideoCapabilities(bool isCameraSupported, bool isMicSupported, bool isWriteStorageSupported, bool[] captureModesSupported, bool[] qualityLevelsSupported)
		{
			this.mIsCameraSupported = isCameraSupported;
			this.mIsMicSupported = isMicSupported;
			this.mIsWriteStorageSupported = isWriteStorageSupported;
			this.mCaptureModesSupported = captureModesSupported;
			this.mQualityLevelsSupported = qualityLevelsSupported;
		}

		public bool IsCameraSupported
		{
			get
			{
				return this.mIsCameraSupported;
			}
		}

		public bool IsMicSupported
		{
			get
			{
				return this.mIsMicSupported;
			}
		}

		public bool IsWriteStorageSupported
		{
			get
			{
				return this.mIsWriteStorageSupported;
			}
		}

		public bool SupportsCaptureMode(VideoCaptureMode captureMode)
		{
			if (captureMode != VideoCaptureMode.Unknown)
			{
				return this.mCaptureModesSupported[(int)captureMode];
			}
			Logger.w("SupportsCaptureMode called with an unknown captureMode.");
			return false;
		}

		public bool SupportsQualityLevel(VideoQualityLevel qualityLevel)
		{
			if (qualityLevel != VideoQualityLevel.Unknown)
			{
				return this.mQualityLevelsSupported[(int)qualityLevel];
			}
			Logger.w("SupportsCaptureMode called with an unknown qualityLevel.");
			return false;
		}

		public override string ToString()
		{
			string format = "[VideoCapabilities: mIsCameraSupported={0}, mIsMicSupported={1}, mIsWriteStorageSupported={2}, mCaptureModesSupported={3}, mQualityLevelsSupported={4}]";
			object[] array = new object[5];
			array[0] = this.mIsCameraSupported;
			array[1] = this.mIsMicSupported;
			array[2] = this.mIsWriteStorageSupported;
			array[3] = string.Join(",", (from p in this.mCaptureModesSupported
			select p.ToString()).ToArray<string>());
			array[4] = string.Join(",", (from p in this.mQualityLevelsSupported
			select p.ToString()).ToArray<string>());
			return string.Format(format, array);
		}
	}
}
