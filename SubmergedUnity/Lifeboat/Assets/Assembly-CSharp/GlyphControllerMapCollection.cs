using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PEW/GlyphControllerMapCollection")]
public class GlyphControllerMapCollection : ScriptableObject
{
	public List<GlyphControllerMapCollection.GlyphControllerMap> nameToGlyphCollectionList = new List<GlyphControllerMapCollection.GlyphControllerMap>();

	private Dictionary<string, string> nameToGlyphCollectionDict;

	private static GlyphControllerMapCollection _instance;

	public void Initialize()
	{
		this.nameToGlyphCollectionDict = new Dictionary<string, string>();
		foreach (GlyphControllerMapCollection.GlyphControllerMap glyphControllerMap in this.nameToGlyphCollectionList)
		{
			this.nameToGlyphCollectionDict.Add(glyphControllerMap.controllerName, glyphControllerMap.glyphCollectionPath);
		}
	}

	public static GlyphControllerMapCollection Instance
	{
		get
		{
			if (!GlyphControllerMapCollection._instance)
			{
				GlyphControllerMapCollection._instance = Resources.Load<GlyphControllerMapCollection>("ControllerGlyphMapAsset");
				GlyphControllerMapCollection._instance.Initialize();
			}
			return GlyphControllerMapCollection._instance;
		}
	}

	public GlyphCollection TryGetGlyphCollectionForController(string controllerName)
	{
		string text;
		if (this.nameToGlyphCollectionDict.TryGetValue(controllerName, out text))
		{
			GlyphCollection glyphCollection = Resources.Load<GlyphCollection>(text);
			if (glyphCollection)
			{
				glyphCollection.Initialize();
				return glyphCollection;
			}
		}
		return null;
	}

	[Serializable]
	public class GlyphControllerMap
	{
		public string controllerName;

		public string glyphCollectionPath;
	}
}
