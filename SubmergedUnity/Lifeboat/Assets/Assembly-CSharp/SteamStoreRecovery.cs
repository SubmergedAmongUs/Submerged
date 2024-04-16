using System;
using Steamworks;
using UnityEngine;

public class SteamStoreRecovery : MonoBehaviour
{
	private void Start()
	{
		foreach (PetBehaviour petBehaviour in DestroyableSingleton<HatManager>.Instance.AllPets)
		{
			if (petBehaviour.SteamAppId != 0U)
			{
				if (SteamApps.BIsDlcInstalled(new AppId_t(petBehaviour.SteamAppId)))
				{
					SaveManager.SetPurchased(petBehaviour.ProdId);
				}
				else
				{
					SaveManager.ClearPurchased(petBehaviour.ProdId);
				}
			}
		}
		foreach (MapBuyable mapBuyable in DestroyableSingleton<HatManager>.Instance.AllMaps)
		{
			if (SteamApps.BIsDlcInstalled(new AppId_t(mapBuyable.SteamAppId)))
			{
				SaveManager.SetPurchased(mapBuyable.ProdId);
			}
			else
			{
				SaveManager.ClearPurchased(mapBuyable.ProdId);
			}
		}
	}
}
