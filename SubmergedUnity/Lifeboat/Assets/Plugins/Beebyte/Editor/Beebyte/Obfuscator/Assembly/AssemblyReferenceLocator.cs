#if UNITY_2017_3_OR_NEWER
/*
 * Copyright (c) 2018 Beebyte Limited. All rights reserved.
 */
using System.Collections.Generic;
using System.IO;
using UnityEditor.Compilation;

namespace Beebyte.Obfuscator.Assembly
{
	public class AssemblyReferenceLocator
	{
		public static IEnumerable<string> GetAssemblyReferenceDirectories()
		{
			HashSet<string> directories = new HashSet<string>();

			foreach (UnityEditor.Compilation.Assembly assembly in CompilationPipeline.GetAssemblies())
			{
				directories.UnionWith(GetAssemblyReferenceDirectories(assembly));
			}
			return directories;
		}

		private static IEnumerable<string> GetAssemblyReferenceDirectories(UnityEditor.Compilation.Assembly assembly)
		{
			HashSet<string> directories = new HashSet<string>();

			if (assembly == null) return directories;

			if (assembly.compiledAssemblyReferences == null || assembly.compiledAssemblyReferences.Length <= 0)
			{
				return directories;
			}
			
			foreach (string assemblyRef in assembly.compiledAssemblyReferences)
			{
				directories.Add(Path.GetDirectoryName(assemblyRef));
			}
			return directories;
		}
	}
}
#endif
