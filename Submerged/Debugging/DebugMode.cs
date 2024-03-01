using System.Diagnostics;
using System.Linq;
using AmongUs.GameOptions;
using Submerged.Debugging.Tabs;

namespace Submerged.Debugging;

public static class DebugMode
{
    [Conditional("DEBUG")]
    public static void Initialize(SubmergedPlugin plugin)
    {
        plugin.AddComponent<DebugWindow>();
        DebugWindow.Instance.Tabs.Add(new SubmergedTab());
        DebugWindow.Instance.Tabs.Add(new ModdingTab());
        DebugWindow.Instance.Tabs.Add(new KillAnimEditorTab());
        GameOptionsData.MaxImpostors = Enumerable.Repeat(3, 16).ToArray();
        HideNSeekGameOptionsV07.MaxImpostors = Enumerable.Repeat(1, 16).ToArray();
    }
}
