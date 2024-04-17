using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class TextTranslatorTMP : MonoBehaviour
{
	public StringNames TargetText;
	public string defaultStr;
	public bool ToUpper;
	public bool ResetOnlyWhenNoDefault;
}
