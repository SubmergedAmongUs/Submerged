using System.ComponentModel;
using Reactor.Utilities.Attributes;
using Submerged.BaseGame.Interfaces;
using Submerged.Extensions;
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
[Description("Yes, this is a bug with Unhollower. No, we are not going to report it as it would be hard to reproduce and we are busy people.")]
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

    private char _targetLetter;
    private float _timer;

    protected OpenableDoor myDoor;

    public void Start()
    {
        character = transform.GetComponentInChildren<TextMeshPro>();
        finishedScreen = transform.Find("Finished Screen").gameObject;
        errorScreen = transform.Find("Error Screen").gameObject;
        handle = transform.Find("Rotated/Handle").gameObject;

        _targetLetter = SetRandomChar();
        character.text = _targetLetter.ToString();
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

        if (Input.GetKeyDown(_targetLetter.ToString().ToLower()))
        {
            finishedScreen.SetActive(true);
            _letterSelected = true;
        }

        CheckHandle();
    }

    public char SetRandomChar()
    {
        char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

        return chars.Random();
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
