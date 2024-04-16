/*
 * Copyright (c) 2015-2019 Beebyte Limited. All rights reserved.
 */
#if !BEEBYTE_OBFUSCATOR_DISABLE

using System;
using System.IO;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;
#if UNITY_5_6_OR_NEWER
using UnityEditor.Build;
#endif
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif

namespace Beebyte.Obfuscator
{
	/**
	 * Hooks into the Unity build pipeline and delegates to perform obfuscation / restoration if requested.
	 */
#if UNITY_2018_2_OR_NEWER
	public class PipelineHook : IPreprocessBuildWithReport, IPostBuildPlayerScriptDLLs
#elif UNITY_2018_1_OR_NEWER
	public class PipelineHook : IPreprocessBuildWithReport, IPostprocessBuildWithReport
#elif UNITY_5_6_OR_NEWER
	public class PipelineHook : IPreprocessBuild, IPostprocessBuild
#else
	public class PipelineHook
#endif
	{
		private static Project _project = new Project();

#if UNITY_5_6_OR_NEWER
		public int callbackOrder
		{
			get { return 1; }
		}
#endif

		[PostProcessBuild(2)]
		public static void PostProcessBuild(BuildTarget target, string pathToBuildProject)
		{
			_project.ObfuscateAssets(target, pathToBuildProject);
		}

#if UNITY_2018_2_OR_NEWER
		public void OnPostBuildPlayerScriptDLLs(BuildReport report)
#elif UNITY_2018_1_OR_NEWER
		public void OnPostprocessBuild(BuildReport report)
#elif UNITY_5_6_OR_NEWER
		public void OnPostprocessBuild(BuildTarget target, string path)
#else
		[PostProcessBuild(1)]
		private static void PostBuildHook(BuildTarget buildTarget, string pathToBuildProject)
#endif
		{
			if (_project.HasMonoBehaviourAssetsThatNeedReverting()) RestoreUtils.RestoreMonobehaviourSourceFiles();
			RestoreUtils.RestoreOriginalDlls();
			
			try
			{
				if (_project.IsSuccess()) return;
				
				if (_project.HasCSharpScripts())
				{
					Debug.LogError("Failed to obfuscate");
#if UNITY_5_6_OR_NEWER
					throw new BuildFailedException("Failed to obfuscate");
#else
					throw new OperationCanceledException("Failed to obfuscate");
#endif
				}
					
				Debug.LogWarning("No obfuscation required because no C# scripts were found");
			}
			finally
			{
				Clear();
			}
		}

#if UNITY_2018_1_OR_NEWER
		public void OnPreprocessBuild(BuildReport report)
		{
			_project = new Project();
		}
#elif UNITY_5_6_OR_NEWER
		public void OnPreprocessBuild(BuildTarget target, string path)
		{
			_project = new Project();
		}
#endif
		
		[PostProcessScene(1)]
		public static void Obfuscate()
		{
			_project.ObfuscateIfNeeded();
		}
		
		public static void ClearProjectViaUpdate()
		{
			Clear();
            EditorApplication.update -= ClearProjectViaUpdate;
		}

		private static void Clear()
		{
            _project = new Project();
		}
	}
	
    [InitializeOnLoad]
    public static class RestorationStatic
    {
        /*
         * Often Unity's EditorApplication.update delegate is reset. Because it's so important to restore
         * renamed MonoBehaviour assets we assign here where it will be called after scripts are compiled.
         */ 
        static RestorationStatic()
        {
#if UNITY_2018_2_OR_NEWER
#else
            EditorApplication.update += RestoreUtils.RestoreMonobehaviourSourceFiles;
#endif
            EditorApplication.update += RestoreUtils.RestoreOriginalDlls;
        }
    }
}
#endif
