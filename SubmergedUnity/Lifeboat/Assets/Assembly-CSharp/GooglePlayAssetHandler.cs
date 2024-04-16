using System;
using System.Collections;
using System.Collections.Generic;
using PowerTools;
using TMPro;
using UnityEngine;

public class GooglePlayAssetHandler : MonoBehaviour
{
	public TextMeshPro text;

	public GameObject popup;

	public GameObject confirmPopUp;

	public TextMeshPro downloadSizeText;

	public GameObject errorPopUp;

	public TextMeshPro errorText;

	public HorizontalGauge Gauge;

	public SpriteRenderer Runner;

	public SpriteAnim LeftFolder;

	public SpriteAnim RightFolder;

	public AnimationClip FolderOpen;

	public AnimationClip FolderClose;

	public bool downloading;

	private string downloadError;

	private long spaceNeeded;

	private List<string> packsToDownload;

	private GooglePlayAssetHandler.ConfirmationStatus confirmationStatus;

	private GooglePlayAssetHandler.ErrorConfirmationStatus errorConfirmationStatus;

	public IEnumerator PromptUser()
	{
		yield return null;
		yield break;
	}

	public IEnumerator DownloadAssetPacks()
	{
		yield return null;
		yield break;
	}

	public IEnumerator ShowError()
	{
		this.errorPopUp.gameObject.SetActive(true);
		this.errorText.text = this.downloadError;
		while (this.errorConfirmationStatus == GooglePlayAssetHandler.ErrorConfirmationStatus.Waiting)
		{
			yield return null;
		}
		this.errorPopUp.gameObject.SetActive(false);
		GooglePlayAssetHandler.ErrorConfirmationStatus errorConfirmationStatus = this.errorConfirmationStatus;
		if (errorConfirmationStatus != GooglePlayAssetHandler.ErrorConfirmationStatus.Retry)
		{
			if (errorConfirmationStatus == GooglePlayAssetHandler.ErrorConfirmationStatus.Decline)
			{
				this.confirmationStatus = GooglePlayAssetHandler.ConfirmationStatus.Decline;
			}
		}
		else
		{
			this.downloadError = string.Empty;
			yield return this.DownloadAssetPacks();
		}
		yield return null;
		yield break;
	}

	private IEnumerator LoadAssetPackCoroutine(string assetPackName)
	{
		yield return null;
		yield break;
	}

	public IEnumerator GetDownloadSize(List<string> packs)
	{
		yield return null;
		yield break;
	}

	private IEnumerator DoRun()
	{
		while (downloading)
		{
			LeftFolder.Play(this.FolderOpen);
			var pos = this.Runner.transform.localPosition;
			yield return new WaitForLerp(1.125f, f =>
			{
				pos.x = Mathf.Lerp(-1.25f, 0.5625f, f);
				this.Runner.transform.localPosition = pos;
			});
			this.LeftFolder.Play(this.FolderClose);
			this.RightFolder.Play(this.FolderOpen);
			yield return new WaitForLerp(1.375f, t => 
			{ 
				pos.x = Mathf.Lerp(0.5625f, 1.25f, t);
		 		Runner.transform.localPosition = pos;
		 	});
			
			yield return new WaitForAnimationFinish(this.RightFolder, this.FolderClose);
		}
	}

	public bool RejectedDownload()
	{
		return this.confirmationStatus == GooglePlayAssetHandler.ConfirmationStatus.Decline;
	}

	public void ClickDownloadButton(bool accept)
	{
		this.confirmationStatus = (accept ? GooglePlayAssetHandler.ConfirmationStatus.Accept : GooglePlayAssetHandler.ConfirmationStatus.Decline);
	}

	public void CloseErrorPopUp(bool retry)
	{
		this.errorConfirmationStatus = (retry ? GooglePlayAssetHandler.ErrorConfirmationStatus.Retry : GooglePlayAssetHandler.ErrorConfirmationStatus.Decline);
	}

	private enum ConfirmationStatus
	{
		Waiting,
		Accept,
		Decline
	}

	private enum ErrorConfirmationStatus
	{
		Waiting,
		Retry,
		Decline
	}
}
