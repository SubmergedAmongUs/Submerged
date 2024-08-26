using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor;
using Reactor.Localization;
using Reactor.Utilities;
using Submerged.Debugging;
using Submerged.Enums;
using Submerged.Extensions;
using Submerged.IL2CPP;
using Submerged.Loading;
using Submerged.Localization;
using Submerged.Map;
using Submerged.Resources;

namespace Submerged;

[BepInAutoPlugin]
[BepInDependency(ReactorPlugin.Id)]
public sealed partial class SubmergedPlugin : BasePlugin
{
    public SubmergedPlugin()
    {
        InteropPatches.Initialize();
    }

    public override void Load()
    {
        ReactorCredits.Register<SubmergedPlugin>(location =>
            location == ReactorCredits.Location.MainMenu ||
            ShipStatus.Instance.IsSubmerged() ||
            (LobbyBehaviour.Instance && GameManager.Instance && GameManager.Instance.LogicOptions?.MapId == (byte) CustomMapTypes.Submerged));

        Harmony.CreateAndPatchAll(GetType().Assembly, Id);

        DebugMode.Initialize(this);

        ResourceManager.CacheSprite("AdminButton", 115.46f);
        ResourceManager.CacheSprite("CreateGameBG");
        ResourceManager.CacheSprite("FilterIcon");
        ResourceManager.CacheSprite("FloorDown");
        ResourceManager.CacheSprite("FloorDownHover");
        ResourceManager.CacheSprite("FloorUp");
        ResourceManager.CacheSprite("FloorUpHover");
        ResourceManager.CacheSprite("Logo", 400);
        ResourceManager.CacheSprite("Logo", 250, "OptionsLogo");
        ResourceManager.CacheSprite("OptionsBG");
        ResourceManager.CacheSprite("OptionsIcon", 90);
        ResourceManager.CacheSprite("ReportDisabled");

        LoadingManager.RegisterLoading(nameof(AssetLoader));
        LoadingManager.RegisterLoading(nameof(MapLoader));
        AddComponent<LoadingManager>();
        AddComponent<AssetLoader>();

        CustomSystemTypes.Initialize();

        LocalizationManager.Register(new SubmergedLocalizationProvider());
    }
}
