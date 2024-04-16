using System;
using UnityEngine;

public class ConditionalHide : MonoBehaviour
{
	public RuntimePlatform[] HideForPlatforms = new RuntimePlatform[]
	{
		RuntimePlatform.WindowsPlayer
	};

	public RuntimePlatform[] OnlyShowForPlatforms = new RuntimePlatform[0];

	private void Awake()
	{
		for (int i = 0; i < this.HideForPlatforms.Length; i++)
		{
			if (this.HideForPlatforms[i] == RuntimePlatform.WindowsPlayer)
			{
				base.gameObject.SetActive(false);
			}
		}
		for (int j = 0; j < this.OnlyShowForPlatforms.Length; j++)
		{
			base.gameObject.SetActive(this.OnlyShowForPlatforms[j] == RuntimePlatform.WindowsPlayer);
		}
	}
}
