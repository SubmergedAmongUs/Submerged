using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HatManager : DestroyableSingleton<HatManager>
{
	public HatBehaviour NoneHat;

	public Material DefaultHatShader;

	public List<PetBehaviour> AllPets = new List<PetBehaviour>();

	public List<HatBehaviour> AllHats = new List<HatBehaviour>();

	public List<SkinData> AllSkins = new List<SkinData>();

	public List<MapBuyable> AllMaps = new List<MapBuyable>();

	internal PetBehaviour GetPetById(uint petId)
	{
		if ((ulong)petId >= (ulong)((long)this.AllPets.Count))
		{
			return this.AllPets[0];
		}
		return this.AllPets[(int)petId];
	}

	public uint GetIdFromPet(PetBehaviour pet)
	{
		return (uint)this.AllPets.FindIndex((PetBehaviour p) => p.idleClip == pet.idleClip);
	}

	public PetBehaviour[] GetUnlockedPets()
	{
		return (from h in this.AllPets
		where h.Free || SaveManager.GetPurchase(h.ProductId)
		select h).ToArray<PetBehaviour>();
	}

	public HatBehaviour GetHatById(uint hatId)
	{
		if ((ulong)hatId >= (ulong)((long)this.AllHats.Count))
		{
			return this.NoneHat;
		}
		return this.AllHats[(int)hatId];
	}

	public HatBehaviour[] GetUnlockedHats()
	{
		return (from h in this.AllHats
		where (!HatManager.IsMapStuff(h.ProdId) && h.LimitedMonth == 0) || SaveManager.GetPurchase(h.ProductId)
		select h into o
		orderby o.Order descending, o.name
		select o).ToArray<HatBehaviour>();
	}

	public static bool IsMapStuff(string prodId)
	{
		return prodId == "map_mira" || prodId == "map_polus" || prodId == "hat_geoff" || prodId == "map_airship" || prodId == "hat_traffic_purple";
	}

	public uint GetIdFromHat(HatBehaviour hat)
	{
		return (uint)this.AllHats.IndexOf(hat);
	}

	public SkinData[] GetUnlockedSkins()
	{
		return (from s in this.AllSkins
		where !HatManager.IsMapStuff(s.ProdId) || SaveManager.GetPurchase(s.ProdId)
		select s into o
		orderby o.Order descending, o.name
		select o).ToArray<SkinData>();
	}

	public uint GetIdFromSkin(SkinData skin)
	{
		return (uint)this.AllSkins.IndexOf(skin);
	}

	internal SkinData GetSkinById(uint skinId)
	{
		if ((ulong)skinId >= (ulong)((long)this.AllSkins.Count))
		{
			return this.AllSkins[0];
		}
		return this.AllSkins[(int)skinId];
	}

	internal void SetSkin(SpriteRenderer skinRend, uint skinId)
	{
		SkinData skinById = this.GetSkinById(skinId);
		if (skinById)
		{
			skinRend.sprite = skinById.IdleFrame;
		}
	}

	internal HatBehaviour GetHatByProdId(string prodId)
	{
		return this.AllHats.FirstOrDefault((HatBehaviour h) => h.ProdId == prodId);
	}
}
