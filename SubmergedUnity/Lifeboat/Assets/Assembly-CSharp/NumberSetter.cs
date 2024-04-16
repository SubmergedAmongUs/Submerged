using System;
using System.Collections.Generic;
using UnityEngine;

public class NumberSetter : MonoBehaviour
{
	public NumberButton ButtonPrefab;

	public Scroller ButtonParent;

	public float ButtonStart = 0.5f;

	public float ButtonHeight = 0.5f;

	private NumberButton[] AllButtons;

	public NumberMenu parent;

	public NumberSetter dayNumberSetter;

	public NumberSetter.DateType dateType = NumberSetter.DateType.MONTHS;

	private List<UiElement> selectableObjects = new List<UiElement>();

	public UiElement backButton;

	private void OnEnable()
	{
		if (this.AllButtons != null)
		{
			this.selectableObjects.Clear();
			foreach (NumberButton numberButton in this.AllButtons)
			{
				this.selectableObjects.Add(numberButton.Button);
			}
			if (base.isActiveAndEnabled)
			{
				ControllerManager.Instance.OpenOverlayMenu(base.gameObject.name, this.backButton, this.AllButtons[0].Button, this.selectableObjects, false);
			}
		}
	}

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.gameObject.name);
	}

	public void Start()
	{
		Collider2D component = this.ButtonParent.GetComponent<Collider2D>();
		NumberButton[] allButtons = this.AllButtons;
		string[] array = new string[0];
		switch (this.dateType)
		{
		case NumberSetter.DateType.DAYS:
		{
			int num = DateTime.DaysInMonth(SaveManager.BirthDateYear, SaveManager.BirthDateMonth);
			array = new string[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = (i + 1).ToString();
			}
			break;
		}
		case NumberSetter.DateType.YEARS:
		{
			int year = DateTime.Now.Year;
			array = new string[year - 1900];
			for (int j = year; j > 1900; j--)
			{
				array[year - j] = j.ToString();
			}
			break;
		}
		case NumberSetter.DateType.MONTHS:
			array = new string[12];
			for (int k = 0; k < 12; k++)
			{
				array[k] = (k + 1).ToString();
			}
			break;
		}
		Vector3 localPosition = new Vector3(0f, this.ButtonStart, -0.5f);
		this.AllButtons = new NumberButton[array.Length];
		for (int l = 0; l < array.Length; l++)
		{
			NumberButton button = UnityEngine.Object.Instantiate<NumberButton>(this.ButtonPrefab, this.ButtonParent.Inner);
			this.AllButtons[l] = button;
			if (this.dateType == NumberSetter.DateType.MONTHS)
			{
				button.Text.text = DestroyableSingleton<TranslationController>.Instance.GetMonthStringViaNumber(l + 1);
			}
			else
			{
				button.Text.text = array[l];
			}
			button.Button.OnClick.AddListener(delegate()
			{
				this.SetData(button);
			});
			button.Button.ClickMask = component;
			switch (this.dateType)
			{
			case NumberSetter.DateType.DAYS:
				if (array[l] == SaveManager.BirthDateDay.ToString())
				{
					button.Text.color = Color.green;
				}
				break;
			case NumberSetter.DateType.YEARS:
				if (array[l] == SaveManager.BirthDateYear.ToString())
				{
					button.Text.color = Color.green;
				}
				break;
			case NumberSetter.DateType.MONTHS:
				button.monthNum = l + 1;
				if (button.monthNum == SaveManager.BirthDateMonth)
				{
					button.Text.color = Color.green;
				}
				break;
			}
			button.transform.localPosition = localPosition;
			localPosition.y -= this.ButtonHeight;
		}
		this.ButtonParent.YBounds.max = (float)array.Length * this.ButtonHeight - 2f * this.ButtonStart - 0.1f;
		this.OnEnable();
	}

	public void SetData(NumberButton selected)
	{
		this.parent.SetValue(selected.Text.text);
		for (int i = 0; i < this.AllButtons.Length; i++)
		{
			this.AllButtons[i].Text.color = Color.white;
		}
		selected.Text.color = Color.green;
		switch (this.dateType)
		{
		case NumberSetter.DateType.DAYS:
			SaveManager.BirthDateDay = int.Parse(selected.Text.text);
			break;
		case NumberSetter.DateType.YEARS:
			SaveManager.BirthDateYear = int.Parse(selected.Text.text);
			this.dayNumberSetter.UpdateDays();
			break;
		case NumberSetter.DateType.MONTHS:
			SaveManager.BirthDateMonth = selected.monthNum;
			this.dayNumberSetter.UpdateDays();
			break;
		}
		this.parent.Close();
	}

	public void UpdateDays()
	{
		if (this.AllButtons != null)
		{
			foreach (NumberButton numberButton in this.AllButtons)
			{
				if (numberButton)
				{
					 UnityEngine.Object.Destroy(numberButton.gameObject);
				}
			}
			this.Start();
		}
		int num = DateTime.DaysInMonth(SaveManager.BirthDateYear, SaveManager.BirthDateMonth);
		SaveManager.BirthDateDay = Mathf.Clamp(SaveManager.BirthDateDay, 1, num);
		this.parent.text.text = SaveManager.BirthDateDay.ToString();
	}

	public enum DateType
	{
		DAYS,
		YEARS,
		MONTHS
	}
}
