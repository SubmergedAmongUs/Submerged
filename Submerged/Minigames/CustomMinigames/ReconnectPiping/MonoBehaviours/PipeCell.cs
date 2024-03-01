using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using Submerged.Minigames.CustomMinigames.ReconnectPiping.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace Submerged.Minigames.CustomMinigames.ReconnectPiping.MonoBehaviours;

[RegisterInIl2Cpp]
public sealed class PipeCell(nint ptr) : MonoBehaviour(ptr)
{
    public GameObject straight;
    public GameObject elbow;

    private PassiveButton _button;

    private bool _canClick = true;
    private int _queuedClicks;
    private ReconnectPipingMinigame _reconnectPipingMinigame;
    private bool _straightSet;

    private void Awake()
    {
        _reconnectPipingMinigame = transform.parent.GetComponentInParent<ReconnectPipingMinigame>();
        straight = transform.Find("Straight").gameObject;
        elbow = transform.Find("Elbow").gameObject;

        _button = gameObject.AddComponent<PassiveButton>();

        _button.Colliders = new[] { straight.GetComponent<Collider2D>(), elbow.GetComponent<Collider2D>() };
        _button.OnMouseOut = new Button.ButtonClickedEvent();
        _button.OnMouseOver = new Button.ButtonClickedEvent();

        _button.OnClick = new Button.ButtonClickedEvent();
        _button.OnClick.AddListener(Click);

        // TEMP
        straight.gameObject.SetActive(false);
        elbow.gameObject.SetActive(true);
    }

    private void Click()
    {
        if (!_canClick || _reconnectPipingMinigame.MyNormTask.IsComplete) return;
        _canClick = false;
        _queuedClicks++;

        if (_queuedClicks > 1) return;
        this.StartCoroutine(RotatePiece());
    }

    [HideFromIl2Cpp]
    public IEnumerator RotatePiece()
    {
        const float ROTATION_DURATION = 0.15f;

        while (_queuedClicks > 0)
        {
            Vector3 originalRotation = gameObject.transform.localEulerAngles;
            Vector3 newRotation = gameObject.transform.localEulerAngles;

            for (float t = 0; t < ROTATION_DURATION; t += Time.deltaTime)
            {
                newRotation.z = Mathf.Lerp(originalRotation.z, originalRotation.z - 90, t / ROTATION_DURATION);
                gameObject.transform.localEulerAngles = newRotation;

                yield return null;
            }

            gameObject.transform.localEulerAngles = new Vector3(0, 0, originalRotation.z - 90);
            _queuedClicks--;

            yield return null;
        }

        _reconnectPipingMinigame.CheckComplete();
        _canClick = true;
    }

    public void SetPiece(Direction firstDirection, Direction secondDirection, bool randomiseRotation = true)
    {
        GameObject gameObj = gameObject;

        if (((int) firstDirection + (int) secondDirection) % 2 == 0)
        {
            straight.gameObject.SetActive(true);
            elbow.gameObject.SetActive(false);
            _straightSet = true;
        }
        else
        {
            straight.gameObject.SetActive(false);
            elbow.gameObject.SetActive(true);
            _straightSet = false;
        }

        if (randomiseRotation)
        {
            for (int i = 0; i < UnityRandom.Range(0, 4); i++)
            {
                Vector3 rotation = gameObject.transform.localEulerAngles;
                rotation.z -= 90;
                gameObj.transform.localEulerAngles = rotation;
            }
        }
        else
        {
            if (_straightSet)
            {
                if (firstDirection == Direction.East || secondDirection == Direction.East)
                {
                    Vector3 rotation = gameObject.transform.localEulerAngles;
                    rotation.z -= 90;
                    gameObj.transform.localEulerAngles = new Vector3(0, 0, -90);
                }
            }
            else
            {
                switch ((int) firstDirection + (int) secondDirection)
                {
                    case 1: // NE
                        gameObj.transform.localEulerAngles = new Vector3(0, 0, 90);

                        break;
                    case 3: // SE or NW
                        gameObj.transform.localEulerAngles = new Vector3(0, 0, -180);
                        if (firstDirection == Direction.South || secondDirection == Direction.South) gameObj.transform.localEulerAngles = new Vector3(0, 0, 0);

                        break;
                    case 5: // SW
                        gameObj.transform.localEulerAngles = new Vector3(0, 0, -90);

                        break;
                }
            }
        }
    }

    public void SetRandom()
    {
        List<Direction> directions =
        [
            Direction.North,
            Direction.East,
            Direction.South,
            Direction.West
        ];

        directions.Shuffle();
        SetPiece(directions[0], directions[1]);
    }

    public Direction GetNextDirection(Direction direction)
    {
        float currentAngle = Mathf.Abs(gameObject.transform.localEulerAngles.z % 360 + 360) % 360;

        if (_straightSet)
        {
            switch (currentAngle)
            {
                case 0:
                case 180:
                    if (direction == Direction.South) return Direction.North;
                    if (direction == Direction.North) return Direction.South;

                    break;
                case 90:
                case 270:
                    if (direction == Direction.East) return Direction.West;
                    if (direction == Direction.West) return Direction.East;

                    break;
            }

            switch (direction)
            {
                case Direction.North:
                case Direction.South:
                    if (currentAngle % 180 == 0) return (Direction) (((int) direction + 2) % 4);

                    break;
                case Direction.East:
                case Direction.West:
                    if ((currentAngle + 90) % 180 == 0) return (Direction) (((int) direction + 2) % 4);

                    break;
            }
        }
        else
        {
            switch (currentAngle)
            {
                case 0:
                    if (direction == Direction.South) return Direction.East;
                    if (direction == Direction.East) return Direction.South;

                    break;
                case 90:
                    if (direction == Direction.North) return Direction.East;
                    if (direction == Direction.East) return Direction.North;

                    break;
                case 180:
                    if (direction == Direction.North) return Direction.West;
                    if (direction == Direction.West) return Direction.North;

                    break;
                case 270:
                    if (direction == Direction.South) return Direction.West;
                    if (direction == Direction.West) return Direction.South;

                    break;
            }
        }

        return direction;
    }
}
