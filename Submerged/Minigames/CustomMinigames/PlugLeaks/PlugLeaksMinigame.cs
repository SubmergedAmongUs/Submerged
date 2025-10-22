using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using Submerged.Minigames.MonoBehaviours;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.PlugLeaks;

[RegisterInIl2Cpp]
public sealed class PlugLeaksMinigame(nint ptr) : Minigame(ptr)
{
    private const float ANGLE_OFFSET = 24.65f;

    private static readonly int _redColor = Shader.PropertyToID("_RedColor");

    public Color[] redColors =
    [
        new(0.3199982f, 0.4059609f, 0.4433962f, 1f),
        new(0.4528302f, 0.4147556f, 0.3438946f, 1f),
        new(0.3867925f, 0.3338603f, 0.2791474f, 1f),
        new(0.5188679f, 0.5188679f, 0.5188679f, 1f),
        new(0.3199982f, 0.4059609f, 0.4433962f, 1f),
        new(0.4528302f, 0.4147556f, 0.3438946f, 1f)
    ];

    public GameObject[] backgrounds;
    public SpriteRenderer redRip;

    public Transform tapeTransform;
    public Draggable tapeDraggable;

    public Transform tapeStrip;

    public PolygonCollider2D crackCollider;
    public List<Vector2> colliderPoints;
    public List<BoxCollider2D> tapeStripColliders = [];
    private bool _blockClose;

    private int _count;
    private Transform _currentTape;

    private float _delta;
    private float _lastTapeAngle = 24.65f;

    private MinigameProperties _minigameProperties;

    // Engines = 0
    // Ballast = 1
    // Balalst Hallway = 2
    // Electrical = 3
    // Lobby = 4
    // Research = 5

    private void Start()
    {
        _minigameProperties = GetComponent<MinigameProperties>();

        crackCollider = transform.Find("Layers/Crack").GetComponent<PolygonCollider2D>();

        backgrounds = transform.Find("Backgrounds").GetChildren().Select(t => t.gameObject).ToArray();
        redRip = transform.Find("Layers/Rip").GetComponent<SpriteRenderer>();

        int roomId = 0;

        switch (ConsoleId)
        {
            case 0:
                roomId = 2;
                break;

            case 1:
                roomId = 0;
                break;

            case 2:
                roomId = 3;
                break;

            case 3:
                roomId = 1;
                break;

            case 4:
                roomId = 4;
                break;

            case 5:
                roomId = 5;
                break;
        }

        foreach (GameObject obj in backgrounds) obj.SetActive(false);
        backgrounds[roomId].SetActive(true);
        redRip.material.SetColor(_redColor, redColors[roomId]);

        tapeTransform = transform.Find("Tape");
        tapeDraggable = tapeTransform.gameObject.AddComponent<Draggable>();
        tapeStrip = transform.Find("TapeStrip");
        tapeDraggable.onUp += () =>
        {
            if (colliderPoints == null)
            {
                colliderPoints = [];
                Il2CppStructArray<Vector2> path = crackCollider.GetPath(0);

                foreach (Vector2 point in path)
                {
                    colliderPoints.Add(crackCollider.transform.TransformPoint(point));
                }
            }

            if (_currentTape) return;
            SoundManager.Instance.PlaySound(_minigameProperties.audioClips[0], false);
            _currentTape = Instantiate(tapeStrip.gameObject, transform).transform;

            _currentTape.gameObject.SetActive(true);
            Vector3 pos = tapeTransform.position;
            _count++;
            pos.z = tapeStrip.position.z - _count * 0.01f;
            _currentTape.position = pos;
        };
        tapeDraggable.onOver += () => _blockClose = true;
        tapeDraggable.onExit += () => _blockClose = false;

        if (UnityRandom.RandomRangeInt(0, 2) == 0)
        {
            foreach (GameObject obj in _minigameProperties.gameObjects)
            {
                obj.transform.SetXScale(-1);
            }
        }
    }

    private void LateUpdate()
    {
        _delta = -1;
        if (_currentTape != null) UpdateRollAndStripRotation();
        tapeTransform.localEulerAngles = new Vector3(0, 0, _lastTapeAngle);

        if (_delta > 8.32)
        {
            SoundManager.Instance.PlaySound(_minigameProperties.audioClips[1], false);
            tapeStripColliders.Add(_currentTape.GetComponent<BoxCollider2D>());

            foreach (BoxCollider2D collider in tapeStripColliders)
            {
                List<Vector2> points = [..colliderPoints];

                foreach (Vector2 point in points)
                {
                    if (collider.OverlapPoint(point))
                    {
                        colliderPoints.Remove(point);
                    }
                }
            }

            if (colliderPoints.Count <= 5)
            {
                CompleteTask();
            }

            _currentTape.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.None;
            _currentTape = null;
        }
    }

    private void UpdateRollAndStripRotation()
    {
        Vector3 delta = tapeTransform.position - _currentTape.position;
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

        _lastTapeAngle = angle + ANGLE_OFFSET;
        _currentTape.localEulerAngles = new Vector3(0, 0, angle);
        _delta = delta.sqrMagnitude;
    }

    private void CompleteTask()
    {
        tapeDraggable.forceStop = true;
        PlugLeaksTask customTask = MyNormTask.Cast<PlugLeaksTask>();
        customTask.validConsoleIds.Remove(ConsoleId);
        if (MyNormTask) MyNormTask.NextStep();
        StartCoroutine(CoStartClose());
    }

    [UsedImplicitly]
    private void TryClose()
    {
        if (!_blockClose)
        {
            GetComponent<MinigameProperties>().CloseTask();
        }
    }
}
