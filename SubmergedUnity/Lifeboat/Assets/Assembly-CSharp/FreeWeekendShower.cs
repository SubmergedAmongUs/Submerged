using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

public class FreeWeekendShower : MonoBehaviour
{
	public TextMeshPro Output;

	private void Start()
	{
		base.StartCoroutine(this.Check());
	}

	private IEnumerator Check()
	{
		WaitForSeconds wait = new WaitForSeconds(1f);
		StringBuilder txt = new StringBuilder();
		for (;;)
		{
			txt.Length = 0;
			if (Constants.ShouldFlipSkeld())
			{
				txt.AppendLine("Happy April Fools! Enjoy ehT Dleks!");
			}
			if (Constants.ShouldFlipSkeld())
			{
				txt.AppendLine("Happy April Fools! Enjoy ehT Dleks!");
			}
			this.Output.text = txt.ToString();
			yield return wait;
		}
		yield break;
	}
}
