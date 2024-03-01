using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.Debugging.Tabs;
using UnityEngine;

namespace Submerged.Debugging;

[RegisterInIl2Cpp]
public sealed class DebugWindow(nint ptr) : MonoBehaviour(ptr)
{
    private int _activeTabIdx;
    private Rect _windowRect = new(20, 20, 100, 100);

    public static DebugWindow Instance { get; private set; }

    [HideFromIl2Cpp]
    public bool Enabled { get; set; }

    [HideFromIl2Cpp]
    public string Title { get; set; } = "Debug Window";

    [HideFromIl2Cpp]
    public List<IDebugTab> Tabs { get; set; } = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.F1) && Input.GetKey(KeyCode.F2)) ||
            (Input.GetKeyDown(KeyCode.F2) && Input.GetKey(KeyCode.F1)))
            Enabled = !Enabled;
    }

    public void OnGUI()
    {
        if (!Enabled || !Tabs.Any()) return;
        _windowRect.height = _windowRect.width = 20;
        _windowRect = GUILayout.Window(0, _windowRect, (Action<int>) (_ => DrawWindow()), Title);

        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && _windowRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
        {
            Input.ResetInputAxes();
        }
    }

    private void DrawWindow()
    {
        if (Tabs.Count == 0) return;

        GUI.DragWindow(new Rect(0, 0, 10000, 20));
        GUILayout.BeginHorizontal(GUIStyle.none);

        for (int i = 0; i < Tabs.Count; i++)
        {
            IDebugTab currentTab = Tabs[i];

            if (!currentTab.ShouldShow)
            {
                if (_activeTabIdx == i)
                {
                    _activeTabIdx = (_activeTabIdx + 1) % Tabs.Count;
                }

                continue;
            }

            if (GUILayout.Toggle(_activeTabIdx == i, currentTab.Name, new GUIStyle(GUI.skin.button))) _activeTabIdx = i;
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(5f);
        Tabs[_activeTabIdx].BuildGUI();
    }
}
