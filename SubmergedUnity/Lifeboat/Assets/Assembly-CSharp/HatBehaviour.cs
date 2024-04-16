using System;
using UnityEngine;

[CreateAssetMenu]
public class HatBehaviour : ScriptableObject, IBuyable, IEpicBuyable
{
	public Sprite MainImage;

	public Sprite BackImage;

	public Sprite LeftMainImage;

	public Sprite LeftBackImage;

	public string EpicId;

	public Sprite ClimbImage;

	public Sprite FloorImage;

	public Sprite LeftClimbImage;

	public Sprite LeftFloorImage;

	public bool InFront;

	public bool NoBounce;

	public bool NotInStore;

	public bool Free;

	public Material AltShader;

	public Vector2 ChipOffset;

	public int LimitedMonth;

	public int LimitedYear;

	public SkinData RelatedSkin;

	public string StoreName;

	public string ProductId;

	public int Order;

	public string ProdId
	{
		get
		{
			return this.ProductId;
		}
	}

	public string EpicAppId
	{
		get
		{
			return this.EpicId;
		}
	}

	public string EpicPrice
	{
		get
		{
			return "$2.99";
		}
	}
}
