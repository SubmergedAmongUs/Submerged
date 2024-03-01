using Submerged.Floors;
using Submerged.Map;
using Submerged.Minigames.CustomMinigames.FixWiring.MonoBehaviours;
using UnityEngine;

namespace Submerged.Debugging.Tabs;

public class SubmergedTab : IDebugTab
{
    public string Name => "Submerged";
    public bool ShouldShow => SubmarineStatus.instance != null && PlayerControl.LocalPlayer;

    public void BuildGUI()
    {
        if (!PlayerControl.LocalPlayer) return;

        if (GUILayout.Button("Go To Upper Deck")) FloorHandler.LocalPlayer.RpcRequestChangeFloor(true);
        if (GUILayout.Button("Go To Lower Deck")) FloorHandler.LocalPlayer.RpcRequestChangeFloor(false);

        if (GUILayout.Button("Reset Wires Easter Egg")) PlayerPrefs.SetInt(KcegListener.PLAYER_PREFS_KEY, 0);
    }
}
