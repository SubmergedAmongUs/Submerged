using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

[CreateAssetMenu(menuName = "PEW/GlyphCollection")]
public class GlyphCollection : ScriptableObject
{
	public string controllerType;

	public List<GlyphCollection.GlyphMap> glyphMaps = new List<GlyphCollection.GlyphMap>();

	public Dictionary<string, GlyphCollection.GlyphMap> glyphDict;

	private static List<ActionElementMap> mapResults = new List<ActionElementMap>();

	private static GlyphCollection defaultGlyphCollection;

	private static Dictionary<string, GlyphCollection> otherGlyphCollections = new Dictionary<string, GlyphCollection>();

	private static HashSet<string> controllersWithNoValidGlyphCollection = new HashSet<string>();

	public void Initialize()
	{
		this.glyphDict = new Dictionary<string, GlyphCollection.GlyphMap>();
		foreach (GlyphCollection.GlyphMap glyphMap in this.glyphMaps)
		{
			this.glyphDict[glyphMap.elementIdentifier.ToLower()] = glyphMap;
			if (!string.IsNullOrEmpty(glyphMap.alternateElementIdentifier))
			{
				this.glyphDict[glyphMap.alternateElementIdentifier.ToLower()] = glyphMap;
			}
		}
	}

	private static string GlyphPath
	{
		get
		{
			return "Glyphs/XboxGlyphs";
		}
	}

	public static Sprite FindGlyph(int actionName, out GlyphCollection.ErrorCode error)
	{
		if (!GlyphCollection.defaultGlyphCollection)
		{
			GlyphCollection.defaultGlyphCollection = Resources.Load<GlyphCollection>(GlyphCollection.GlyphPath);
			if (!GlyphCollection.defaultGlyphCollection)
			{
				error = GlyphCollection.ErrorCode.NoGlyphFound;
				return null;
			}
			GlyphCollection.defaultGlyphCollection.Initialize();
		}
		Player player = ReInput.players.GetPlayer(0);
		Rewired.Controller controller = player.controllers.GetLastActiveController();
		if (controller == null)
		{
			foreach (Rewired.Controller controller2 in player.controllers.Controllers)
			{
				if (controller2 is Joystick)
				{
					controller = controller2;
					break;
				}
			}
		}
		if (controller == null)
		{
			error = GlyphCollection.ErrorCode.NoController;
			return null;
		}
		GlyphCollection glyphCollection = GlyphCollection.defaultGlyphCollection;
		GlyphCollection glyphCollection2;
		if (GlyphCollection.otherGlyphCollections.TryGetValue(controller.name, out glyphCollection2))
		{
			glyphCollection = glyphCollection2;
		}
		else if (!GlyphCollection.controllersWithNoValidGlyphCollection.Contains(controller.name))
		{
			glyphCollection2 = GlyphControllerMapCollection.Instance.TryGetGlyphCollectionForController(controller.name);
			if (glyphCollection2)
			{
				glyphCollection = glyphCollection2;
				GlyphCollection.otherGlyphCollections.Add(controller.name, glyphCollection2);
				Debug.Log("Found valid glyph collection for " + controller.name);
			}
			else
			{
				GlyphCollection.controllersWithNoValidGlyphCollection.Add(controller.name);
				Debug.Log("No valid glyph collection for " + controller.name + ", using default");
			}
		}
		int elementMapsWithAction = player.controllers.maps.GetElementMapsWithAction(actionName, false, GlyphCollection.mapResults);
		if (elementMapsWithAction <= 0)
		{
			string str = "GlyphCollection.FindGlyph: No elements bound to action ";
			RewiredConstsEnum.Action action = (RewiredConstsEnum.Action)actionName;
			Debug.LogError(str + action.ToString());
			error = GlyphCollection.ErrorCode.NoElementsBoundToAction;
			return null;
		}
		ActionElementMap actionElementMap = GlyphCollection.mapResults[0];
		if (elementMapsWithAction > 1)
		{
			for (int i = 1; i < elementMapsWithAction; i++)
			{
				if (GlyphCollection.mapResults[i].elementType == null)
				{
					actionElementMap = GlyphCollection.mapResults[i];
				}
			}
		}
		GlyphCollection.GlyphMap glyphMap = null;
		if (glyphCollection.glyphDict.TryGetValue(actionElementMap.elementIdentifierName.ToLower(), out glyphMap))
		{
			error = GlyphCollection.ErrorCode.None;
			return glyphMap.glyph;
		}
		Debug.LogError("GlyphCollection.FindGlyph: GlyphCollection didn't have a glyph for element " + actionElementMap.elementIdentifierName + " on controller" + controller.name);
		error = GlyphCollection.ErrorCode.NoGlyphFound;
		return null;
	}

	[Serializable]
	public class GlyphMap
	{
		public string elementIdentifier;

		public string alternateElementIdentifier;

		public Sprite glyph;
	}

	public enum ErrorCode
	{
		None,
		NoController,
		NoGlyphFound,
		NoElementsBoundToAction
	}
}
