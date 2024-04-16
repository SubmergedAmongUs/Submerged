using System;
using UnityEngine;

public class EnableVSync : MonoBehaviour
{
	private void Awake()
	{
		QualitySettings.vSyncCount = 1;
	}
}
