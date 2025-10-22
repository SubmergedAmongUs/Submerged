using System.Collections.Generic;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using Submerged.Minigames.MonoBehaviours;
using Submerged.Localization.Strings;
using TMPro;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.TrackMantaRay;

[RegisterInIl2Cpp]
public sealed class TrackMantaMinigame(nint ptr) : Minigame(ptr)
{
    private const float LERP_TIME = 0.3f;

    private const float MAXIMUM_ROTATION_PER_SEC = 135f;
    private const float SWIM_SPEED = 2.22f * 8f;
    private const float TIME_TO_TRACK = 5f;

    private readonly List<Vector2> _positions = [];

    private bool _amTracking;
    private Vector2 _lastPosition;

    private float _lerpTimer;
    private ClickableSprite _mantaButton;

    private Transform _mantaRay;
    private Vector2 _nextPosition;
    private bool _outOfRange;

    private TextMeshPro[] _textMeshPros;
    private float _timeTracked;

    private ClickableSprite _trackingCollider;

    private void Start()
    {
        _trackingCollider = transform.Find("TrackingCollider").gameObject.AddComponent<ClickableSprite>();
        _mantaRay = transform.Find("MantaRay");
        _mantaButton = _mantaRay.gameObject.AddComponent<ClickableSprite>();

        _mantaButton.onOver += () => _amTracking = true;
        _mantaButton.onExit += () => _amTracking = false;

        _trackingCollider.onOver += () => _outOfRange = true;
        _trackingCollider.onExit += () => _outOfRange = false;

        _textMeshPros = GetComponentsInChildren<TextMeshPro>();

        for (float x = -2f; x < 2f; x += 0.2f)
            for (float y = -2f; y < 2f; y += 0.2f)
                _positions.Add(new Vector2(x * 5f, y * 5f));
        GetNextWaypoint();
    }

    // Update is called once per frame
    private void Update()
    {
        if (_amTracking && !_outOfRange) _timeTracked += Time.deltaTime;

        _lerpTimer += Time.deltaTime;
        Vector2 fishTarget = _nextPosition;
        Transform mantaRayTransform = _mantaRay.transform;
        Vector3 position = mantaRayTransform.localPosition;
        TurnToVector(fishTarget);
        position += mantaRayTransform.up * (SWIM_SPEED * Time.deltaTime);
        mantaRayTransform.localPosition = position;

        if (_lerpTimer >= LERP_TIME)
        {
            GetNextWaypoint();
            _lerpTimer = 0;
        }

        int percent = Mathf.FloorToInt(_timeTracked * 100 / TIME_TO_TRACK);

        if (percent >= 100)
        {
            if (amClosing != CloseState.None) return;
            if (MyNormTask != null)
            {
                MyNormTask.NextStep();
            }
            StartCoroutine(CoStartClose());

            foreach (TextMeshPro t in _textMeshPros)
            {
                t.text = string.Format(Tasks.TrackMantaRay_Tracking, 100);
            }
        }
        else
        {
            foreach (TextMeshPro t in _textMeshPros)
            {
                t.text = string.Format(Tasks.TrackMantaRay_Tracking, $"{percent:00}");
            }
        }
    }

    private void GetNextWaypoint()
    {
        _lastPosition = _mantaRay.position;
        _positions.Remove(_lastPosition);
        _nextPosition = _positions.Random();
        _positions.Add(_lastPosition);
    }

    [HideFromIl2Cpp]
    public void TurnToVector(Vector2 location)
    {
        float currentAngle = _mantaRay.eulerAngles.z;
        Vector3 itemPosition = _mantaRay.localPosition;

        float deltaX = itemPosition.x - location.x;
        float deltaY = itemPosition.y - location.y;

        float angle = Mathf.Atan2(deltaY, deltaX) * Mathf.Rad2Deg + 90;

        float adjAngle = Mathf.MoveTowardsAngle(currentAngle, angle, MAXIMUM_ROTATION_PER_SEC * Time.deltaTime);
        _mantaRay.eulerAngles = new Vector3(0, 0, adjAngle);
    }
}
