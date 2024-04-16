using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class AgeGateScreen : MonoBehaviour
{
	public TextMeshPro monthText;

	public TextMeshPro dayText;

	public TextMeshPro yearText;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultSelection;

	public List<UiElement> selectableObjects;

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	public IEnumerator Show()
	{
		this.monthText.text = DestroyableSingleton<TranslationController>.Instance.GetMonthStringViaNumber(SaveManager.BirthDateMonth);
		this.dayText.text = SaveManager.BirthDateDay.ToString();
		this.yearText.text = SaveManager.BirthDateYear.ToString();
		base.gameObject.SetActive(true);
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton, this.DefaultSelection, this.selectableObjects, false);
		while (base.gameObject.activeSelf)
		{
			yield return null;
		}
		yield break;
	}

	public void Close()
	{
		if (this.ShakeIfInvalid())
		{
			return;
		}
		SaveManager.BirthDateSetDate = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
		base.GetComponent<TransitionOpen>().Close();
	}

	public bool ShakeIfInvalid()
	{
		if (this.yearText.text == DateTime.UtcNow.Year.ToString())
		{
			base.StartCoroutine(Effects.SwayX(base.transform, 0.75f, 0.25f));
			return true;
		}
		return false;
	}
}
