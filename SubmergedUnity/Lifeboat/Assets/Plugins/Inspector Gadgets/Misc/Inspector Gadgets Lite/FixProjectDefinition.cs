// Inspector Gadgets // https://kybernetik.com.au/inspector-gadgets // Copyright 2021 Kybernetik //

#if UNITY_EDITOR && UNITY_2020_1_OR_NEWER

using System;
using System.IO;
using UnityEditor;

namespace InspectorGadgets.Editor
{
    /// <summary>[Editor-Only]
    /// Ensures that project files don't reference the Runtime-Only version of <see cref="InspectorGadgets"/>.Lite.dll.
    /// </summary>
    internal static class FixProjectDefinition
    {
        /************************************************************************************************************************/

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            var files = Directory.GetFiles(Environment.CurrentDirectory, "*.csproj", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < files.Length; i++)
                FixFile(files[i]);
        }

        /************************************************************************************************************************/

        private static readonly char[] NewLineCharacters = { '\r', '\n' };

        private static void FixFile(string path)
        {
            var text = File.ReadAllText(path);

            var index = text.IndexOf($"{nameof(InspectorGadgets)}.Lite.dll");
            if (index < 0)
                return;

            const string Folder = "Runtime";
            if (string.Compare(text, index - Folder.Length - 1, Folder, 0, Folder.Length) != 0)
                return;

            var lineStart = text.LastIndexOfAny(NewLineCharacters, index);
            if (lineStart < 0)
                return;

            var lineEnd = text.IndexOfAny(NewLineCharacters, index);
            if (lineEnd < 0)
                return;

            text = text.Substring(0, lineStart) + text.Substring(lineEnd, text.Length - lineEnd);
            File.WriteAllText(path, text);
        }

        /************************************************************************************************************************/
    }
}

#endif
