using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Beebyte.Obfuscator
{
	public class OptionsManager
	{
		public const string ImportAssetName = "ObfuscatorOptionsImport";
		public const string OptionsAssetName = "ObfuscatorOptions";

		public const string DefaultImportPath = @"Assets/Editor/Beebyte/Obfuscator/ObfuscatorOptionsImport.asset";
		public const string DefaultOptionsPath = @"Assets/Editor/Beebyte/Obfuscator/ObfuscatorOptions.asset";

		public static Options LoadOptions()
		{
			if (HasInstallFiles()) Install();

			Options o = LoadAsset(OptionsAssetName);

			if (o != null)
			{
                Obfuscator.FixHexBug(o);
				return o;
			}
			
			Debug.LogError("Failed to load " + OptionsAssetName + " asset at " + DefaultOptionsPath);
			return null;
		}
		
		/**
		 * Can return null, i.e. on first package install.
		 */
		public static Options LoadOptionsIgnoringInstallFiles()
		{
			Options o = LoadAsset(OptionsAssetName);
			if (o == null) return null;
			
			Obfuscator.FixHexBug(o);
			return o;
		}

		private static bool HasInstallFiles()
		{
			return LoadAsset(ImportAssetName) != null;
		}

		private static Options LoadAsset(string name)
		{
			string path = GetAssetPath(name);

			return LoadAssetAtPath(path);
		}

		private static void Install()
		{
			Options importOptions = LoadAsset(ImportAssetName);
			if (importOptions == null)
			{
				Debug.LogError("Could not find " + ImportAssetName + ".asset - aborting installation.");
				return;
			}

			string importPath = GetAssetPath(ImportAssetName);
			string newOptionsPath = GetInstallPathFromImport(importPath);

			Options o = LoadAssetAtPath(newOptionsPath);

			if (o != null)
			{
				bool overwrite = EditorUtility.DisplayDialog("Obfuscator Installation", "ObfuscatorOptions already exists, would you like to replace it with new default options?", "Use new defaults", "Keep existing settings");
				if (overwrite) AssetDatabase.MoveAssetToTrash(newOptionsPath);
				else
				{
					AssetDatabase.MoveAssetToTrash(importPath);
					return;
				}
			}

			//Copy & Delete instead of Move, otherwise future installs think that ObfuscatorOptions is actually ObfuscatorOptionsImport
			AssetDatabase.CopyAsset(importPath, newOptionsPath);
			AssetDatabase.DeleteAsset(importPath);
			AssetDatabase.Refresh();
		}

		private static string GetDefaultPath(string assetName)
		{
			if (ImportAssetName.Equals(assetName)) return DefaultImportPath;
			if (OptionsAssetName.Equals(assetName)) return DefaultOptionsPath;
			return null;
		}

#if UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1
		private static string GetAssetPath(string name)
		{
			return GetDefaultPath(name);
		}

		private static Options LoadAssetAtPath(string path)
		{
			return (Options)Resources.LoadAssetAtPath(path, typeof(Options));
		}
#else
		private static string GetAssetPath(string name)
		{
			string[] optionGuids = AssetDatabase.FindAssets(name);

			IList<string> optionPaths = new List<string>();
			
			foreach (string guid in optionGuids)
			{
                string optionPath = AssetDatabase.GUIDToAssetPath(guid);
				if (optionPath.EndsWith(name + ".asset"))
				{
                    optionPaths.Add(optionPath);
				}
			}

			if (optionPaths.Count == 0) return null;
			if (optionPaths.Count == 1) return optionPaths[0];
			
            Debug.LogError("Multiple " + name + " assets found! Aborting");
            return null;
		}

		private static Options LoadAssetAtPath(string path)
		{
			return AssetDatabase.LoadAssetAtPath<Options>(path);
		}
#endif

		private static string GetInstallPathFromImport(string importPath)
		{
			return importPath.Replace(ImportAssetName + ".asset", OptionsAssetName + ".asset");
		}
	}
}
