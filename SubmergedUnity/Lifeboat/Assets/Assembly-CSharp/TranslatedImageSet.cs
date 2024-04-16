using System;
using UnityEngine;

[CreateAssetMenu]
public class TranslatedImageSet : ScriptableObject
{
	public string Name;

	public SupportedLangs languageID;

	public TextAsset Data;

	public ImageData[] Images;
}
