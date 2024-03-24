using System.Globalization;
using System.Reflection;
using HarmonyLib;
using ResourceEmbedderCompilerGenerated;

namespace Submerged.Localization.Patches;

[HarmonyPatch]
public static class SatelliteLoadPatches
{
    [HarmonyTargetMethod, UsedImplicitly]
    public static MethodBase TargetMethod() => AccessTools.Method("System.Resources.ManifestBasedResourceGroveler:InternalGetSatelliteAssembly");

    [HarmonyPrefix, UsedImplicitly]
    public static bool FixSatelliteCachePatch(ref Assembly __result, CultureInfo culture)
    {
        Assembly resource = ResourceEmbedderILInjected.LoadFromResource(new AssemblyName
        {
            Name = "Submerged.resources", CultureInfo = culture
        }, null);
        if (resource == null) return true;

        __result = resource;
        return false;
    }
}
