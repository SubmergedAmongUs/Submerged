using System;
using System.Collections.Generic;
using UnityEngine;

public class QuickChatSentenceVariantSet
{
	public StringNames baseToken;

	private List<QuickChatSentenceVariant> variants = new List<QuickChatSentenceVariant>();

	public QuickChatSentenceVariant GetMatchingVariant(List<StringNames> currentKeys)
	{
		for (int i = this.variants.Count - 1; i >= 0; i--)
		{
			QuickChatSentenceVariant quickChatSentenceVariant = this.variants[i];
			if (quickChatSentenceVariant.ShouldUse(currentKeys))
			{
				return quickChatSentenceVariant;
			}
		}
		return null;
	}

	public void AddVariant(StringNames[] keys, string value, bool hasAny)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			Debug.LogWarning("Attempting to add variant for " + this.baseToken.ToString() + " with an empty string");
			return;
		}
		QuickChatSentenceVariant item = new QuickChatSentenceVariant(keys, value);
		if (hasAny)
		{
			this.variants.Insert(0, item);
			return;
		}
		this.variants.Add(item);
	}
}
