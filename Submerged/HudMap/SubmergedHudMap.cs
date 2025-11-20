using Il2CppInterop.Runtime.InteropTypes.Fields;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Submerged.HudMap;

[RegisterInIl2Cpp]
public sealed class SubmergedHudMap(nint ptr) : MonoBehaviour(ptr)
{
    public const float LOWER_Y = 8.375f;

    [UsedImplicitly]
    public Il2CppReferenceField<Transform> hudTransform;

    [UsedImplicitly]
    public Il2CppReferenceField<MapBehaviour> map;

    [UsedImplicitly]
    public Il2CppReferenceField<GameObject> upArrow;

    [UsedImplicitly]
    public Il2CppReferenceField<GameObject> downArrow;

    private bool _disableChangeFloor;
    private bool _startedOnTop;

    private void Start()
    {
        if (!GameManager.Instance.IsHideAndSeek()) return;

        upArrow.Value.gameObject.SetActive(false);
        downArrow.Value.gameObject.SetActive(false);
        _disableChangeFloor = true;
    }

    private void Update()
    {
        if (_disableChangeFloor) return;

        switch (Input.mouseScrollDelta.y)
        {
            case < 0:
                MoveMapDown();
                break;

            case > 0:
                MoveMapUp();
                break;
        }

        if (!map.Value.countOverlay.isActiveAndEnabled) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveMapUp();
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveMapDown();
        }
    }

    private void OnEnable()
    {
        if (PlayerControl.LocalPlayer.transform.position.y < -10f)
        {
            MoveMapDown();
        }
        else
        {
            MoveMapUp();
            _startedOnTop = true;
        }
    }

    public void MoveMapUp()
    {
        upArrow.Value.SetActive(false);
        downArrow.Value.SetActive(!_disableChangeFloor);
        hudTransform.Value.localPosition = Vector3.zero;

        foreach (UiElement button in map.Value.detectiveLocationControllerButtons)
        {
            if (button.name.EndsWith("_Scrolled") == _startedOnTop)
            {
                button.transform.localPosition -= new Vector3(0, LOWER_Y, 0);
                button.name = !_startedOnTop ? button.name + "_Scrolled" : button.name[..^"_Scrolled".Length];
            }
        }
    }

    public void MoveMapDown()
    {
        upArrow.Value.SetActive(!_disableChangeFloor);
        downArrow.Value.SetActive(false);
        hudTransform.Value.localPosition = new Vector3(0, LOWER_Y, 0);

        foreach (UiElement button in map.Value.detectiveLocationControllerButtons)
        {
            if (button.name.EndsWith("_Scrolled") != _startedOnTop)
            {
                button.transform.localPosition += new Vector3(0, LOWER_Y, 0);
                button.name = _startedOnTop ? button.name + "_Scrolled" : button.name[..^"_Scrolled".Length];
            }
        }
    }
}
