using System;
using System.Diagnostics;
using HarmonyLib;

namespace Submerged.Debugging;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate, AllowMultiple = true)]
[Conditional("DEBUG")]
public sealed class DebugHarmonyPatchAttribute : HarmonyPatch;
