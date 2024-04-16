using System;
using System.Collections.Generic;
using UnityEngine;

namespace GooglePlayGames.BasicApi
{
	public class SignInHelper
	{
		private static int True = 0;

		private static int False = 1;

		private const string PromptSignInKey = "prompt_sign_in";

		public static SignInStatus ToSignInStatus(int code)
		{
			Dictionary<int, SignInStatus> dictionary = new Dictionary<int, SignInStatus>
			{
				{
					-12,
					SignInStatus.AlreadyInProgress
				},
				{
					0,
					SignInStatus.Success
				},
				{
					4,
					SignInStatus.UiSignInRequired
				},
				{
					7,
					SignInStatus.NetworkError
				},
				{
					8,
					SignInStatus.InternalError
				},
				{
					10,
					SignInStatus.DeveloperError
				},
				{
					16,
					SignInStatus.Canceled
				},
				{
					17,
					SignInStatus.Failed
				},
				{
					12500,
					SignInStatus.Failed
				},
				{
					12501,
					SignInStatus.Canceled
				},
				{
					12502,
					SignInStatus.AlreadyInProgress
				}
			};
			if (!dictionary.ContainsKey(code))
			{
				return SignInStatus.Failed;
			}
			return dictionary[code];
		}

		public static void SetPromptUiSignIn(bool value)
		{
			PlayerPrefs.SetInt("prompt_sign_in", value ? SignInHelper.True : SignInHelper.False);
		}

		public static bool ShouldPromptUiSignIn()
		{
			return PlayerPrefs.GetInt("prompt_sign_in", SignInHelper.True) != SignInHelper.False;
		}
	}
}
