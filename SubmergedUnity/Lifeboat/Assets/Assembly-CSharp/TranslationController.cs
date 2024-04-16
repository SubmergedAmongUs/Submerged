using System;
using System.Collections.Generic;
using System.Linq;
using Steamworks;
using UnityEngine;

public class TranslationController : DestroyableSingleton<TranslationController>
{
	private static readonly StringNames[] SystemTypesToStringNames = SystemTypeHelpers.AllTypes.Select(delegate(SystemTypes t)
	{
		StringNames result;
		Enum.TryParse<StringNames>(t.ToString(), out result);
		return result;
	}).ToArray<StringNames>();

	private static readonly StringNames[] TaskTypesToStringNames = TaskTypesHelpers.AllTypes.Select(delegate(TaskTypes t)
	{
		StringNames result;
		Enum.TryParse<StringNames>(t.ToString(), out result);
		return result;
	}).ToArray<StringNames>();

	public TranslatedImageSet[] Languages;

	public LanguageUnit CurrentLanguage;

	public LanguageUnit FallbackLanguage;

	public TranslatedImageSet FallbackLanguageImageSet;

	public List<ITranslatedText> ActiveTexts = new List<ITranslatedText>();

	public override void Awake()
	{
		base.Awake();
		if (DestroyableSingleton<TranslationController>.Instance == this)
		{
			this.CurrentLanguage = new LanguageUnit(this.Languages[(int)SaveManager.LastLanguage]);
		}
		this.FallbackLanguage = new LanguageUnit(this.FallbackLanguageImageSet);
	}

	public void SetLanguage(TranslatedImageSet lang)
	{
		int num = this.Languages.IndexOf(lang);
		Debug.Log("Set language to " + num.ToString());
		SaveManager.LastLanguage = (uint)num;
		this.CurrentLanguage = new LanguageUnit(this.Languages[num]);
		for (int i = 0; i < this.ActiveTexts.Count; i++)
		{
			this.ActiveTexts[i].ResetText();
		}
	}

	public Sprite GetImage(ImageNames id)
	{
		return this.CurrentLanguage.GetImage(id) ?? this.FallbackLanguage.GetImage(id);
	}

	public string GetString(StringNames id, params object[] parts)
	{
		return this.CurrentLanguage.GetString(id, "", parts);
	}

	public string GetStringWithDefault(StringNames id, string defaultStr, params object[] parts)
	{
		return this.CurrentLanguage.GetString(id, defaultStr, parts);
	}

	public string GetString(SystemTypes room)
	{
		StringNames stringNames = TranslationController.SystemTypesToStringNames[(int)room];
		if (stringNames == StringNames.ExitButton)
		{
			return "STRMISS";
		}
		return this.GetString(stringNames, Array.Empty<object>());
	}

	public string GetString(TaskTypes task)
	{
		return this.GetString(TranslationController.TaskTypesToStringNames[(int)((byte)task)], Array.Empty<object>());
	}

	public string GetMonthStringViaNumber(int monthNum)
	{
		StringNames id = StringNames.January + monthNum - 1;
		return DestroyableSingleton<TranslationController>.Instance.GetString(id, Array.Empty<object>());
	}

	public StringNames GetTaskName(TaskTypes task)
	{
		return TranslationController.TaskTypesToStringNames[(int)((byte)task)];
	}

	public StringNames GetSystemName(SystemTypes room)
	{
		return TranslationController.SystemTypesToStringNames[(int)room];
	}

	public string GetFITBVariant(StringNames id, List<StringNames> entryIDs)
	{
		return this.CurrentLanguage.GetFITBVariant(id, entryIDs);
	}

	internal static uint SelectDefaultLanguage()
	{
		SupportedLangs result;
		if (Enum.TryParse<SupportedLangs>(SteamApps.GetCurrentGameLanguage(), true, out result))
		{
			return (uint)result;
		}
		return 0U;
	}
}
