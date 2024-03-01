using System.Runtime.CompilerServices;
using HarmonyLib;
using Submerged.Extensions;
using UnityEngine;

namespace Submerged.Vents.Patches;

[HarmonyPatch]
public static class HidePlayerPatches
{
    private static readonly ConditionalWeakTable<Sprite, string> _spriteNames = new();

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.LateUpdate))]
    [HarmonyPostfix]
    public static void HidePlayerBehindVentPatch(PlayerPhysics __instance)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;

        PlayerControl myPlayer = __instance.myPlayer;
        if (!myPlayer) return;

        GameData.PlayerInfo myData = myPlayer.Data;
        if (myData == null) return;

        Transform transform = __instance.transform;
        Vector3 pos = transform.position;

        Sprite playerSprite = myPlayer.cosmetics.currentBodySprite.BodySprite.sprite;

        if (!_spriteNames.TryGetValue(playerSprite, out string name))
        {
            name = playerSprite.name;
            _spriteNames.Add(playerSprite, name);
        }

        // Lower Central Vent
        Vector2 lowerDelta = pos;
        lowerDelta.x -= 6.1963f;
        lowerDelta.y -= -26.9848f;

        if (name is "Vent0003" or "Vent0004" or "Vent0005" or "Vent0006" or "Vent0007" && lowerDelta.sqrMagnitude <= 4)
        {
            transform.SetZPos(-0.0025f);

            return;
        }

        // Engine vent
        Vector3 upperDelta = pos;
        upperDelta.x -= -11.1805f;
        upperDelta.y -= -34.672f;

        if (upperDelta.sqrMagnitude <= 4 && (__instance.myPlayer.inVent || name == "Vent0007"))
        {
            transform.SetZPos(-0.0025f);
        }
    }
}
