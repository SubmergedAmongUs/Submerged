using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
	public AdDataCollectScreen AdsPolicy;

	public AnnouncementPopUp Announcement;

	public GooglePlayAssetHandler googlePlayAssetHandler;

	[Header("Console Controller Navigation")]
	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	public List<PassiveButton> disableOnStartup = new List<PassiveButton>();

	public void Start()
	{
		ChatLanguageSet.Instance.Load();
		base.StartCoroutine(this.RunStartUp());
		ControllerManager.Instance.NewScene(base.name, null, this.DefaultButtonSelected, this.ControllerSelectable, false);
		QualitySettings.vSyncCount = (SaveManager.VSync ? 1 : 0);
	}

	private IEnumerator RunStartUp()
	{
		yield return this.googlePlayAssetHandler.PromptUser();
		yield return DestroyableSingleton<EOSManager>.Instance.RunLogin();
		base.StartCoroutine(this.Announcement.Init());
		yield return this.Announcement.Show();
		this.CheckAddOns();
		yield return null;
		yield break;
	}

	private void CheckAddOns()
	{
		DateTime utcNow = DateTime.UtcNow;
		for (int i = 0; i < DestroyableSingleton<HatManager>.Instance.AllHats.Count; i++)
		{
			HatBehaviour hatBehaviour = DestroyableSingleton<HatManager>.Instance.AllHats[i];
			if (!hatBehaviour.ProdId.StartsWith("pet_") && (hatBehaviour.LimitedMonth == utcNow.Month || hatBehaviour.LimitedMonth == 0) && (hatBehaviour.LimitedYear == utcNow.Year || hatBehaviour.LimitedYear == 0) && !hatBehaviour.NotInStore && !HatManager.IsMapStuff(hatBehaviour.ProdId) && !SaveManager.GetPurchase(hatBehaviour.ProductId))
			{
				SaveManager.SetPurchased(hatBehaviour.ProductId);
			}
		}
	}

	public void ShowAnnouncementPopUp()
	{
		if (this.Announcement != null)
		{
			this.Announcement.gameObject.SetActive(true);
		}
	}

	private void LateUpdate()
	{
		if (!ControllerManager.Instance.IsUiControllerActive && DestroyableSingleton<EOSManager>.Instance.HasFinishedLoginFlow())
		{
			Debug.Log("No current valid menu active, opening main menu scene");
			ControllerManager.Instance.NewScene(base.name, null, this.DefaultButtonSelected, this.ControllerSelectable, false);
			ControllerManager.Instance.CurrentUiState.CurrentSelection = this.DefaultButtonSelected;
		}
	}
}
