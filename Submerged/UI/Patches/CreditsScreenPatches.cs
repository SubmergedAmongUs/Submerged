using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Reactor.Utilities.Extensions;
using Submerged.Extensions;
using Submerged.Resources;
using UnityEngine;

namespace Submerged.UI.Patches;

[HarmonyPatch]
public static class CreditsScreenPatches
{
    private const string MAIN_MENU_SPRITES_TEX_NAME = "MainMenuSprites";

    private static Texture2D _texture;
    private static CreditsScreenManager _creditsScreenManagerPrefab;
    private static GameObject _creditsScreen;

    private static CreditsScreenManager GetCreditsScreen()
    {
        if (_creditsScreenManagerPrefab) return _creditsScreenManagerPrefab;
        GameObject creditsMenuPrefab = ResourceManager.GetAssetBundle("credits").LoadAsset<GameObject>("CreditsScreen")!;
        creditsMenuPrefab.DontDestroy();
        creditsMenuPrefab.SetActive(false);

        return _creditsScreenManagerPrefab = creditsMenuPrefab.AddComponent<CreditsScreenManager>();
    }

    // TODO: Move part of this into update
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Awake))]
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.ActivateMainMenuUI))]
    [HarmonyPostfix]
    public static void CreateCreditsButtonPatch(MainMenuManager __instance)
    {
        if (!_creditsScreen) _creditsScreen = UnityObject.Instantiate(GetCreditsScreen()).gameObject;
        _creditsScreen.name = "SubmergedCreditsMenu";

        DoNotPressButton doNotPressButton = __instance.GetComponentInChildren<DoNotPressButton>(true);

        _texture ??= ResourceManager.GetTexture(MAIN_MENU_SPRITES_TEX_NAME).DontUnload();

        SpriteRenderer pedestalSprite = doNotPressButton.GetComponent<SpriteRenderer>();
        SpriteRenderer pressedSprite = doNotPressButton.transform.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer unpressedSprite = doNotPressButton.transform.GetChild(1).GetComponent<SpriteRenderer>();

        pedestalSprite.gameObject.SetActive(true);
        pressedSprite.gameObject.SetActive(true);
        unpressedSprite.gameObject.SetActive(true);

        unpressedSprite.enabled = true;
        pressedSprite.enabled = false;

        MaterialPropertyBlock block = new();
        block.AddTexture("_MainTex", _texture);

        Il2CppArrayBase<SpriteRenderer> spriteRenderers = doNotPressButton.GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            if (!spriteRenderer.sprite) continue;
            if (!spriteRenderer.sprite.texture) continue;
            if (spriteRenderer.sprite.texture.name != MAIN_MENU_SPRITES_TEX_NAME) continue;
            spriteRenderer.SetPropertyBlock(block);
        }

        PassiveButton creditsButton = doNotPressButton.GetComponent<PassiveButton>();
        creditsButton.OnClick.RemoveAllListeners();
        creditsButton.OnMouseOver.RemoveAllListeners();
        creditsButton.OnMouseOut.RemoveAllListeners();

        creditsButton.OnMouseOver.AddListener(() =>
        {
            pressedSprite.enabled = true;
            unpressedSprite.enabled = false;
        });

        creditsButton.OnMouseOut.AddListener(() =>
        {
            pressedSprite.enabled = false;
            unpressedSprite.enabled = true;
        });

        creditsButton.OnClick.AddListener(() => { _creditsScreen.SetActive(true); });

        creditsButton.transform.localScale = 0.9f * Vector3.one;
    }
}
