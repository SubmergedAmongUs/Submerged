using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class FontCache : MonoBehaviour
{
	public static FontCache Instance;

	private Dictionary<string, FontData> cache = new Dictionary<string, FontData>();

	public List<FontExtensionData> extraData = new List<FontExtensionData>();

	public List<TextAsset> DefaultFonts = new List<TextAsset>();

	public List<Material> DefaultFontMaterials = new List<Material>();

	public void OnEnable()
	{
		if (!FontCache.Instance)
		{
			FontCache.Instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
			return;
		}
		if (FontCache.Instance != null)
		{
			 UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public FontData LoadFont(TextAsset dataSrc)
	{
		if (this.cache == null)
		{
			this.cache = new Dictionary<string, FontData>();
		}
		FontData fontData;
		if (this.cache.TryGetValue(dataSrc.name, out fontData))
		{
			return fontData;
		}
		int num = this.extraData.FindIndex((FontExtensionData ed) => ed.FontName.Equals(dataSrc.name, StringComparison.OrdinalIgnoreCase));
		FontExtensionData eData = null;
		if (num >= 0)
		{
			eData = this.extraData[num];
		}
		fontData = FontCache.LoadFontUncached(dataSrc, eData);
		this.cache[dataSrc.name] = fontData;
		return fontData;
	}

	public static FontData LoadFontUncached(TextAsset dataSrc, FontExtensionData eData = null)
	{
		return FontLoader.FromBinary(dataSrc, eData);
	}
}
