using System;

namespace UnityEngine.SignInWithApple
{
	public struct UserInfo
	{
		public string userId;

		public string email;

		public string displayName;

		public string idToken;

		public string error;

		public UserDetectionStatus userDetectionStatus;
	}
}
