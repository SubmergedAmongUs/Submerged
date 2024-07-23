using System;
using HarmonyLib;
using Reactor.Utilities.Extensions;
using Submerged.Resources;
using Submerged.Systems.Electrical.Patches;
using UnityEngine;

namespace Submerged.UI.Patches;

[HarmonyPatch]
public static class ShowReportButtonDisabledPatches
{
    private static readonly Lazy<Sprite> _reportDisabledSprite = new(() => ResourceManager.spriteCache["ReportDisabled"]);

    private static GameObject _reportDisabledObject;

    [HarmonyPatch(typeof(ActionButton), nameof(ActionButton.Start))]
    [HarmonyPostfix]
    public static void CreateDisabledIconObject(ActionButton __instance)
    {
        if (__instance.TryCast<ReportButton>() is not { } reportButton) return;

        if (_reportDisabledObject)
        {
            Warning("Report button disabled icon already exists somehow, deleting.");
            _reportDisabledObject.Destroy();
        }

        _reportDisabledObject = new GameObject("DisabledIcon");
        _reportDisabledObject.layer = reportButton.gameObject.layer;
        _reportDisabledObject.SetActive(false);
        _reportDisabledObject.transform.SetParent(reportButton.transform, false);
        _reportDisabledObject.transform.localPosition = new Vector3(0, 0, -0.1f);

        GameObject childifiedButton = new("ChildSprite");
        childifiedButton.layer = reportButton.gameObject.layer;
        childifiedButton.transform.SetParent(reportButton.transform, false);
        childifiedButton.transform.localPosition = Vector3.zero;
        SpriteRenderer originalRenderer = reportButton.GetComponent<SpriteRenderer>();
        SpriteRenderer childifiedButtonRenderer = childifiedButton.AddComponent<SpriteRenderer>();
        childifiedButtonRenderer.sprite = originalRenderer.sprite;
        childifiedButtonRenderer.material = originalRenderer.material;
        originalRenderer.enabled = false;
        reportButton.graphic = childifiedButtonRenderer;

        SpriteRenderer spriteRenderer = _reportDisabledObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = _reportDisabledSprite.Value;
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void DontShowReportButtonPatch()
    {
        if (!_reportDisabledObject) return;

        if (_reportDisabledObject.activeSelf != LightFlickerPatches.IsLightFlickerActive)
        {
            _reportDisabledObject.SetActive(LightFlickerPatches.IsLightFlickerActive);
        }
    }
}
