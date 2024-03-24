using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;

namespace Submerged.IL2CPP;

/*
 * This is a workaround for https://github.com/BepInEx/Il2CppInterop/issues/97
 * We are essentially pretending that the class being injected as the field return type is just Object
 * as that would be deserialized the same way by the engine, while not breaking due to the types
 * not being fully injected
 *
 * The patch specifically tries to do what interop does with strings in the field injection, where it just remaps the type
 * if it comes across an Il2CppStringField
 */
public static class InteropPatches
{
    private static readonly Harmony _harmony = new(nameof(InteropPatches));

    internal static void Initialize()
    {
        _harmony.PatchAll(typeof(InteropPatches));
    }

    public static Type[] GetGenericTypeArgumentsPatch(Type type)
    {
        Type[] args = type.GenericTypeArguments.ToArray();
        args[0] = args[0].IsSubclassOf(typeof(UnityObject)) ? typeof(UnityObject) : args[0];
        return args;
    }

    [HarmonyPatch(typeof(ClassInjector), nameof(ClassInjector.RegisterTypeInIl2Cpp), typeof(Type), typeof(RegisterTypeOptions))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> ForceInjectedFieldToBeUnityObjectPatch(IEnumerable<CodeInstruction> instructions)
    {
        CodeMatcher matcher = new(instructions);
        matcher.MatchEndForward(new CodeMatch(OpCodes.Ldloc_S),
                                new CodeMatch(OpCodes.Ldloc_S),
                                new CodeMatch(OpCodes.Ldelem_Ref),
                                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(FieldInfo), "get_FieldType")),
                                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Type), "get_GenericTypeArguments")));

        matcher.MatchStartBackwards(new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Type), "get_GenericTypeArguments")));
        matcher.Set(OpCodes.Call, AccessTools.Method(typeof(InteropPatches), nameof(GetGenericTypeArgumentsPatch)));

        return matcher.InstructionEnumeration();
    }
}
