/*
 * Copyright (c) 2020 Beebyte Limited. All rights reserved.
 */
using System.IO;
using UnityEditor;

namespace Beebyte.Obfuscator
{
    public class GlobalGameManagersPath
    {
		internal static string GetPathToGlobalGameManagersAsset(BuildTarget buildTarget, string buildPath)
		{
			if ((int) buildTarget == 2)
			{
				return GetPathForMac(buildPath);
			}

#if UNITY_2018_2_OR_NEWER
			if (EditorUserBuildSettings.GetPlatformSettings("Standalone", "CreateSolution").Equals("true"))
			{
				return GetPathForVSProjectWindowsAndLinuxStandalone(buildPath);
			}
#endif
			return GetPathForWindowsAndLinuxStandalone(buildPath);
		}

		private static string GetPathForMac(string buildPath)
		{
			return Path.GetDirectoryName(buildPath) +
			       Path.DirectorySeparatorChar.ToString() +
			       Path.GetFileNameWithoutExtension(buildPath) +
			       ".app" +
			       Path.DirectorySeparatorChar.ToString() +
			       "Contents" +
			       Path.DirectorySeparatorChar.ToString() +
			       "Resources" +
			       Path.DirectorySeparatorChar.ToString() +
			       "Data" +
			       Path.DirectorySeparatorChar.ToString() +
			       "globalgamemanagers.assets";
		}
		
		private static string GetPathForWindowsAndLinuxStandalone(string buildPath)
		{
			return Path.GetDirectoryName(buildPath) +
			       Path.DirectorySeparatorChar.ToString() +
			       Path.GetFileNameWithoutExtension(buildPath) +
			       "_Data" +
			       Path.DirectorySeparatorChar.ToString() +
			       "globalgamemanagers.assets";
		}
		
		private static string GetPathForVSProjectWindowsAndLinuxStandalone(string buildPath)
		{
		   return Path.GetDirectoryName(buildPath) +
				  Path.DirectorySeparatorChar.ToString() +
				  "build" +
				  Path.DirectorySeparatorChar.ToString() +
				  "bin" +
				  Path.DirectorySeparatorChar.ToString() +
				  Path.GetFileNameWithoutExtension(buildPath) +
				  "_Data" +
				  Path.DirectorySeparatorChar.ToString() +
				  "globalgamemanagers.assets";
		}
    }
}
