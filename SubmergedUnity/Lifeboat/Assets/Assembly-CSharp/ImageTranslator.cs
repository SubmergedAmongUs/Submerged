using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ImageTranslator : MonoBehaviour, ITranslatedText
{
	public ImageNames TargetImage;

	public void ResetText()
	{
		Sprite image = DestroyableSingleton<TranslationController>.Instance.GetImage(this.TargetImage);
		if (image)
		{
			base.GetComponent<SpriteRenderer>().sprite = image;
		}
	}

	public void Start()
	{
		DestroyableSingleton<TranslationController>.Instance.ActiveTexts.Add(this);
		this.ResetText();
	}

	public void OnDestroy()
	{
		if (DestroyableSingleton<TranslationController>.InstanceExists)
		{
			DestroyableSingleton<TranslationController>.Instance.ActiveTexts.Remove(this);
		}
	}
}
