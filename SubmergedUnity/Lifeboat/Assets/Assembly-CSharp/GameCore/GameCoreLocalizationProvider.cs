using System;

namespace GameCore
{
	public class GameCoreLocalizationProvider : LocalizationProvider
	{
		public override string GetLocalizedText(LocalizationKeys key)
		{
			StringNames id = StringNames.NoTranslation;
			switch (key)
			{
			case LocalizationKeys.SaveGameOutOfSpaceMessage:
				id = StringNames.SaveGameOutOfSpaceMessage;
				break;
			case LocalizationKeys.SaveGameOutOfSpaceConfirm:
				id = StringNames.SaveGameOutOfSpaceConfirm;
				break;
			case LocalizationKeys.SaveGameOutOfSpaceCancel:
				id = StringNames.SaveGameOutOfSpaceCancel;
				break;
			}
			return DestroyableSingleton<TranslationController>.Instance.GetString(id, Array.Empty<object>());
		}
	}
}
