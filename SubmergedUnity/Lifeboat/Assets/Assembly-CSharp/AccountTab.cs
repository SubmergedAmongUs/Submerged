using System;
using System.Collections.Generic;
using InnerNet;
using TMPro;
using UnityEngine;

public class AccountTab : MonoBehaviour
{
	public TextMeshPro userName;

	public PoolablePlayer playerImage;

	public TextMeshPro quickChatInstructions;

	public GameObject guestMode;

	public GameObject offlineMode;

	public FullAccount loggedInMode;

	public GameObject waitingForGuardian;

	public TextMeshPro guardianEmailText;

	public EditName editNameScreen;

	public GameObject idCard;

	public SpriteRenderer actualTabSprite;

	public GameObject resendEmailButton;

	public GameObject signIntoAccountButton;

	public GameObject askForGuardianEmailButton;

	public TextMeshPro veryBadErrorText;

	public Collider2D clickToCloseCollider;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public List<UiElement> PotentialDefaultSelections;

	public List<UiElement> selectableObjects;

	private UiElement DefaultSelection
	{
		get
		{
			foreach (UiElement uiElement in this.PotentialDefaultSelections)
			{
				if (uiElement.isActiveAndEnabled)
				{
					return uiElement;
				}
			}
			return null;
		}
	}

	public void TurnAllSectionsOff()
	{
		this.loggedInMode.gameObject.SetActive(false);
		this.guestMode.SetActive(false);
		this.waitingForGuardian.SetActive(false);
		this.veryBadErrorText.gameObject.SetActive(false);
		this.offlineMode.gameObject.SetActive(false);
	}

	public void UpdateKidAccountCanChangeName()
	{
		this.loggedInMode.CanSetCustomName(DestroyableSingleton<AccountManager>.Instance.CanMinorSetCustomDisplayName());
	}

	public void SignIn()
	{
		DestroyableSingleton<EOSManager>.Instance.LoginFromAccountTab();
	}

	public void SignInToGuestMode()
	{
		DestroyableSingleton<EOSManager>.Instance.LoginToGuestModeFromAccountTab();
	}

	public void LogOutOfGuestMode()
	{
		DestroyableSingleton<EOSManager>.Instance.LogOutOfGuestModeFromAccountTab();
	}

	public void RandomizeName()
	{
		DestroyableSingleton<AccountManager>.Instance.RandomizeName();
	}

	public void UpdateNameDisplay()
	{
		this.userName.text = SaveManager.PlayerName;
		this.playerImage.UpdateFromSaveManager();
	}

	public void ChangeName()
	{
		this.editNameScreen.gameObject.SetActive(true);
	}

	public void ResendEmail()
	{
		DestroyableSingleton<EOSManager>.Instance.UpdateGuardianEmail();
		DestroyableSingleton<AccountManager>.Instance.ShowGuardianEmailSentConfirm();
	}

	public void EditGuardianEmail()
	{
		base.StartCoroutine(DestroyableSingleton<AccountManager>.Instance.EditGuardianEmail());
	}

	public void UpdateGuardianEmailText()
	{
		this.guardianEmailText.text = SaveManager.GuardianEmail;
		this.resendEmailButton.gameObject.SetActive(!string.IsNullOrEmpty(SaveManager.GuardianEmail));
	}

	public void SetDLLErrorMode()
	{
		this.TurnAllSectionsOff();
		DestroyableSingleton<AccountManager>.Instance.RandomizeName();
		this.idCard.SetActive(true);
		this.veryBadErrorText.gameObject.SetActive(true);
		this.veryBadErrorText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DLLNotFoundAccountError, Array.Empty<object>());
	}

	public void UpdateVisuals()
	{
		this.TurnAllSectionsOff();
		this.UpdateNameDisplay();
		if (SaveManager.ChatModeType == QuickChatModes.FreeChatOrQuickChat)
		{
			this.quickChatInstructions.text = "";
		}
		if (string.IsNullOrEmpty(this.userName.text))
		{
			DestroyableSingleton<AccountManager>.Instance.RandomizeName();
		}
		switch (SaveManager.AccountLoginStatus)
		{
		case EOSManager.AccountLoginStatus.Offline:
			Debug.Log("*******************updating to OFFLINE MODE");
			this.actualTabSprite.color = Color.grey;
			this.idCard.SetActive(true);
			this.offlineMode.gameObject.SetActive(true);
			if (DestroyableSingleton<EOSManager>.Instance.GetPlatformInterface() == null)
			{
				this.SetDLLErrorMode();
				return;
			}
			break;
		case EOSManager.AccountLoginStatus.Guest:
			Debug.Log("*******************updating to GUEST MODE");
			this.actualTabSprite.color = Color.yellow;
			if (SaveManager.ChatModeType == QuickChatModes.QuickChatOnly)
			{
				this.quickChatInstructions.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.QuickChatInstructionsStart, Array.Empty<object>()) + " " + DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.QuickChatInstructionsGuest, Array.Empty<object>());
				this.resendEmailButton.gameObject.SetActive(!string.IsNullOrEmpty(SaveManager.GuardianEmail));
			}
			this.idCard.SetActive(true);
			this.guestMode.gameObject.SetActive(true);
			return;
		case EOSManager.AccountLoginStatus.LoggedIn:
			Debug.Log("*******************updating to LOGGED IN MODE");
			this.actualTabSprite.color = Color.green;
			if (SaveManager.ChatModeType == QuickChatModes.QuickChatOnly)
			{
				this.quickChatInstructions.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.QuickChatInstructionsStart, Array.Empty<object>()) + " ";
				if (DestroyableSingleton<EOSManager>.Instance.IsMinor() && !DestroyableSingleton<EOSManager>.Instance.IsFreechatAllowed())
				{
					TextMeshPro textMeshPro = this.quickChatInstructions;
					textMeshPro.text += DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.QuickChatInstructionsChild, Array.Empty<object>());
				}
				else
				{
					TextMeshPro textMeshPro2 = this.quickChatInstructions;
					textMeshPro2.text += DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.QuickChatInstructionsFull, Array.Empty<object>());
				}
				this.resendEmailButton.gameObject.SetActive(!string.IsNullOrEmpty(SaveManager.GuardianEmail));
			}
			this.idCard.SetActive(true);
			this.loggedInMode.gameObject.SetActive(true);
			this.loggedInMode.UpdateLoggedInAccountVisuals();
			return;
		case EOSManager.AccountLoginStatus.WaitingForParent:
			Debug.Log("*******************updating to WAITING FOR PARENT MODE");
			this.actualTabSprite.color = Color.cyan;
			this.guardianEmailText.text = (string.IsNullOrEmpty(SaveManager.GuardianEmail) ? DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GuardianEmail, Array.Empty<object>()) : SaveManager.GuardianEmail);
			this.idCard.SetActive(false);
			this.waitingForGuardian.gameObject.SetActive(true);
			break;
		default:
			return;
		}
	}

	public void Toggle()
	{
		if (base.GetComponent<SlideOpen>().isOpen)
		{
			this.Close();
			return;
		}
		this.Open();
	}

	public void Close()
	{
		this.clickToCloseCollider.enabled = false;
		base.GetComponent<SlideOpen>().Close();
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	public void Open()
	{
		this.clickToCloseCollider.enabled = true;
		this.UpdateVisuals();
		base.GetComponent<SlideOpen>().Open();
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton, this.DefaultSelection, this.selectableObjects, false);
	}
}
