/*
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Beebyte.Obfuscator
{
	public class ObfuscatorMenuExample
	{
		private static Options _options = null;

		private static IList<string> GetDllPaths()
		{
			var dlls = new List<string> {@"C:\path\to\External.dll"};

			foreach (string dll in dlls.Where(dll => !File.Exists(dll)))
			{
				throw new Exception("Could not find " + dll);
			}
			return dlls;
		}

		[MenuItem("Tools/Obfuscate External DLL")]
		private static void ObfuscateExternalDll()
		{
			Debug.Log("Obfuscating");

			var dllPaths = GetDllPaths();

			//Options are read in the same way as normal Obfuscation, i.e. from the ObfuscatorOptions.asset
			if (_options == null) _options = OptionsManager.LoadOptions();

			bool oldSkipRenameOfAllPublicMonobehaviourFields = _options.skipRenameOfAllPublicMonobehaviourFields;
			try
			{
				//Preserving monobehaviour public field names is an common step for obfuscating external DLLs that
				//allow MonoBehaviours to be dragged into the scene's hierarchy.
				_options.skipRenameOfAllPublicMonobehaviourFields = true;

				//Consider setting this hidden value to false to allow classes like EditorWindow to be obfuscated.
				//ScriptableObjects would normally be treated as Serializable to avoid breaking loading/saving,
				//but for Editor windows this might not be necessary.
				//options.treatScriptableObjectsAsSerializable = false;

				Obfuscator.SetExtraAssemblyDirectories(_options.extraAssemblyDirectories);
				Obfuscator.Obfuscate(dllPaths, _options, EditorUserBuildSettings.activeBuildTarget);
			}
			finally
			{
				_options.skipRenameOfAllPublicMonobehaviourFields = oldSkipRenameOfAllPublicMonobehaviourFields;
				EditorUtility.ClearProgressBar();
			}
		}
	}
}
*/