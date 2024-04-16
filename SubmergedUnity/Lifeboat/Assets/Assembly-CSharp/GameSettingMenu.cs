using System;
using System.Collections.Generic;
using UnityEngine;

public class GameSettingMenu : MonoBehaviour
{
	public Transform[] AllItems;

	public float YStart;

	public float YOffset;

	public Transform[] HideForOnline;

	private void OnEnable()
	{
		int num = 0;
		for (int i = 0; i < this.AllItems.Length; i++)
		{
			Transform transform = this.AllItems[i];
			if (transform.gameObject.activeSelf)
			{
				if (AmongUsClient.Instance.GameMode == GameModes.OnlineGame && this.HideForOnline.IndexOf(transform) != -1)
				{
					transform.gameObject.SetActive(false);
				}
				else
				{
					if (transform.name.Equals("MapName", StringComparison.OrdinalIgnoreCase))
					{
						int num2 = 0;
						List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>();
						for (int j = 0; j < GameOptionsData.MapNames.Length; j++)
						{
							if (j != 3 && j < AmongUsClient.Instance.ShipPrefabs.Count)
							{
								string key = (j == 0 && Constants.ShouldFlipSkeld()) ? "Dleks" : GameOptionsData.MapNames[j];
								list.Add(new KeyValuePair<string, int>(key, j));
								num2++;
							}
						}
						transform.GetComponent<KeyValueOption>().Values = list;
						if (num2 == 1)
						{
							transform.gameObject.SetActive(false);
							goto IL_118;
						}
					}
					Vector3 localPosition = transform.localPosition;
					localPosition.y = this.YStart - (float)num * this.YOffset;
					transform.localPosition = localPosition;
					num++;
				}
			}
			IL_118:;
		}
		base.GetComponent<Scroller>().YBounds.max = (float)num * this.YOffset - 2f * this.YStart - 0.1f;
	}
}
