using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Submerged.KillAnimation.Patches;
using Submerged.Loading;
using Submerged.Map;
using UnityEngine;

namespace Submerged.Debugging.Tabs;

public class ModdingTab : IDebugTab
{
    public string Name => "Modding";
    public bool ShouldShow => true;

    public void BuildGUI()
    {
        GUILayout.Label($"FPS: {Mathf.RoundToInt(1f / Time.smoothDeltaTime)}");

        if (GUILayout.Button("Widescreen Fullscreen"))
        {
            Screen.SetResolution(2560, 1080, true);
            ResolutionManager.SetResolution(2560, 1080, true);
        }

        if (GUILayout.Button("Load Unity Explorer"))
        {
            Assembly unityExplorerAssembly = Assembly.LoadFile(Path.Combine(Paths.PluginPath, "sinai-dev-UnityExplorer", "UnityExplorer.plugin"));
            ((BasePlugin) Activator.CreateInstance(unityExplorerAssembly.GetType("UnityExplorer.ExplorerBepInPlugin")!))!.Load();
        }

        if (GUILayout.Button("Hide Loading"))
        {
            LoadingManager.loadingers.Clear();
        }

        if (!PlayerControl.LocalPlayer || !ShipStatus.Instance) return;

        if (GUILayout.Button("Open Task Picker"))
        {
            Minigame minigamePrefab = MapLoader.Skeld.transform.GetComponentsInChildren<SystemConsole>().First(c => c.FreeplayOnly).MinigamePrefab;
            PlayerControl.LocalPlayer.NetTransform.Halt();
            Minigame minigame = UnityObject.Instantiate(minigamePrefab, Camera.main!.transform, false);
            minigame.transform.localPosition = new Vector3(0f, 0f, -50f);
            minigame.Begin(null);
        }

        PlayerControl.LocalPlayer.Collider.enabled = GUILayout.Toggle(PlayerControl.LocalPlayer.Collider.enabled, "Enable Collision", new GUIStyle(GUI.skin.button));

        if (GUILayout.Button("Murder Self (O2)"))
        {
            OxygenDeathAnimationPatches.IsOxygenDeath = true;
            OxygenDeathRpcPatches.MurderPlayerAsOxygenDeath = true;

            try
            {
                PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer, true);
            }
            finally
            {
                OxygenDeathRpcPatches.MurderPlayerAsOxygenDeath = false;
                OxygenDeathAnimationPatches.IsOxygenDeath = false;
            }
        }

        if (GUILayout.Button("Call Meeting"))
        {
            PlayerControl.AllPlayerControls.ToArray().First(p => !p.Data.IsDead).CmdReportDeadBody(null);
        }

        if (GUILayout.Button("Teleport Dummy")) PlayerControl.AllPlayerControls.ToArray().First(p => p.isDummy).transform.position = PlayerControl.LocalPlayer.transform.position;

        if (GUILayout.Button("Customise Dummy"))
        {
            PlayerControl dummy = PlayerControl.AllPlayerControls.ToArray().First(p => p.isDummy);
            dummy.SetHat(PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId, 0);
        }

        if (GUILayout.Button("Kill Dummy")) PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.AllPlayerControls.ToArray().First(p => p.isDummy), true);
        if (GUILayout.Button("Dummy Kill Me")) PlayerControl.AllPlayerControls.ToArray().First(p => p.isDummy).RpcMurderPlayer(PlayerControl.LocalPlayer, true);
    }
}
