using System.Reflection;
using HarmonyLib;
using Detour = MonoMod.RuntimeDetour.Detour;
using _IL2CPP = Il2CppInterop.Runtime.IL2CPP;

namespace Submerged.IL2CPP;

// This is needed because otherwise sometimes the exile text is not displayed
public static class FixExileTextDetours
{
    // We are using MonoMod detours here because harmony cries
    public static void Apply()
    {
        MethodInfo il2CppStringToManagedMethod = AccessTools.Method(typeof(_IL2CPP), nameof(_IL2CPP.Il2CppStringToManaged));
        MethodInfo il2CppStringToManagedPatch = AccessTools.Method(typeof(FixExileTextDetours), nameof(Il2CppStringToManaged));
        Detour detour = new(il2CppStringToManagedMethod, il2CppStringToManagedPatch);
        detour.Apply();
    }

    public static unsafe string Il2CppStringToManaged(IntPtr il2CppString)
    {
        if (il2CppString == IntPtr.Zero) return null;
        int length = _IL2CPP.il2cpp_string_length(il2CppString);
        return length == 0 ? "" : new string(_IL2CPP.il2cpp_string_chars(il2CppString), 0, length);
    }
}
