using System;
using HarmonyLib;
using Submerged.Extensions;
using Submerged.Floors;
using Submerged.Map;
using Submerged.Resources;
using UnityEngine;

namespace Submerged.UI.Patches;

[HarmonyPatch]
public static class ChangeFloorButtonPatches
{
    private static readonly Lazy<Sprite> _downButton = new(() => ResourceManager.spriteCache["FloorDown"]);
    private static readonly Lazy<Sprite> _downButtonHover = new(() => ResourceManager.spriteCache["FloorDownHover"]);
    private static readonly Lazy<Sprite> _upButton = new(() => ResourceManager.spriteCache["FloorUp"]);
    private static readonly Lazy<Sprite> _upButtonHover = new(() => ResourceManager.spriteCache["FloorUpHover"]);

    private static GameObject _floorButton;
    private static SpriteRenderer _floorButtonRenderer;
    private static SpriteRenderer _floorButtonRendererHover;

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void HudUpdatePatch(HudManager __instance)
    {
        if (!SubmarineStatus.instance || Minigame.Instance || !GameManager.Instance || !GameManager.Instance.IsNormal() || MeetingHud.Instance || !PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data.IsDead)
        {
            if (_floorButton) _floorButton.SetActive(false);

            return;
        }

        if (!_floorButton)
        {
            _floorButton = UnityObject.Instantiate(__instance.MapButton.gameObject, __instance.MapButton.transform.parent);

            AspectPosition aspectPos = _floorButton.GetComponent<AspectPosition>();
            Vector3 distanceFromEdge = aspectPos.DistanceFromEdge;
            distanceFromEdge.y += 0.8f;
            aspectPos.DistanceFromEdge = distanceFromEdge;
            aspectPos.AdjustPosition();

            _floorButtonRenderer = _floorButton.transform.Find("Inactive").GetComponent<SpriteRenderer>();
            _floorButtonRendererHover = _floorButton.transform.Find("Active").GetComponent<SpriteRenderer>();

            PassiveButton buttonBehavior = _floorButton.EnsureComponent<PassiveButton>();
            buttonBehavior.OnClick.RemoveAllListeners();
            buttonBehavior.OnClick.AddListener(() =>
            {
                FloorHandler floorHandler = FloorHandler.LocalPlayer;
                floorHandler.RegisterFloorOverride(!floorHandler.onUpper);
                floorHandler.RpcRequestChangeFloor(!floorHandler.onUpper);
            });
        }

        _floorButton.SetActive(true);
        SetButtonStyle(PlayerControl.LocalPlayer.transform.position.y <= -5);
    }

    private static void SetButtonStyle(bool isMovingUp)
    {
        if (isMovingUp)
        {
            _floorButtonRenderer.sprite = _upButton.Value;
            _floorButtonRendererHover.sprite = _upButtonHover.Value;
        }
        else
        {
            _floorButtonRenderer.sprite = _downButton.Value;
            _floorButtonRendererHover.sprite = _downButtonHover.Value;
        }
    }
}
