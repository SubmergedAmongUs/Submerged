using System;
using System.Collections;
using Epic.OnlineServices.KWS;
using InnerNet;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class AccountManager : DestroyableSingleton<AccountManager>
{
	[SerializeField]
	private AccountTab accountTab;

	[SerializeField]
	private PermissionsRequest enterGuardianEmailWindow;

	[SerializeField]
	private UpdateGuardianEmail updateGuardianEmailWindow;

	[SerializeField]
	private InfoTextBox guardianEmailConfirmWindow;

	[SerializeField]
	private InfoTextBox genericInfoDisplayBox;

	[SerializeField]
	private AgeGateScreen enterDateOfBirthScreen;

	public GameObject waitingText;

	public GameObject postLoadWaiting;

	public GameObject privacyPolicyBg;

	public SignInGuestOfflineChoice signInGuestOffline;

	public PrivacyPolicyScreen PrivacyPolicy;

	private ChatModeCycle chatModeMenuScreen;

	public KWSPermissionStatus freeChatAllowed = KWSPermissionStatus.Rejected;

	public KWSPermissionStatus customDisplayName = KWSPermissionStatus.Rejected;

	public Action OnLoggedInStatusChange;

	private EOSManager.AccountLoginStatus prevLoggedInStatus = EOSManager.AccountLoginStatus.Guest;

	public void UpdateKidAccountDisplay()
	{
		this.accountTab.UpdateKidAccountCanChangeName();
		if (!DestroyableSingleton<EOSManager>.Instance.IsFreechatAllowed())
		{
			SaveManager.ChatModeType = QuickChatModes.QuickChatOnly;
		}
		if (this.chatModeMenuScreen != null)
		{
			this.chatModeMenuScreen.UpdateDisplay();
		}
	}

	public void SetChatModeButtonForUpdates(ChatModeCycle modeCycle)
	{
		this.chatModeMenuScreen = modeCycle;
	}

	public void UpdateAccountInfoDisplays()
	{
		this.accountTab.UpdateNameDisplay();
	}

	public void ShowGuardianEmailSentConfirm()
	{
		this.guardianEmailConfirmWindow.gameObject.SetActive(true);
	}

	public void ShowPermissionsRequestForm()
	{
		this.enterGuardianEmailWindow.Show();
	}

	public IEnumerator EditGuardianEmail()
	{
		yield return this.updateGuardianEmailWindow.Show();
		this.UpdateGuardianEmailDisplay();
		yield break;
	}

	public void UpdateGuardianEmailDisplay()
	{
		this.accountTab.UpdateGuardianEmailText();
	}

	public IEnumerator ShowAgeGate()
	{
		yield return this.enterDateOfBirthScreen.Show();
		yield break;
	}

	public void CheckAndRegenerateName()
	{
		if (!this.ValidateRandomName())
		{
			this.RandomizeName();
		}
	}

	public bool CanMinorSetCustomDisplayName()
	{
		return this.customDisplayName == null && SaveManager.AccountLoginStatus == EOSManager.AccountLoginStatus.LoggedIn;
	}

	public bool HasMinorsGuardianEverUpdatedAnything()
	{
		return this.freeChatAllowed != KWSPermissionStatus.Pending || this.customDisplayName != KWSPermissionStatus.Pending;
	}

	public bool HasGuardianRejectedEverything()
	{
		return this.freeChatAllowed == KWSPermissionStatus.Rejected && this.customDisplayName == KWSPermissionStatus.Rejected;
	}

	public void RandomizeName()
	{
		SaveManager.PlayerName = this.GetRandomName();
		this.accountTab.UpdateNameDisplay();
	}

	public string GetRandomName()
	{
		string name;
		do
		{
			name = base.GetComponent<RandomNameGenerator>().GetName();
		}
		while (name.Length > 10 || BlockedWords.ContainsWord(name));
		return name;
	}

	private bool ValidateRandomName()
	{
		return base.GetComponent<RandomNameGenerator>().ValidateName(SaveManager.PlayerName);
	}

	public override void Awake()
	{
		base.Awake();
		SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(this.OnSceneLoaded);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		SceneManager.sceneLoaded -= new UnityAction<Scene, LoadSceneMode>(this.OnSceneLoaded);
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		this.accountTab.gameObject.SetActive(scene.name == "MainMenu" || scene.name == "MatchMaking" || scene.name == "MMOnline");
	}

	public void SignInSuccess(Action callback)
	{
		this.genericInfoDisplayBox.SetOneButton();
		this.genericInfoDisplayBox.titleTexxt.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Success, Array.Empty<object>());
		this.genericInfoDisplayBox.bodyText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SuccessLogIn, Array.Empty<object>());
		this.genericInfoDisplayBox.button1Text.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Close, Array.Empty<object>());
		this.genericInfoDisplayBox.button1.OnClick.RemoveAllListeners();
		this.genericInfoDisplayBox.button1.OnClick.AddListener(delegate()
		{
			callback();
		});
		this.genericInfoDisplayBox.gameObject.SetActive(true);
		if (this.signInGuestOffline.gameObject.activeSelf)
		{
			this.signInGuestOffline.gameObject.SetActive(false);
		}
	}

	public void SignInFail(EOSManager.EOS_ERRORS error, Action callback)
	{
		this.genericInfoDisplayBox.SetOneButton();
		this.genericInfoDisplayBox.titleTexxt.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Failed, Array.Empty<object>());
		this.genericInfoDisplayBox.bodyText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorLogIn, Array.Empty<object>()) + string.Format("\n(Error {0})", error);
		this.genericInfoDisplayBox.button1Text.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Close, Array.Empty<object>());
		this.genericInfoDisplayBox.gameObject.SetActive(true);
		this.genericInfoDisplayBox.button1.OnClick.RemoveAllListeners();
		this.genericInfoDisplayBox.button1.OnClick.AddListener(delegate()
		{
			callback();
		});
	}

	public void SetDLLErrorMode()
	{
		SaveManager.AccountLoginStatus = EOSManager.AccountLoginStatus.Offline;
		SaveManager.ChatModeType = QuickChatModes.QuickChatOnly;
		this.accountTab.SetDLLErrorMode();
	}

	public void UpdateVisuals()
	{
		if (this.prevLoggedInStatus != SaveManager.AccountLoginStatus)
		{
			Action onLoggedInStatusChange = this.OnLoggedInStatusChange;
			if (onLoggedInStatusChange != null)
			{
				onLoggedInStatusChange();
			}
		}
		this.prevLoggedInStatus = SaveManager.AccountLoginStatus;
		this.accountTab.UpdateVisuals();
	}

	public bool CanPlayOnline()
	{
		return !(DestroyableSingleton<EOSManager>.Instance == null) && SaveManager.AccountLoginStatus != EOSManager.AccountLoginStatus.Offline && !(DestroyableSingleton<EOSManager>.Instance.GetPlatformInterface() == null);
	}
}
