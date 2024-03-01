using System;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Submerged.Map;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.FixWiring.Patches;

[HarmonyPatch]
public static class ConsolePatches
{
    [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.PickRandomConsoles), typeof(TaskTypes), typeof(Il2CppStructArray<byte>))]
    [HarmonyPostfix]
    public static void EletricalLastConsolePatch([HarmonyArgument(0)] TaskTypes taskType, [HarmonyArgument(1)] Il2CppStructArray<byte> consoleIds)
    {
        byte[] original = consoleIds.ToArray();

        try
        {
            if (!SubmarineStatus.instance || taskType != TaskTypes.FixWiring) return;

            if (GameManager.Instance.IsHideAndSeek())
            {
                if (UnityRandom.Range(0, 1f) < 0.5f)
                {
                    (consoleIds[0], consoleIds[1], consoleIds[2]) = (1, 2, 0);
                }
                else
                {
                    (consoleIds[0], consoleIds[1], consoleIds[2]) = (2, 3, 0);
                }

                return;
            }

            if (consoleIds[0] == 0)
            {
                (consoleIds[0], consoleIds[1], consoleIds[2]) = (consoleIds[1], consoleIds[2], 0);
            }
            else
            {
                int rnd = UnityRandom.RandomRangeInt(0, 3);

                switch (rnd)
                {
                    case 0:
                        (consoleIds[0], consoleIds[1], consoleIds[2]) = (consoleIds[1], consoleIds[2], 0);

                        break;

                    case 1:
                        (consoleIds[0], consoleIds[1], consoleIds[2]) = (consoleIds[0], consoleIds[2], 0);

                        break;

                    case 2:
                        (consoleIds[0], consoleIds[1], consoleIds[2]) = (consoleIds[0], consoleIds[1], 0);

                        break;
                }
            }
        }
        catch (Exception e)
        {
            Error("Caught exception in wires patch. PLEASE REPORT THIS ON GITHUB!");
            Error(e);

            for (int i = 0; i < consoleIds.Length; i++)
            {
                consoleIds[i] = original[i];
            }
        }
    }

    [HarmonyPatch(typeof(Console), nameof(Console.Use))]
    [HarmonyPrefix]
    public static bool ReplaceElectricalMinigamePrefabPatch(Console __instance)
    {
        if (!SubmarineStatus.instance) return true;

        __instance.CanUse(PlayerControl.LocalPlayer.Data, out bool canUse, out bool _);

        if (!canUse) return false;

        PlayerControl localPlayer = PlayerControl.LocalPlayer;
        PlayerTask playerTask = __instance.FindTask(localPlayer);

        if (playerTask.MinigamePrefab)
        {
            Minigame minigamePrefab;

            if (playerTask.TaskType == TaskTypes.FixWiring && __instance.ConsoleId == 0)
            {
                minigamePrefab = playerTask.GetComponent<ColorChip>().InUseForeground.GetComponent<Minigame>();
            }
            else
            {
                minigamePrefab = playerTask.GetMinigamePrefab();
            }

            Minigame minigame = UnityObject.Instantiate(minigamePrefab, Camera.main!.transform, false);
            minigame.transform.localPosition = new Vector3(0f, 0f, -50f);
            minigame.Console = __instance;
            minigame.Begin(playerTask);
        }

        return false;
    }
}
