using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Reflection;
using Reactor;
using Reactor.Localization;
using Reactor.Patches;
using Submerged.Debugging;
using Submerged.Enums;
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
    private static string _humanReadableVersion;

    public SubmergedPlugin()
    {
        InteropPatches.Initialize();

        Version version = Assembly.GetExecutingAssembly().GetName().Version!;
        _humanReadableVersion = $"{version.Major}.{version.Minor}.{version.Build}";

        if (IsDevBuild) _humanReadableVersion += "-dev";
    }

    public override void Load()
    {
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

        ReactorVersionShower.TextUpdated += text => text.text += $"\n{VersionText}";
    }

    private static bool IsDevBuild =>
#if DEBUG
        true;
#else
        false;
#endif

    internal static string VersionText => $"{(IsDevBuild ? "<color=red>" : "")}Submerged v{_humanReadableVersion}{(IsDevBuild ? "</color>" : "")}";
}
