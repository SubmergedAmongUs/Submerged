using System;
using System.Collections;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.KWS;
using Epic.OnlineServices.Logging;
using Epic.OnlineServices.Platform;
using InnerNet;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class EOSManager : DestroyableSingleton<EOSManager>
{
	private CallResult<EncryptedAppTicketResponse_t> OnEncryptedAppTicketResponseCallResult;

	[SerializeField]
	private string productName;

	[SerializeField]
	private string productVersion;

	[SerializeField]
	private string productId;

	[SerializeField]
	private string sandboxId;

	[SerializeField]
	private string deploymentId;

	[SerializeField]
	private string clientId;

	[SerializeField]
	private string clientSecret;

	private bool hasRunLoginFlow;

	private const float platformTickInterval = 0.1f;

	private float platformTickTimer;

	private bool platformInitialized;

	private bool loginFlowFinished;

	private PlatformInterface platformInterface;

	private ProductUserId userId;

	private ProductUserId deviceIDuserID;

	private bool hasShownSigninScreen;

	private bool attemptAuthAgain = true;

	private int ageOfConsent;

	private string kwsUserId;

	private ContinuanceToken continuanceToken;

	public string exchangeToken = "";

	private string platformAuthToken = "";

	private bool authExpiredCallbackTriggered;

	private bool hasTriedStartupDeviceID;

	private void AuthWithCorrectPlatformImpl()
	{
		Debug.Log("Auth with Steam");
		if (!SteamManager.Initialized)
		{
			this.PlatformAuthReturn(false);
			return;
		}
		this.OnEncryptedAppTicketResponseCallResult = CallResult<EncryptedAppTicketResponse_t>.Create(new CallResult<EncryptedAppTicketResponse_t>.APIDispatchDelegate(this.OnSteamEncryptedAppTicketLoginCallback));
		SteamAPICall_t steamAPICall_t = SteamUser.RequestEncryptedAppTicket(null, 0);
		this.OnEncryptedAppTicketResponseCallResult.Set(steamAPICall_t, null);
	}

	private void OnSteamEncryptedAppTicketLoginCallback(EncryptedAppTicketResponse_t pCallback, bool bIOFailure)
	{
		Debug.Log("[" + 154.ToString() + " - EncryptedAppTicketResponse] - " + pCallback.m_eResult.ToString());
		if (pCallback.m_eResult != EResult.k_EResultOK)
		{
			Debug.LogError(string.Format("EOS Failed to get Steamworks auth: {0}", pCallback.m_eResult));
			this.PlatformAuthReturn(false);
			return;
		}
		byte[] array = new byte[1024];
		if (SteamUser.GetEncryptedAppTicket(array, 1024, out uint num))
		{
			byte[] array2 = new byte[num];
			Array.Copy(array, 0L, array2, 0L, (long)((ulong)num));
			this.platformAuthToken = Common.ToString(array2);
			this.PlatformAuthReturn(true);
			return;
		}
		Debug.LogError(string.Format("EOS Failed to get Steamworks app ticket: {0}", pCallback.m_eResult));
		this.PlatformAuthReturn(false);
	}

	private void LoginWithCorrectPlatformImpl()
	{
		Debug.Log("Login with Steam");
		if (!string.IsNullOrEmpty(this.platformAuthToken))
		{
			LoginOptions loginOptions = new LoginOptions
			{
				Credentials = new Credentials
				{
					Token = this.platformAuthToken,
					Type = ExternalCredentialType.SteamAppTicket
				}
			};
			this.platformInterface.GetConnectInterface().Login(loginOptions, null, new OnLoginCallback(this.EOSConnectPlatformLoginCallback));
			return;
		}
		if (this.attemptAuthAgain)
		{
			this.attemptAuthAgain = false;
			this.RetryAuthAndLoginImpl();
			return;
		}
		this.attemptAuthAgain = true;
		Debug.LogError("EOS Failed to get Steamworks auth: token empty");
		DestroyableSingleton<AccountManager>.Instance.SignInFail(EOSManager.EOS_ERRORS.SteamworksAuthFail, new Action(this.LogInToDeviceIDForGuestMode));
	}

	private void RetryAuthAndLoginImpl()
	{
		Debug.Log("Retry with Steam");
		this.OnEncryptedAppTicketResponseCallResult = CallResult<EncryptedAppTicketResponse_t>.Create(new CallResult<EncryptedAppTicketResponse_t>.APIDispatchDelegate(this.OnSteamEncryptedAppTicketLoginCallback));
		SteamAPICall_t steamAPICall_t = SteamUser.RequestEncryptedAppTicket(null, 0);
		this.OnEncryptedAppTicketResponseCallResult.Set(steamAPICall_t, null);
	}

	private void OnSteamRetryEncryptedAppTicketLoginCallback(EncryptedAppTicketResponse_t pCallback)
	{
		Debug.Log("[" + 154.ToString() + " - EncryptedAppTicketResponse] - " + pCallback.m_eResult.ToString());
		if (pCallback.m_eResult != EResult.k_EResultOK)
		{
			Debug.LogError(string.Format("EOS Failed to get Steamworks auth: {0}", pCallback.m_eResult));
			this.LoginWithCorrectPlatformImpl();
			return;
		}
		byte[] array = new byte[1024];
		if (SteamUser.GetEncryptedAppTicket(array, 1024, out uint num))
		{
			byte[] array2 = new byte[num];
			Array.Copy(array, 0L, array2, 0L, (long)((ulong)num));
			this.platformAuthToken = Common.ToString(array2);
			this.LoginWithCorrectPlatformImpl();
			return;
		}
		Debug.LogError(string.Format("EOS Failed to get Steamworks app ticket: {0}", pCallback.m_eResult));
		this.LoginWithCorrectPlatformImpl();
	}

	public string ProductName
	{
		get
		{
			return this.productName;
		}
	}

	public string ProductVersion
	{
		get
		{
			return this.productVersion;
		}
	}

	public string ProductId
	{
		get
		{
			return this.productId;
		}
	}

	public string SandboxId
	{
		get
		{
			return this.sandboxId;
		}
	}

	public string DeploymentId
	{
		get
		{
			return this.deploymentId;
		}
	}

	public string ClientId
	{
		get
		{
			return this.clientId;
		}
	}

	public string ClientSecret
	{
		get
		{
			return this.clientSecret;
		}
	}

	public string ProductUserId
	{
		get
		{
			string result = null;
			if (SaveManager.AccountLoginStatus == EOSManager.AccountLoginStatus.Guest)
			{
				ProductUserId productUserId = this.deviceIDuserID;
				if (productUserId != null)
				{
					productUserId.ToString(out result);
				}
			}
			else
			{
				ProductUserId productUserId2 = this.userId;
				if (productUserId2 != null)
				{
					productUserId2.ToString(out result);
				}
			}
			return result;
		}
	}

	public override void Awake()
	{
		base.Awake();
		if (!DestroyableSingleton<EOSManager>.InstanceExists || DestroyableSingleton<EOSManager>.Instance != this)
		{
			return;
		}
		this.InitializePlatformInterface();
	}

	public IEnumerator RunLogin()
	{
		if (SaveManager.AccountLoginStatus == EOSManager.AccountLoginStatus.Offline)
		{
			this.IsAllowedOnline(false);
		}
		if (this.hasRunLoginFlow)
		{
			DestroyableSingleton<AccountManager>.Instance.privacyPolicyBg.gameObject.SetActive(false);
			DestroyableSingleton<AccountManager>.Instance.waitingText.gameObject.SetActive(false);
			yield break;
		}
		this.hasRunLoginFlow = true;
		yield return DestroyableSingleton<AccountManager>.Instance.PrivacyPolicy.Show();
		DestroyableSingleton<AccountManager>.Instance.privacyPolicyBg.gameObject.SetActive(false);
		if (this.platformInitialized)
		{
			this.LoginForKWS(true);
		}
		else
		{
			this.ContinueInOfflineMode();
			DestroyableSingleton<AccountManager>.Instance.SetDLLErrorMode();
			this.ContinueInOfflineMode();
			DestroyableSingleton<AccountManager>.Instance.SignInFail(EOSManager.EOS_ERRORS.InterfaceInitFail, new Action(this.FinishLoginFlow));
		}
		while (!this.HasFinishedLoginFlow())
		{
			yield return null;
		}
		yield break;
	}

	public void InitializePlatformInterface()
	{
		InitializeOptions initializeOptions = new InitializeOptions
		{
			ProductName = this.productName,
			ProductVersion = this.productVersion
		};
		Result result = Result.NotFound;
		try
		{
			result = PlatformInterface.Initialize(initializeOptions);
		}
		catch (DllNotFoundException ex)
		{
			Debug.Log("DLL Not Found: " + ex.Message);
			GameObject gameObject = this.FindPlayOnlineButton();
			if (gameObject != null)
			{
				ButtonRolloverHandler component = gameObject.GetComponent<ButtonRolloverHandler>();
				if (component != null)
				{
					component.SetDisabledColors();
				}
				gameObject.GetComponent<PassiveButton>().enabled = false;
			}
		}
		bool flag = Application.isEditor && result == Result.AlreadyConfigured;
		if (result != null && !flag)
		{
			throw new Exception("Failed to initialize platform: " + result.ToString());
		}
		LoggingInterface.SetLogLevel((LogCategory) int.MaxValue, LogLevel.Warning);
		LoggingInterface.SetCallback(new LogMessageFunc(this.HandleEosLogging));
		Options options = new Options
		{
			ProductId = this.productId,
			SandboxId = this.sandboxId,
			DeploymentId = this.deploymentId,
			Flags = (PlatformFlags) 2L,
			ClientCredentials = new ClientCredentials
			{
				ClientId = this.clientId,
				ClientSecret = this.clientSecret
			}
		};
		this.platformInterface = PlatformInterface.Create(options);
		if (this.platformInterface == null)
		{
			throw new Exception("Failed to create platform interface");
		}
		this.platformInterface.GetConnectInterface().AddNotifyAuthExpiration(new AddNotifyAuthExpirationOptions(), null, new OnAuthExpirationCallback(this.OnAuthExpirationCallback));
		this.platformInitialized = true;
	}

	private void OnAuthExpirationCallback(AuthExpirationCallbackInfo data)
	{
		this.authExpiredCallbackTriggered = true;
		this.AuthWithCorrectPlatformImpl();
	}

	private void HandleEosLogging(LogMessage msg)
	{
		LogLevel level = msg.Level;
		if (level == LogLevel.Error)
		{
			Debug.LogError(msg.Message);
			return;
		}
		if (level != LogLevel.Warning)
		{
			Debug.Log(msg.Message);
			return;
		}
		Debug.LogWarning(msg.Message);
	}

	public void LoginForKWS(bool allowOffline = true)
	{
		this.hasShownSigninScreen = false;
		if (!this.platformInitialized)
		{
			this.CloseStartupWaitScreen();
			DestroyableSingleton<AccountManager>.Instance.SetDLLErrorMode();
			DestroyableSingleton<AccountManager>.Instance.SignInFail(EOSManager.EOS_ERRORS.InterfaceInitFail, new Action(this.FinishLoginFlow));
			return;
		}
		this.BeginLoginFlow();
	}

	public void BeginLoginFlow()
	{
		CreateDeviceIdOptions createDeviceIdOptions = new CreateDeviceIdOptions
		{
			DeviceModel = SystemInfo.deviceUniqueIdentifier
		};
		this.platformInterface.GetConnectInterface().CreateDeviceId(createDeviceIdOptions, null, delegate(CreateDeviceIdCallbackInfo createDeviceIDCallbackInfo)
		{
			LoginOptions loginOptions = new LoginOptions
			{
				Credentials = new Credentials
				{
					Token = null,
					Type = ExternalCredentialType.DeviceidAccessToken
				},
				UserLoginInfo = new UserLoginInfo
				{
					DisplayName = SaveManager.PlayerName
				}
			};
			this.platformInterface.GetConnectInterface().Login(loginOptions, null, new OnLoginCallback(this.LogInToDeviceIDOnStartupCallback));
		});
	}

	private void LogInToDeviceIDOnStartupCallback(LoginCallbackInfo loginCallbackInfo)
	{
		if (loginCallbackInfo.ResultCode == null)
		{
			this.deviceIDuserID = loginCallbackInfo.LocalUserId;
			this.ReallyBeginFlow();
			return;
		}
		Debug.Log("Device ID Login Failure " + loginCallbackInfo.ResultCode.ToString());
		if (!this.hasTriedStartupDeviceID)
		{
			this.hasTriedStartupDeviceID = true;
			Epic.OnlineServices.Connect.CreateUserOptions createUserOptions = new Epic.OnlineServices.Connect.CreateUserOptions
			{
				ContinuanceToken = loginCallbackInfo.ContinuanceToken
			};
			this.platformInterface.GetConnectInterface().CreateUser(createUserOptions, null, delegate(Epic.OnlineServices.Connect.CreateUserCallbackInfo createUserCallback)
			{
				LoginOptions loginOptions = new LoginOptions
				{
					Credentials = new Credentials
					{
						Token = null,
						Type = ExternalCredentialType.DeviceidAccessToken
					},
					UserLoginInfo = new UserLoginInfo
					{
						DisplayName = SaveManager.PlayerName
					}
				};
				this.platformInterface.GetConnectInterface().Login(loginOptions, null, new OnLoginCallback(this.LogInToDeviceIDOnStartupCallback));
			});
			return;
		}
		this.ReallyBeginFlow();
	}

	public void ReallyBeginFlow()
	{
		this.AuthWithCorrectPlatformImpl();
	}

	private void PlatformAuthReturn(bool success)
	{
		this.CloseStartupWaitScreen();
		if (!this.authExpiredCallbackTriggered)
		{
			this.StartAgeGateQuery();
		}
		this.authExpiredCallbackTriggered = false;
	}

	private void ContinueInOfflineMode()
	{
		Debug.LogWarning("Continuing in offline mode");
		SaveManager.AccountLoginStatus = EOSManager.AccountLoginStatus.Offline;
		this.IsAllowedOnline(false);
		this.userId = new ProductUserId();
		this.FinishLoginFlow();
	}

	private void IsAllowedOnline(bool canOnline)
	{
		GameObject gameObject = this.FindPlayOnlineButton();
		if (gameObject == null)
		{
			return;
		}
		if (canOnline)
		{
			ButtonRolloverHandler component = gameObject.GetComponent<ButtonRolloverHandler>();
			if (component != null)
			{
				component.SetEnabledColors();
			}
			gameObject.GetComponent<PassiveButton>().enabled = true;
			return;
		}
		ButtonRolloverHandler component2 = gameObject.GetComponent<ButtonRolloverHandler>();
		if (component2 != null)
		{
			component2.SetDisabledColors();
		}
		gameObject.GetComponent<PassiveButton>().enabled = false;
	}

	private GameObject FindPlayOnlineButton()
	{
		return GameObject.Find("PlayOnlineButton");
	}

	private void StartAgeGateQuery()
	{
		QueryAgeGateOptions queryAgeGateOptions = new QueryAgeGateOptions();
		this.ShowCallbackWaitAnim();
		this.platformInterface.GetKWSInterface().QueryAgeGate(queryAgeGateOptions, null, new OnQueryAgeGateCallback(this.KWSQueryAgeGateCallback));
	}

	private void KWSQueryAgeGateCallback(QueryAgeGateCallbackInfo ageGateCallbackInfo)
	{
		this.HideCallbackWaitAnim();
		Debug.Log("country " + ageGateCallbackInfo.CountryCode);
		Debug.Log("consent " + ageGateCallbackInfo.AgeOfConsent.ToString());
		this.ageOfConsent = (int)ageGateCallbackInfo.AgeOfConsent;
		if (!SaveManager.GetLocalDoB() || SaveManager.BirthDateYear >= DateTime.Now.Year)
		{
			base.StartCoroutine(this.ShowAgePrompt());
			return;
		}
		this.CheckAgeAndLoginStatus();
	}

	private IEnumerator ShowAgePrompt()
	{
		yield return DestroyableSingleton<AccountManager>.Instance.ShowAgeGate();
		this.CheckAgeAndLoginStatus();
		yield break;
	}

	public void LoginFromAccountTab()
	{
		this.LoginWithCorrectPlatform();
	}

	public void LoginToGuestModeFromAccountTab()
	{
		this.LogInToDeviceIDForGuestMode();
	}

	public void LogOutOfGuestModeFromAccountTab()
	{
		this.ContinueInOfflineMode();
	}

	public void GoOfflineFromPermissionsWindow()
	{
		this.ContinueInOfflineMode();
	}

	private void LoginWithCorrectPlatform()
	{
		this.ShowCallbackWaitAnim();
		this.LoginWithCorrectPlatformImpl();
	}

	private void CheckAgeAndLoginStatus()
	{
		SaveManager.SaveLocalDoB(SaveManager.BirthDateYear, SaveManager.BirthDateMonth, SaveManager.BirthDateDay);
		switch (SaveManager.AccountLoginStatus)
		{
		case EOSManager.AccountLoginStatus.Offline:
			this.ShowSignInScreen();
			return;
		case EOSManager.AccountLoginStatus.Guest:
			this.LogInToDeviceIDForGuestMode();
			return;
		case EOSManager.AccountLoginStatus.LoggedIn:
			this.LoginWithCorrectPlatform();
			return;
		case EOSManager.AccountLoginStatus.WaitingForParent:
			DestroyableSingleton<AccountManager>.Instance.UpdateGuardianEmailDisplay();
			this.LoginWithCorrectPlatform();
			return;
		default:
			return;
		}
	}

	private void ShowSignInScreen()
	{
		if (!this.hasShownSigninScreen)
		{
			DestroyableSingleton<AccountManager>.Instance.signInGuestOffline.signInButton.OnClick.RemoveAllListeners();
			DestroyableSingleton<AccountManager>.Instance.signInGuestOffline.continueGuestButton.OnClick.RemoveAllListeners();
			DestroyableSingleton<AccountManager>.Instance.signInGuestOffline.continueOfflineButton.OnClick.RemoveAllListeners();
			this.hasShownSigninScreen = true;
			if (this.continuanceToken == null)
			{
				DestroyableSingleton<AccountManager>.Instance.signInGuestOffline.signInButton.OnClick.AddListener(new UnityAction(this.LoginWithCorrectPlatform));
			}
			else
			{
				DestroyableSingleton<AccountManager>.Instance.signInGuestOffline.signInButton.OnClick.AddListener(new UnityAction(this.CreateAccountWithPlatformAuth));
			}
			DestroyableSingleton<AccountManager>.Instance.signInGuestOffline.continueGuestButton.OnClick.AddListener(new UnityAction(this.LogInToDeviceIDForGuestMode));
			DestroyableSingleton<AccountManager>.Instance.signInGuestOffline.continueOfflineButton.OnClick.AddListener(new UnityAction(this.ContinueInOfflineMode));
			DestroyableSingleton<AccountManager>.Instance.signInGuestOffline.gameObject.SetActive(true);
			return;
		}
		if (this.continuanceToken == null)
		{
			this.LoginWithCorrectPlatform();
			return;
		}
		this.CreateAccountWithPlatformAuth();
	}

	public void CreateKWSUer()
	{
		if (!string.IsNullOrEmpty(SaveManager.GuardianEmail))
		{
			Epic.OnlineServices.KWS.CreateUserOptions createUserOptions = new Epic.OnlineServices.KWS.CreateUserOptions
			{
				LocalUserId = this.userId,
				DateOfBirth = string.Concat(new string[]
				{
					SaveManager.BirthDateYear.ToString(),
					"-",
					SaveManager.BirthDateMonth.ToString().PadLeft(2, '0'),
					"-",
					SaveManager.BirthDateDay.ToString().PadLeft(2, '0')
				}),
				ParentEmail = SaveManager.GuardianEmail
			};
			this.ShowCallbackWaitAnim();
			this.platformInterface.GetKWSInterface().CreateUser(createUserOptions, null, new Epic.OnlineServices.KWS.OnCreateUserCallback(this.CreateKWSUserCallback));
			return;
		}
		this.HideCallbackWaitAnim();
		DestroyableSingleton<AccountManager>.Instance.SignInFail(EOSManager.EOS_ERRORS.InvalidParentEmail, new Action(this.LogInToDeviceIDForGuestMode));
	}

	private void CreateKWSUserCallback(Epic.OnlineServices.KWS.CreateUserCallbackInfo createUserCallbackInfo)
	{
		this.UpdatePermissionKeys(false);
	}

	public void LogInToDeviceIDForGuestMode()
	{
		this.ShowCallbackWaitAnim();
		CreateDeviceIdOptions createDeviceIdOptions = new CreateDeviceIdOptions
		{
			DeviceModel = SystemInfo.deviceUniqueIdentifier
		};
		this.platformInterface.GetConnectInterface().CreateDeviceId(createDeviceIdOptions, null, delegate(CreateDeviceIdCallbackInfo createDeviceIDCallbackIngo)
		{
			LoginOptions loginOptions = new LoginOptions
			{
				Credentials = new Credentials
				{
					Token = null,
					Type = ExternalCredentialType.DeviceidAccessToken
				},
				UserLoginInfo = new UserLoginInfo
				{
					DisplayName = SaveManager.PlayerName
				}
			};
			this.platformInterface.GetConnectInterface().Login(loginOptions, null, new OnLoginCallback(this.EOSConnectDeviceIDLoginForGuestModeCallback));
		});
	}

	private void EOSConnectDeviceIDLoginForGuestModeCallback(LoginCallbackInfo loginCallbackInfo)
	{
		if (loginCallbackInfo.ResultCode == null)
		{
			this.deviceIDuserID = loginCallbackInfo.LocalUserId;
			SaveManager.AccountLoginStatus = EOSManager.AccountLoginStatus.Guest;
			this.FinishLoginFlow();
			return;
		}
		Debug.LogError(string.Format("EOS Device ID login callback - FAILURE: {0}", loginCallbackInfo.ResultCode));
		this.HideCallbackWaitAnim();
		DestroyableSingleton<AccountManager>.Instance.SignInFail(EOSManager.EOS_ERRORS.GuestModeAuthFail, new Action(this.ContinueInOfflineMode));
	}

	private void CheckKWSPermissionsOnPlatformLogin()
	{
		QueryPermissionsOptions queryPermissionsOptions = new QueryPermissionsOptions
		{
			LocalUserId = this.userId
		};
		this.ShowCallbackWaitAnim();
		this.platformInterface.GetKWSInterface().QueryPermissions(queryPermissionsOptions, null, new OnQueryPermissionsCallback(this.KWSQueryPermissionsOnPlatformLoginCallback));
	}

	private void KWSQueryPermissionsOnPlatformLoginCallback(QueryPermissionsCallbackInfo permissionsCallbackInfo)
	{
		this.HideCallbackWaitAnim();
		if (!string.IsNullOrEmpty(permissionsCallbackInfo.KWSUserId))
		{
			string[] array = permissionsCallbackInfo.DateOfBirth.Split(new char[]
			{
				'-'
			});
			int birthDateYear;
			int.TryParse(array[0], out birthDateYear);
			int birthDateMonth;
			int.TryParse(array[1], out birthDateMonth);
			int birthDateDay;
			int.TryParse(array[2], out birthDateDay);
			SaveManager.BirthDateYear = birthDateYear;
			SaveManager.BirthDateMonth = birthDateMonth;
			SaveManager.BirthDateDay = birthDateDay;
		}
		this.kwsUserId = permissionsCallbackInfo.KWSUserId;
		if (this.IsMinor())
		{
			Debug.Log("is minor");
			AddNotifyPermissionsUpdateReceivedOptions addNotifyPermissionsUpdateReceivedOptions = new AddNotifyPermissionsUpdateReceivedOptions();
			this.platformInterface.GetKWSInterface().AddNotifyPermissionsUpdateReceived(addNotifyPermissionsUpdateReceivedOptions, null, new OnPermissionsUpdateReceivedCallback(this.KWSPermissionsUpdatedCallback));
			if (string.IsNullOrEmpty(this.kwsUserId))
			{
				SaveManager.AccountLoginStatus = EOSManager.AccountLoginStatus.WaitingForParent;
				DestroyableSingleton<AccountManager>.Instance.ShowPermissionsRequestForm();
				return;
			}
			this.UpdatePermissionKeys(true);
			return;
		}
		else
		{
			Debug.Log("is adult");
			if (SaveManager.AccountLoginStatus != EOSManager.AccountLoginStatus.LoggedIn)
			{
				Debug.Log("weren't logged in before");
				SaveManager.AccountLoginStatus = EOSManager.AccountLoginStatus.LoggedIn;
				SaveManager.ChatModeType = QuickChatModes.FreeChatOrQuickChat;
				DestroyableSingleton<AccountManager>.Instance.SignInSuccess(new Action(this.FinishLoginFlow));
				return;
			}
			Debug.Log("were logged in");
			SaveManager.AccountLoginStatus = EOSManager.AccountLoginStatus.LoggedIn;
			this.FinishLoginFlow();
			return;
		}
	}

	private void KWSPermissionsUpdatedCallback(PermissionsUpdateReceivedCallbackInfo permissionsCallbackInfo)
	{
		if (SaveManager.AccountLoginStatus == EOSManager.AccountLoginStatus.LoggedIn || SaveManager.AccountLoginStatus == EOSManager.AccountLoginStatus.WaitingForParent)
		{
			this.UpdatePermissionKeys(true);
		}
	}

	private void FinishLoginFlow()
	{
		Debug.Log("finishing flow, login status " + SaveManager.AccountLoginStatus.ToString());
		if (SaveManager.AccountLoginStatus == EOSManager.AccountLoginStatus.Guest)
		{
			SaveManager.ChatModeType = QuickChatModes.QuickChatOnly;
			DestroyableSingleton<AccountManager>.Instance.CheckAndRegenerateName();
		}
		if (SaveManager.AccountLoginStatus != EOSManager.AccountLoginStatus.Offline)
		{
			this.IsAllowedOnline(true);
		}
		this.CloseStartupWaitScreen();
		this.HideCallbackWaitAnim();
		DestroyableSingleton<AccountManager>.Instance.UpdateVisuals();
		this.loginFlowFinished = true;
	}

	private void LinkToDeviceIDAndRegenDeviceID()
	{
		this.ShowCallbackWaitAnim();
		LinkAccountOptions linkAccountOptions = new LinkAccountOptions
		{
			ContinuanceToken = this.continuanceToken,
			LocalUserId = this.deviceIDuserID
		};
		this.platformInterface.GetConnectInterface().LinkAccount(linkAccountOptions, null, delegate(LinkAccountCallbackInfo linkAccountCallbackInfo)
		{
			Debug.Log("linked accounts");
			if (linkAccountCallbackInfo.ResultCode == null)
			{
				DeleteDeviceIdOptions deleteDeviceIdOptions = new DeleteDeviceIdOptions();
				this.platformInterface.GetConnectInterface().DeleteDeviceId(deleteDeviceIdOptions, null, delegate(DeleteDeviceIdCallbackInfo deleteDeviceIDCallbackInfo)
				{
					this.deviceIDuserID = new ProductUserId();
					this.LoginWithCorrectPlatform();
				});
				return;
			}
			Epic.OnlineServices.Connect.CreateUserOptions createUserOptions = new Epic.OnlineServices.Connect.CreateUserOptions
			{
				ContinuanceToken = this.continuanceToken
			};
			this.platformInterface.GetConnectInterface().CreateUser(createUserOptions, null, new Epic.OnlineServices.Connect.OnCreateUserCallback(this.EOSConnectCreateUserCallback));
		});
	}

	private void CreateAccountWithPlatformAuth()
	{
		this.ShowCallbackWaitAnim();
		if (this.continuanceToken == null)
		{
			this.RetryAuthAndLoginImpl();
			return;
		}
		this.LinkToDeviceIDAndRegenDeviceID();
	}

	private void EOSConnectCreateUserCallback(Epic.OnlineServices.Connect.CreateUserCallbackInfo createUserCallbackInfo)
	{
		if (createUserCallbackInfo.ResultCode == null)
		{
			this.userId = createUserCallbackInfo.LocalUserId;
			this.CheckKWSPermissionsOnPlatformLogin();
			return;
		}
		if (createUserCallbackInfo.ResultCode == Result.ConnectUserAlreadyExists)
		{
			Debug.Log("Account exists already - why are we here...just try to log in again");
			this.LoginWithCorrectPlatform();
			return;
		}
		Debug.LogError(string.Format("EOS Failed to create account: {0}", createUserCallbackInfo.ResultCode));
		this.HideCallbackWaitAnim();
		DestroyableSingleton<AccountManager>.Instance.SignInFail(EOSManager.EOS_ERRORS.AccountCreationFail, new Action(this.LogInToDeviceIDForGuestMode));
	}

	private void EOSConnectPlatformLoginCallback(LoginCallbackInfo loginCallbackInfo)
	{
		if (loginCallbackInfo.ResultCode == null)
		{
			Debug.Log("login is success");
			this.userId = loginCallbackInfo.LocalUserId;
			this.CheckKWSPermissionsOnPlatformLogin();
			return;
		}
		Debug.Log("EOS Connect platform Login Callback " + loginCallbackInfo.ResultCode.ToString());
		if (loginCallbackInfo.ResultCode == Result.ConnectExternalTokenValidationFailed || 
		    loginCallbackInfo.ResultCode == Result.TokenNotAccount || 
		    loginCallbackInfo.ResultCode == Result.AuthInvalidToken || 
		    loginCallbackInfo.ResultCode == Result.AuthInvalidPlatformToken || 
		    loginCallbackInfo.ResultCode == Result.AuthInvalidRefreshToken || 
		    loginCallbackInfo.ResultCode == Result.ConnectUnsupportedTokenType)
		{
			this.ClearAuthToken();
		}
		if (this.continuanceToken == null)
		{
			this.continuanceToken = loginCallbackInfo.ContinuanceToken;
		}
		this.ShowSignInScreen();
	}

	public void ClearAuthToken()
	{
		this.platformAuthToken = "";
		PlayerPrefs.DeleteKey("token");
	}

	private void UpdatePermissionKeys(bool isLoggingIn)
	{
		Debug.Log("KWS User ID " + this.kwsUserId);
		GetPermissionByKeyOptions getPermissionByKeyOptions = new GetPermissionByKeyOptions
		{
			LocalUserId = this.userId,
			Key = "freeChat"
		};
		Result permissionByKey = this.platformInterface.GetKWSInterface().GetPermissionByKey(getPermissionByKeyOptions, out DestroyableSingleton<AccountManager>.Instance.freeChatAllowed);
		if (permissionByKey != null)
		{
			Debug.Log("freeChat Update: " + permissionByKey.ToString());
		}
		GetPermissionByKeyOptions getPermissionByKeyOptions2 = new GetPermissionByKeyOptions
		{
			LocalUserId = this.userId,
			Key = "customDisplayName"
		};
		permissionByKey = this.platformInterface.GetKWSInterface().GetPermissionByKey(getPermissionByKeyOptions2, out DestroyableSingleton<AccountManager>.Instance.customDisplayName);
		if (permissionByKey != null)
		{
			Debug.Log("custom display name Update: " + permissionByKey.ToString());
		}
		DestroyableSingleton<AccountManager>.Instance.UpdateKidAccountDisplay();
		if (!DestroyableSingleton<AccountManager>.Instance.HasMinorsGuardianEverUpdatedAnything())
		{
			SaveManager.AccountLoginStatus = EOSManager.AccountLoginStatus.WaitingForParent;
			this.FinishLoginFlow();
			return;
		}
		if (SaveManager.AccountLoginStatus != EOSManager.AccountLoginStatus.LoggedIn && SaveManager.AccountLoginStatus != EOSManager.AccountLoginStatus.WaitingForParent)
		{
			SaveManager.AccountLoginStatus = EOSManager.AccountLoginStatus.LoggedIn;
			this.HideCallbackWaitAnim();
			if (this.IsFreechatAllowed())
			{
				SaveManager.ChatModeType = QuickChatModes.FreeChatOrQuickChat;
			}
			DestroyableSingleton<AccountManager>.Instance.SignInSuccess(new Action(this.FinishLoginFlow));
			return;
		}
		SaveManager.AccountLoginStatus = EOSManager.AccountLoginStatus.LoggedIn;
		this.FinishLoginFlow();
	}

	public void UpdateGuardianEmail()
	{
		UpdateParentEmailOptions updateParentEmailOptions = new UpdateParentEmailOptions
		{
			LocalUserId = this.userId,
			ParentEmail = SaveManager.GuardianEmail
		};
		this.platformInterface.GetKWSInterface().UpdateParentEmail(updateParentEmailOptions, null, new OnUpdateParentEmailCallback(this.UpdateGuardianEmailSettingsCallback));
	}

	private void UpdateGuardianEmailSettingsCallback(UpdateParentEmailCallbackInfo updateParentEmailCallbackInfo)
	{
		Debug.Log("Successfully updated guardian email");
		SaveManager.AccountLoginStatus = EOSManager.AccountLoginStatus.WaitingForParent;
		this.FinishLoginFlow();
	}

	public bool IsMinor()
	{
		DateTime t = new DateTime(SaveManager.BirthDateYear, SaveManager.BirthDateMonth, SaveManager.BirthDateDay);
		t = t.AddYears(this.ageOfConsent);
		return t > DateTime.UtcNow;
	}

	public bool IsFreechatAllowed()
	{
		if (SaveManager.AccountLoginStatus == EOSManager.AccountLoginStatus.LoggedIn)
		{
			if (!this.IsMinor())
			{
				return true;
			}
			if (this.IsMinor() && DestroyableSingleton<AccountManager>.Instance.freeChatAllowed == null)
			{
				return true;
			}
		}
		SaveManager.ChatModeType = QuickChatModes.QuickChatOnly;
		return false;
	}

	public IEnumerator WaitForLoginFlow()
	{
		while (!this.HasFinishedLoginFlow())
		{
			yield return null;
		}
		yield break;
	}

	public bool HasFinishedLoginFlow()
	{
		return this.loginFlowFinished;
	}

	public PlatformInterface GetPlatformInterface()
	{
		return this.platformInterface;
	}

	private void CloseStartupWaitScreen()
	{
		DestroyableSingleton<AccountManager>.Instance.waitingText.gameObject.SetActive(false);
	}

	private void ShowCallbackWaitAnim()
	{
		DestroyableSingleton<AccountManager>.Instance.postLoadWaiting.SetActive(true);
	}

	private void HideCallbackWaitAnim()
	{
		DestroyableSingleton<AccountManager>.Instance.postLoadWaiting.SetActive(false);
	}

	private void OnEnable()
	{
		bool flag = this.platformInitialized;
	}

	private new void OnDestroy()
	{
		if (!this.platformInitialized)
		{
			return;
		}
		this.DoShutdown();
	}

	private void Update()
	{
		if (!this.platformInitialized)
		{
			return;
		}
		if (this.platformInterface != null)
		{
			this.platformTickTimer += Time.deltaTime;
			if (this.platformTickTimer >= 0.1f)
			{
				this.platformTickTimer = 0f;
				this.platformInterface.Tick();
			}
		}
	}

	[ContextMenu("Shutdown")]
	private void DoShutdown()
	{
		if (this.platformInterface != null)
		{
			this.platformInterface.Release();
			this.platformInterface = null;
		}
		PlatformInterface.Shutdown();
	}

	[ContextMenu("Delete Device ID")]
	public void DeleteDeviceID()
	{
		SaveManager.BirthDateYear = 2021;
		SaveManager.GuardianEmail = "";
		SaveManager.AccountLoginStatus = EOSManager.AccountLoginStatus.Offline;
		SaveManager.SaveLocalDoB(2021, 1, 1);
		DeleteDeviceIdOptions deleteDeviceIdOptions = new DeleteDeviceIdOptions();
		if (this.platformInterface != null)
		{
			this.platformInterface.GetConnectInterface().DeleteDeviceId(deleteDeviceIdOptions, null, delegate(DeleteDeviceIdCallbackInfo data)
			{
				Debug.Log("Device ID Deleted");
			});
		}
	}

	public enum AccountLoginStatus
	{
		Offline,
		Guest,
		LoggedIn,
		WaitingForParent
	}

	public enum EOS_ERRORS
	{
		FailedEpicAuthToken,
		UnsupportedPlatform,
		LinkAccountFail,
		SteamworksAppTicketFail,
		SteamworksAuthFail,
		iOSAuthFail,
		GoogleAuthFail,
		GoogleAuthNoToken,
		NullContinuanceToken,
		MismatchedProductUserIDs,
		GenericLoginError,
		XboxUserAddError,
		XboxGetTokenError,
		AccountCreationFail,
		InterfaceInitFail,
		InvalidParentEmail,
		GuestModeAuthFail,
		PlatformNotSupported,
		NintendoAuthFailed
	}
}
