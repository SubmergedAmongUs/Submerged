using System;
using System.Collections.Generic;

namespace GameCore
{
	public class UserManager
	{
		public const bool isSingleUserGame = true;

		public List<UserManager.UserData> UserDataList = new List<UserManager.UserData>();

		private UserManager.UserData primaryUser;

		public UserManager.UserData PrimaryUser
		{
			get
			{
				return this.primaryUser;
			}
			private set
			{
			}
		}

		public enum UserOpResult
		{
			Success,
			NoDefaultUser,
			ResolveUserIssueRequired,
			UnclearedVetoes,
			UnknownError
		}

		private enum State
		{
			Initializing,
			GetContext,
			WaitForAddingUser,
			GetBasicInfo,
			InitializeNetwork,
			GrabAchievements,
			UserDisplayImage,
			ReturnMuteList,
			ReturnAvoidList,
			UserPermissionsCheck,
			WaitForNextTask,
			Error,
			Idle,
			End
		}

		public class UserData
		{
		}

		public delegate void AddUserCompletedDelegate(UserManager.UserOpResult result);
	}
}
