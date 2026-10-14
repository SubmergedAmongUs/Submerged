using System;
using System.Collections.Generic;
using System.IO;
using AsmResolver.DotNet;
using Il2CppInterop.Common;
using Il2CppInterop.Generator;
using Il2CppInterop.Generator.Runners;
using Microsoft.Extensions.Logging;

namespace Submerged.Build;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length != 3)
        {
            Console.Error.WriteLine("Usage: Submerged.InteropGenerator <input-directory> <output-directory> <unity-directory>");
            return 1;
        }

        try
        {
            string inputDirectory = Path.GetFullPath(args[0]);
            string outputDirectory = Path.GetFullPath(args[1]);
            string unityDirectory = Path.GetFullPath(args[2]);
            RequireAssembly(inputDirectory, "Assembly-CSharp.dll");
            RequireAssembly(unityDirectory, "UnityEngine.CoreModule.dll");

            string[] inputFiles = Directory.GetFiles(inputDirectory, "*.dll");
            Array.Sort(inputFiles, StringComparer.Ordinal);
            List<AssemblyDefinition> assemblies = new(inputFiles.Length);
            foreach (string file in inputFiles) assemblies.Add(AssemblyDefinition.FromFile(file));

            Directory.CreateDirectory(outputDirectory);
            Il2CppInteropGenerator.Create(new GeneratorOptions
                {
                    Source = assemblies,
                    OutputDir = outputDirectory,
                    UnityBaseLibsDir = unityDirectory,
                    NoXrefCache = true
                })
                .AddLogger(new BuildLogger())
                .AddInteropAssemblyGenerator()
                .Run();

            RequireAssembly(outputDirectory, "Assembly-CSharp.dll");
            RequireAssembly(outputDirectory, "UnityEngine.CoreModule.dll");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    private static void RequireAssembly(string directory, string fileName)
    {
        string path = Path.Combine(directory, fileName);
        if (!File.Exists(path)) throw new FileNotFoundException("Required assembly was not found.", path);
    }

    private sealed class BuildLogger : ILogger, IDisposable
    {
        public IDisposable BeginScope<TState>(TState state) => this;

        public void Dispose() { }

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
            Console.WriteLine(formatter(state, exception));
            if (exception != null) Console.WriteLine(exception);
        }
    }
}
