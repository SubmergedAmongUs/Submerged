using System;
using System.Collections.Generic;

namespace GameCore
{
	public static class HrErrorCode
	{
		private static Dictionary<uint, string> ErrorMessages = new Dictionary<uint, string>
		{
			{
				2300838144U,
				"The game runtime has not yet been initialized."
			},
			{
				2300838145U,
				"The game runtime DLL was not found."
			},
			{
				2300838146U,
				"The game runtime DLL does not support this version of the Microsoft Game Development Kit (GDK)."
			},
			{
				2300858624U,
				"Can’t add this user because the max number of users has been added."
			},
			{
				2300858625U,
				"Can’t perform the operation because the user is signed out."
			},
			{
				2300858626U,
				"Needs UI to resolve an issue with this user. In general, if you’re getting E_GAMEUSER_RESOLVE_USER_ISSUE_REQUIRED (and this can happen from multiple APIs), you should call XUserResolveIssueWithUiAsync. If you got this error when you called XUserAddAsync(AddDefaultUsersSilently, …), this means you’re still trying to get a user and will need to call XUserAddAsync again without AddDefaultUserSilently."
			},
			{
				2300858627U,
				"Not an appropriate time to request deferral."
			},
			{
				2300858628U,
				"User matching the id was not found."
			},
			{
				2300858629U,
				"No token is required for this call."
			},
			{
				2300858630U,
				"There is no current default user. If you’re getting this error, it likely means that you called XUserAddAsync(AddDefaultUsersSilently, …). To fix the issue, you should call XUserAddAsync again, this time without AddDefaultUserSilently to get a user."
			},
			{
				2300858631U,
				"Failed to resolve the given privilege."
			},
			{
				2300858632U,
				"An Xbox live title id must be configured."
			},
			{
				2300858633U,
				"The game identity is not recognized. This error happens when the <MSAAppId> and <TitleId> don’t match the ones associated to the game."
			},
			{
				2300858640U,
				"A package identity must be configured."
			},
			{
				2300858641U,
				"The token request failed."
			},
			{
				2300858880U,
				"The game is not packaged in a container."
			},
			{
				2300858881U,
				"The game uses Intelligent Delivery to selectively install languages, but none of the languages are installed."
			},
			{
				2300859136U,
				"The game requested a license for a product that cannot be licensed."
			},
			{
				2300859137U,
				"The game failed to communicate with the store network."
			},
			{
				2300859138U,
				"The game received a bad response from the store server."
			},
			{
				2300859139U,
				"The user does not have enough of this consumable to use the requested amount."
			},
			{
				2300859140U,
				"The user already owns this product."
			},
			{
				2300859392U,
				"The XGameStreaming runtime has not been initialized. Call XGameStreamingInitialize before calling other APIs."
			},
			{
				2300859393U,
				"The specified client is not connected."
			},
			{
				2300859394U,
				"The requested data is not available. The data may be available later."
			},
			{
				2300859395U,
				"The current machine is not running in a datacenter."
			},
			{
				2300859396U,
				"The current reading didn’t come from a streaming controller."
			}
		};

		public static string GetHrErrorCode(int hresult)
		{
			if (HrErrorCode.ErrorMessages.ContainsKey((uint)hresult))
			{
				return string.Format("{0} - {1}", (HrErrorCode.ErrorCode)hresult, HrErrorCode.ErrorMessages[(uint)hresult]);
			}
			return string.Format("{0} not in Error code list", hresult);
		}

		public enum ErrorCode : uint
		{
			E_GAMERUNTIME_NOT_INITIALIZED = 2300838144U,
			E_GAMERUNTIME_DLL_NOT_FOUND,
			E_GAMERUNTIME_VERSION_MISMATCH,
			E_GAMEUSER_MAX_USERS_ADDED = 2300858624U,
			E_GAMEUSER_SIGNED_OUT,
			E_GAMEUSER_RESOLVE_USER_ISSUE_REQUIRED,
			E_GAMEUSER_DEFERRAL_NOT_AVAILABLE,
			E_GAMEUSER_USER_NOT_FOUND,
			E_GAMEUSER_NO_TOKEN_REQUIRED,
			E_GAMEUSER_NO_DEFAULT_USER,
			E_GAMEUSER_FAILED_TO_RESOLVE,
			E_GAMEUSER_NO_TITLE_ID,
			E_GAMEUSER_UNKNOWN_GAME_IDENTITY,
			E_GAMEUSER_NO_PACKAGE_IDENTITY = 2300858640U,
			E_GAMEUSER_FAILED_TO_GET_TOKEN,
			E_GAMEPACKAGE_APP_NOT_PACKAGED = 2300858880U,
			E_GAMEPACKAGE_NO_INSTALLED_LANGUAGES,
			E_GAMESTORE_LICENSE_ACTION_NOT_APPLICABLE_TO_PRODUCT = 2300859136U,
			E_GAMESTORE_NETWORK_ERROR,
			E_GAMESTORE_SERVER_ERROR,
			E_GAMESTORE_INSUFFICIENT_QUANTITY,
			E_GAMESTORE_ALREADY_PURCHASED,
			E_GAMESTREAMING_NOT_INITIALIZED = 2300859392U,
			E_GAMESTREAMING_CLIENT_NOT_CONNECTED,
			E_GAMESTREAMING_NO_DATA,
			E_GAMESTREAMING_NO_DATACENTER,
			E_GAMESTREAMING_NOT_STREAMING_CONTROLLER
		}
	}
}
