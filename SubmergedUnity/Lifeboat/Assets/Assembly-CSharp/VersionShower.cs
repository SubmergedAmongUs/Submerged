using System;
using TMPro;
using UnityEngine;

public class VersionShower : MonoBehaviour
{
	public TextMeshPro text;

	public void Start()
	{
		string text = "v" + Application.version;
		text += "s";
		if (!DetectTamper.Detect())
		{
			text += "h";
		}
		if (!string.IsNullOrEmpty(""))
		{
			text += " ";
		}
		this.text.text = text;
		Screen.sleepTimeout = -1;
		Debug.Log("Among Us Version " + text + " Pipeline Build Num: " + 0.ToString());
	}
}
