using HarmonyLib;
using Submerged.Extensions;
using Submerged.Map;
using Submerged.Resources;
using UnityEngine;

namespace Submerged.Floors.Patches;

[HarmonyPatch]
public static class ChangeFloorButtonPatches
{
    private static Sprite _upButton;
    private static Sprite _downButton;

    private static GameObject _floorButton;
    private static SpriteRenderer _floorButtonRenderer;

    private static Sprite UpButton
    {
        get
        {
            if (_upButton) return _upButton;
            return _upButton = ResourceManager.spriteCache["FloorUp"];
        }
    }

    private static Sprite DownButton
    {
        get
        {
            if (_downButton) return _downButton;
            return _downButton = ResourceManager.spriteCache["FloorDown"];
        }
    }

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
            distanceFromEdge.y = 1.7f;
            aspectPos.DistanceFromEdge = distanceFromEdge;
            aspectPos.AdjustPosition();

            _floorButtonRenderer = _floorButton.GetComponent<SpriteRenderer>();

            ButtonBehavior buttonBehavior = _floorButton.EnsureComponent<ButtonBehavior>();
            buttonBehavior.OnClick.RemoveAllListeners();
            buttonBehavior.OnClick.AddListener(() =>
            {
                FloorHandler floorHandler = FloorHandler.LocalPlayer;
                floorHandler.RegisterFloorOverride(!floorHandler.onUpper);
                floorHandler.RpcRequestChangeFloor(!floorHandler.onUpper);
            });
        }

        _floorButton.SetActive(true);
        _floorButtonRenderer.sprite = PlayerControl.LocalPlayer.transform.position.y <= -5 ? UpButton : DownButton;
    }
}
