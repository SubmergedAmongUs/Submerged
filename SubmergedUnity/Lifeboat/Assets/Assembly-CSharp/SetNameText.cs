using System;
using TMPro;
using UnityEngine;

public class SetNameText : MonoBehaviour
{
	[SerializeField]
	private TextMeshPro nameText;

	private void Start()
	{
		this.nameText.text = SaveManager.PlayerName;
	}
}
