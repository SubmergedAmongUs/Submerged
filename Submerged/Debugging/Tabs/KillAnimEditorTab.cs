using System;
using System.Collections.Generic;
using System.Linq;
using Submerged.KillAnimation;
using Submerged.KillAnimation.Patches;
using UnityEngine;

namespace Submerged.Debugging.Tabs;

public sealed class KillAnimEditorTab : IDebugTab
{
    private const float ANIM_DURATION = 2.5f;

    private readonly List<KillAnimFrame> _frames = [];
    private readonly Dictionary<int, KillAnimFrame> _previewLookup = new();

    private int _currentAnimation;

    private int _currentFrame = -1;
    private int _currentLength;
    private Vector2 _currentOffset;
    private float _currentTime;

    private CustomKillAnimationPlayer _customKillAnimation;
    private bool _isLooping;
    private bool _isPlaying;

    private EditMode _mode = 0;
    private int _oldFrameCount;
    private float _playhead;

    public void BuildGUI()
    {
        _customKillAnimation = UnityObject.FindObjectOfType<CustomKillAnimationPlayer>();

        if (!_customKillAnimation)
        {
            if (!GUILayout.Button("Show Kill Animation")) return;
            OxygenDeathAnimationPatches.IsOxygenDeath = true;

            try
            {
                HudManager.Instance.KillOverlay.ShowKillAnimation(PlayerControl.LocalPlayer.Data, PlayerControl.LocalPlayer.Data);
            }
            finally
            {
                OxygenDeathAnimationPatches.IsOxygenDeath = false;
            }

            return;
        }

        _customKillAnimation.enabled = false;

        GUILayout.BeginHorizontal();

        if (GUILayout.Toggle(_mode == EditMode.Select, nameof(EditMode.Select), new GUIStyle(GUI.skin.button))) _mode = EditMode.Select;
        GUI.enabled = _frames.Any();
        if (GUILayout.Toggle(_mode == EditMode.Edit, nameof(EditMode.Edit), new GUIStyle(GUI.skin.button))) _mode = EditMode.Edit;
        if (GUILayout.Toggle(_mode == EditMode.Preview, nameof(EditMode.Preview), new GUIStyle(GUI.skin.button))) _mode = EditMode.Preview;
        GUI.enabled = true;
        if (GUILayout.Toggle(_mode == EditMode.Save, nameof(EditMode.Save), new GUIStyle(GUI.skin.button))) _mode = EditMode.Save;

        GUILayout.EndHorizontal();

        if (_mode == EditMode.Select) BuildSelectMode();
        if (_mode == EditMode.Edit) BuildEditMode();
        if (_mode == EditMode.Preview) BuildPreviewMode();
        if (_mode == EditMode.Save) BuildSaveMode();

        _customKillAnimation.UpdateVisuals(_currentTime, _currentOffset, _currentAnimation);
    }

    public string Name => "Kill Anim";
    public bool ShouldShow => HudManager.InstanceExists && PlayerControl.LocalPlayer;

    private void BuildSelectMode()
    {
        _currentFrame = -1;
        _previewLookup.Clear();
        _isPlaying = false;
        _playhead = 0;

        GUILayout.BeginHorizontal();

        if (GUILayout.Toggle(_currentAnimation == 0, HudManager.Instance.KillOverlay.KillAnims[0].KillType.ToString(), new GUIStyle(GUI.skin.button))) _currentAnimation = 0;
        if (GUILayout.Toggle(_currentAnimation == 1, HudManager.Instance.KillOverlay.KillAnims[1].KillType.ToString(), new GUIStyle(GUI.skin.button))) _currentAnimation = 1;
        if (GUILayout.Toggle(_currentAnimation == 2, HudManager.Instance.KillOverlay.KillAnims[2].KillType.ToString(), new GUIStyle(GUI.skin.button))) _currentAnimation = 2;
        if (GUILayout.Toggle(_currentAnimation == 3, HudManager.Instance.KillOverlay.KillAnims[3].KillType.ToString(), new GUIStyle(GUI.skin.button))) _currentAnimation = 3;

        GUILayout.EndHorizontal();

        if (GUILayout.Button("Reset"))
        {
            _currentTime = 0;
            _currentOffset = Vector3.zero;
        }

        GUILayout.Label("Time: " + _currentTime + " / " + ANIM_DURATION);
        _currentTime = Mathf.Round(GUILayout.HorizontalSlider(_currentTime, 0, ANIM_DURATION) * 100) / 100;

        if (GUILayout.Button("Add Frame"))
            _frames.Add(new KillAnimFrame
            {
                animation = _currentAnimation,
                time = _currentTime,
                length = 1,
                offset = Vector2.zero
            });
        if (!_frames.Any()) GUI.enabled = false;
        if (GUILayout.Button("Clear Frames")) _frames.Clear();
        GUI.enabled = true;

        for (int i = 0; i < _frames.Count; i++)
        {
            KillAnimFrame frame = _frames[i];

            GUILayout.BeginHorizontal();
            GUILayout.Label($"{i}: {HudManager.Instance.KillOverlay.KillAnims[frame.animation].KillType} / {frame.time:0.00}");

            if (GUILayout.Button("L", GUILayout.Width(25)))
            {
                _currentAnimation = frame.animation;
                _currentTime = frame.time;
                _currentOffset = frame.offset;
            }

            if (i == 0) GUI.enabled = false;

            if (GUILayout.Button("/\\", GUILayout.Width(25)))
            {
                _frames.RemoveAt(i);
                _frames.Insert(i - 1, frame);
            }

            GUI.enabled = true;

            if (i == _frames.Count - 1) GUI.enabled = false;

            if (GUILayout.Button("\\/", GUILayout.Width(25)))
            {
                _frames.RemoveAt(i);
                _frames.Insert(i + 1, frame);
            }

            GUI.enabled = true;

            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                _frames.RemoveAt(i);
                i--;
            }

            GUILayout.EndHorizontal();
        }
    }

    private void BuildEditMode()
    {
        _previewLookup.Clear();
        _isPlaying = false;
        _playhead = 0;

        for (int i = 0; i < _frames.Count; i++)
        {
            KillAnimFrame frame = _frames[i];

            GUILayout.BeginHorizontal();
            GUILayout.Label($"{i}: {frame.length} / {frame.offset}");
            if (_currentFrame == i) GUI.enabled = false;

            if (GUILayout.Button("Modify"))
            {
                _currentAnimation = frame.animation;
                _currentTime = frame.time;
                _currentFrame = i;
                _currentLength = frame.length;
                _currentOffset = frame.offset;
            }

            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }

        if (_currentFrame >= _frames.Count) _currentFrame = _frames.Count - 1;

        if (_currentFrame == -1) GUI.enabled = false;
        if (GUILayout.Button("Deselect")) _currentFrame = -1;
        GUI.enabled = true;

        if (_currentFrame >= 0)
        {
            if (GUILayout.Button("Reset"))
            {
                _currentLength = 0;
                _currentOffset = Vector3.zero;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Length: " + _currentLength);
            _currentLength = (int) GUILayout.HorizontalSlider(_currentLength, 1, 20);
            GUILayout.EndHorizontal();

            GUILayout.Label("Offset: " + _currentOffset);

            GUILayout.BeginHorizontal();
            GUILayout.Label("X: ");
            _currentOffset.x = Mathf.Round(GUILayout.HorizontalSlider(_currentOffset.x, -3, 3) * 10) / 10;
            if (GUILayout.Button("0", GUILayout.Width(25))) _currentOffset.x = 0;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Y: ");
            _currentOffset.y = Mathf.Round(GUILayout.HorizontalSlider(_currentOffset.y, -3, 3) * 10) / 10;
            if (GUILayout.Button("0", GUILayout.Width(25))) _currentOffset.y = 0;
            GUILayout.EndHorizontal();

            KillAnimFrame currentFrame = _frames[_currentFrame];
            currentFrame.length = _currentLength;
            currentFrame.offset = _currentOffset;
            _frames[_currentFrame] = currentFrame;
        }
    }

    private void BuildPreviewMode()
    {
        _currentFrame = -1;

        if (!_previewLookup.Any())
        {
            int pos = 0;

            foreach (KillAnimFrame frame in _frames)
            {
                for (int i = pos; i < pos + frame.length; i++)
                {
                    _previewLookup[i] = frame;
                }

                pos += frame.length;
            }

            if (pos != 0) _previewLookup[pos] = _previewLookup[pos - 1];
        }

        if (!_isPlaying)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Start"))
            {
                _isPlaying = true;
                _playhead = 0;
            }

            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Stop")) _isPlaying = false;
            GUILayout.EndHorizontal();
        }

        _isLooping = GUILayout.Toggle(_isLooping, "Loop");

        if (_playhead >= ANIM_DURATION)
        {
            if (!_isLooping)
            {
                _playhead = ANIM_DURATION;
                _isPlaying = false;
            }
            else
            {
                _playhead = 0;
            }
        }

        if (_isPlaying && _oldFrameCount != Time.frameCount)
        {
            _playhead += Time.deltaTime;
            _oldFrameCount = Time.frameCount;
        }

        GUILayout.Label("Time: " + _playhead + " / " + ANIM_DURATION);
        _playhead = Mathf.Round(GUILayout.HorizontalSlider(_playhead, 0, ANIM_DURATION) * 100) / 100;

        int frameAmount = _frames.Sum(f => f.length);
        float timePerFrame = ANIM_DURATION / frameAmount;
        int currentPos = Mathf.FloorToInt(_playhead / timePerFrame);

        GUILayout.Label("FPS: " + frameAmount / ANIM_DURATION);

        KillAnimFrame currentFrame = _previewLookup[currentPos];

        _currentAnimation = currentFrame.animation;
        _currentTime = currentFrame.time;
        _currentOffset = currentFrame.offset;
    }

    private void BuildSaveMode()
    {
        _currentFrame = -1;
        _previewLookup.Clear();
        _isPlaying = false;
        _playhead = 0;

        if (GUILayout.Button("Load Actual")) LoadCurrent();

        for (int i = 1; i <= 5; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Slot {i}: ");
            if (GUILayout.Button("Save")) SaveTo($"KillAnimFramesSlot{i}");
            if (!PlayerPrefs.HasKey($"KillAnimFramesSlot{i}")) GUI.enabled = false;
            if (GUILayout.Button("Load")) LoadFrom($"KillAnimFramesSlot{i}");
            if (GUILayout.Button("Delete")) PlayerPrefs.DeleteKey($"KillAnimFramesSlot{i}");
            if (GUILayout.Button("Log")) Message(PlayerPrefs.GetString($"KillAnimFramesSlot{i}"));
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }
    }

    private void SaveTo(string key)
    {
        string toSave = _frames.Aggregate("", (current, frame) => current + KillAnimFrame.Serialize(frame) + ";");
        PlayerPrefs.SetString(key, toSave);
    }

    private void LoadFrom(string key)
    {
        string toLoad = PlayerPrefs.GetString(key);
        _frames.Clear();
        string[] data = toLoad.Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (string frameData in data)
        {
            KillAnimFrame frame = KillAnimFrame.Deserialize(frameData);
            _frames.Add(frame);
        }
    }

    private void LoadCurrent()
    {
        const string TO_LOAD = CustomKillAnimationPlayer.OXYGEN_DEATH_ANIM;
        _frames.Clear();
        string[] data = TO_LOAD.Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (string frameData in data)
        {
            KillAnimFrame frame = KillAnimFrame.Deserialize(frameData);
            _frames.Add(frame);
        }
    }

    private enum EditMode
    {
        Select,
        Edit,
        Preview,
        Save
    }
}
