using System;
using System.Collections.Generic;

public class QuickChatSentenceVariant
{
	private StringNames[] requiredKeysInSlots;

	public string value;

	public QuickChatSentenceVariant(StringNames[] Keys, string Value)
	{
		this.requiredKeysInSlots = new StringNames[Keys.Length - 1];
		for (int i = 0; i < this.requiredKeysInSlots.Length; i++)
		{
			this.requiredKeysInSlots[i] = Keys[i + 1];
		}
		this.value = Value;
	}

	public bool ShouldUse(List<StringNames> currentKeys)
	{
		if (this.requiredKeysInSlots.Length != currentKeys.Count)
		{
			return false;
		}
		for (int i = 0; i < this.requiredKeysInSlots.Length; i++)
		{
			if (this.requiredKeysInSlots[i] != StringNames.ANY && this.requiredKeysInSlots[i] != currentKeys[i] && (this.requiredKeysInSlots[i] != StringNames.QCCrewMe || currentKeys[i] != StringNames.QCCrewI))
			{
				return false;
			}
		}
		return true;
	}
}
