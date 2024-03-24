using System;
using System.Reflection;

// ReSharper disable all

namespace Submerged.BaseGame.Patches;

[Obsolete("Submerged no longer prefix patches ExileController.Begin. You do not need to worry about special compatibility when patching.", true)]
public static class ExileControllerPatches
{
    [Obsolete("Submerged no longer prefix patches ExileController.Begin. You do not need to worry about special compatibility when patching.", true)]
    public static void DisablePrefixWarningForHarmonyId(string id)
    {
        Error("Submerged no longer prefix patches ExileController.Begin. You do not need to worry about special compatibility when patching.");
        Error("Submerged no longer prefix patches ExileController.Begin. You do not need to worry about special compatibility when patching.");
        Error("Submerged no longer prefix patches ExileController.Begin. You do not need to worry about special compatibility when patching.");
        Error("Submerged no longer prefix patches ExileController.Begin. You do not need to worry about special compatibility when patching.");
        Error("Submerged no longer prefix patches ExileController.Begin. You do not need to worry about special compatibility when patching.");
    }

    [Obsolete("Submerged no longer prefix patches ExileController.Begin. You do not need to worry about special compatibility when patching.", true)]
    public static void ExileController_Begin(ExileController self, GameData.PlayerInfo exiled, bool tie)
    {
    }

    [Obsolete("Submerged no longer prefix patches ExileController.Begin. You do not need to worry about special compatibility when patching.", true)]
    public static bool BeginPatch(ExileController __instance, MethodInfo __originalMethod, GameData.PlayerInfo exiled, bool tie)
    {
        return false;
    }
}
