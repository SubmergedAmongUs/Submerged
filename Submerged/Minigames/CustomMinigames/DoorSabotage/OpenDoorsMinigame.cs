using System.ComponentModel;
using System.Linq;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using Submerged.BaseGame.Interfaces;
using TMPro;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.DoorSabotage;

[RegisterInIl2Cpp(typeof(IDoorMinigame))]
public sealed class OpenDoorsMinigame(nint ptr) : OpenDoorsMinigameNoInterface(ptr), AU.IDoorMinigame
{
    public void SetDoor(OpenableDoor door)
    {
        myDoor = door;
    }
}

[RegisterInIl2Cpp]
[Description("Unhollower previously had a bug where you couldn't register a class that both inherited from an IL2CPP class and an IL2CPP interface. " +
    "This might have been fixed at some point but I'm not sure so just to be safe, I'm keeping it like this.")]
public class OpenDoorsMinigameNoInterface(nint ptr) : Minigame(ptr)
{
    public TextMeshPro character;
    public GameObject finishedScreen;
    public GameObject errorScreen;
    public GameObject handle;
    private readonly Controller _controller = new();
    private bool _complete;

    private Collider2D _handleCollider;
    private bool _letterSelected;

    private KeyCode _targetKey;
    private float _timer;

    protected OpenableDoor myDoor;

    public void Start()
    {
        character = transform.GetComponentInChildren<TextMeshPro>();
        finishedScreen = transform.Find("Finished Screen").gameObject;
        errorScreen = transform.Find("Error Screen").gameObject;
        handle = transform.Find("Rotated/Handle").gameObject;

        _targetKey = GetRandomKey();
        character.text = _targetKey.ToString()[^1..];
        _handleCollider = handle.GetComponent<Collider2D>();
    }

    public void Update()
    {
        _timer += Time.deltaTime * 0.25f;
        if (_timer >= 0.65f) _timer = 0;

        character.color = Color.LerpUnclamped(Color.white, new Color(1, 1, 1, 0.5f), Mathf.Sin(_timer * 10f));

        if (_complete) return;

        if (myDoor.IsOpen)
        {
            _complete = true;
            StartCoroutine(CoStartClose(0.25f));

            return;
        }

        _controller.Update();

        if (Input.GetKeyDown(_targetKey))
        {
            finishedScreen.SetActive(true);
            _letterSelected = true;
        }

        CheckHandle();
    }

    private KeyCode GetRandomKey()
    {
        return Enumerable.Range((int) KeyCode.A, 26)
            .Concat(Enumerable.Range((int) KeyCode.Alpha0, 10))
            .Select(k => (KeyCode) k)
            .Random();
    }

    public void Finish()
    {
        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, (byte) (myDoor.Id | 64));
        myDoor.SetDoorway(true);
        StartCoroutine(CoStartClose());
    }

    public void CheckHandle()
    {
        errorScreen.SetActive(false);

        switch (_controller.CheckDrag(_handleCollider))
        {
            case DragState.Dragging:
            {
                errorScreen.SetActive(!_letterSelected);

                if (!_letterSelected) return;

                Vector2 vector = handle.transform.position;
                float num = Vector2.SignedAngle(_controller.DragStartPosition - vector, _controller.DragPosition - vector);
                handle.transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Clamp(num, -90f, 0));

                return;
            }
            case DragState.Released:
            {
                if (!_letterSelected) return;
                float num2 = handle.transform.localEulerAngles.z;

                if (num2 > 180f)
                {
                    num2 -= 360f;
                }

                num2 %= 360f;

                if (Mathf.Abs(num2) > 80f)
                {
                    _complete = true;
                    Finish();

                    return;
                }

                handle.transform.localEulerAngles = new Vector3(0f, 0f, 0f);

                break;
            }
        }
    }
}
