using HarmonyLib;
using Hazel;
using Submerged.Enums;
using Submerged.Extensions;

namespace Submerged.Systems.Oxygen.Patches;

[HarmonyPatch]
public static class SabotagingPatches
{
    [HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.UpdateSystem))]
    [HarmonyPostfix]
    public static void RedirectOxygenPatch([HarmonyArgument(0)] PlayerControl player, [HarmonyArgument(1)] MessageReader reader)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;

        byte amount = reader.Buffer[reader.readHead - 1];

        if (!MeetingHud.Instance && AmongUsClient.Instance.AmHost && amount == (byte) SystemTypes.LifeSupp)
        {
            ShipStatus.Instance.UpdateSystem(CustomSystemTypes.UpperCentral, player, 128);
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.UpdateSystem), typeof(SystemTypes), typeof(PlayerControl), typeof(byte))]
    [HarmonyPrefix]
    public static void RedirectRepairSystemPatch([HarmonyArgument(0)] ref SystemTypes systemType, [HarmonyArgument(2)] byte amount)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;

        if (systemType == SystemTypes.LifeSupp && amount is 16 or 128)
        {
            // 16 is to disable the sabotage, called from ShipStatus.RepairCriticalSabotages
            // 128 is to start the sabotage, called from SabotageSystemType.UpdateSystem
            systemType = CustomSystemTypes.UpperCentral;
        }
    }
}
