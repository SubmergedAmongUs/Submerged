using System.Collections.Generic;
using Beebyte.Obfuscator.Assembly;
using UnityEditor;

namespace Beebyte.Obfuscator
{
	public class RestoreUtils
	{
		/**
		 * When assemblies are declared in the 'Compiled Assemblies' list within Obfuscator options then before any
		 * obfuscation they are temporarily backed up then operated on. This method is then called after obfuscation
		 * to restore the original assembly and remove the backup.
		 */
		public static void RestoreOriginalDlls()
		{
			try
			{
				Options options = OptionsManager.LoadOptionsIgnoringInstallFiles();
				if (options == null || options.compiledAssemblies.Length == 0)
				{
					return;
				}
				ICollection<string> compiledAssemblyPaths = new AssemblySelector(options).GetCompiledAssemblyPaths();
				IDictionary<string, string> backupMap = FileBackup.GetBackupMap(compiledAssemblyPaths);
				//DLLs declared within 'Compiled Assemblies' will be restored from this method.
				FileBackup.Restore(backupMap);
			}
			finally
			{
                EditorApplication.update -= RestoreOriginalDlls;
			}
		}
		
		/**
		 * This method restores obfuscated MonoBehaviour cs files to their original names.
		 */
		public static void RestoreMonobehaviourSourceFiles()
		{
#if UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
#else
			if (BuildPipeline.isBuildingPlayer == false)
			{
#endif
				try
				{
					EditorApplication.LockReloadAssemblies();
					Obfuscator.RevertAssetObfuscation();
				}
				finally
				{
					EditorApplication.update -= RestoreMonobehaviourSourceFiles;
					EditorApplication.UnlockReloadAssemblies();
				}
#if UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
#else
			}
#endif
		}
	}
}