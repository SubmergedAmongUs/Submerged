using System;

namespace GooglePlayGames.BasicApi.Video
{
	public interface CaptureOverlayStateListener
	{
		void OnCaptureOverlayStateChanged(VideoCaptureOverlayState overlayState);
	}
}
