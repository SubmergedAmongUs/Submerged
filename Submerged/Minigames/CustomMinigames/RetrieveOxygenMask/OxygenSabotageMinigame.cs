using Reactor.Utilities.Attributes;
using Submerged.Enums;
using Submerged.Extensions;
using Submerged.Minigames.MonoBehaviours;
using Submerged.Systems.Oxygen;
using TMPro;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.RetrieveOxygenMask;

[RegisterInIl2Cpp]
public sealed class OxygenSabotageMinigame(nint ptr) : Minigame(ptr)
{
    private Transform _background;
    private Draggable _mask;

    private SubmarineOxygenSystem _oxygenSystem;
    private TextMeshPro _text;
    private Draggable _window;

    public void Start()
    {
        _oxygenSystem = MyTask.Cast<OxygenSabotageTask>().system;
        _background = transform.Find("Background");
        _text = _background.Find("Text").GetComponent<TextMeshPro>();
        _text.transform.SetZLocalPos(_text.transform.localPosition.z - 1f);
        SetupMask();
        SetupWindow();
    }

    private void Update()
    {
        _text.text = RemainingMasks.ToString("000");
    }

    private void SetupMask()
    {
        Transform maskTransform = transform.Find("Mask");

        if (!_oxygenSystem.LocalPlayerNeedsMask)
        {
            maskTransform.gameObject.SetActive(false);

            return;
        }

        _mask = maskTransform.gameObject.AddComponent<Draggable>();
        _mask.onTriggerExit += c =>
        {
            if (c.gameObject.name == "MaskBox" && amClosing == CloseState.None)
            {
                ShipStatus.Instance.RpcUpdateSystem(CustomSystemTypes.UpperCentral, 64);
                SubmarineOxygenSystem.Instance.RepairDamage(PlayerControl.LocalPlayer, 64);
                Complete();
            }
        };

        _mask.onDown += () => _background.Find("Hook").SetLocalZ(-0.25f);
        _mask.onUp += () =>
        {
            Vector3 pos = maskTransform.localPosition;
            pos.z = -1.5f;
            maskTransform.localPosition = pos;
        };

        _mask.onDrag += () =>
        {
            Vector3 pos = maskTransform.localPosition;
            pos.z = -15f;
            maskTransform.localPosition = pos;
        };

        _mask.forceStop = true;
    }

    private void SetupWindow()
    {
        Transform windowTransform = transform.Find("Window");

        Vector3 windowPos = windowTransform.localPosition;

        if (!_oxygenSystem.LocalPlayerNeedsMask)
        {
            if (!PlayerControl.LocalPlayer.Data.IsDead)
            {
                windowPos.y += 11.6f;
                windowTransform.localPosition = windowPos;
            }

            return;
        }

        _window = windowTransform.gameObject.AddComponent<Draggable>();
        _window.onDrag += () =>
        {
            Vector3 newPos = windowTransform.localPosition;
            newPos.x = windowPos.x;
            newPos.z = windowPos.z;
            float deltaY = newPos.y - windowPos.y;
            newPos.y = windowPos.y + Mathf.Clamp(deltaY, 0, 11.6f);
            windowTransform.localPosition = newPos;
            _mask.forceStop = deltaY < 10.85f;
        };
        // Window.OnUp += () => SoundManager.Instance.PlaySound(MinigameProperties.AudioClips[0], false, 1f);
    }

    private int RemainingMasks => _oxygenSystem.RemainingMasks;

    private void Complete()
    {
        _mask.enabled = false;
        StartCoroutine(CoStartClose());
    }
}
