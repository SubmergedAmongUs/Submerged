using System;
using System.Collections.Generic;
using System.Linq;
using Reactor.Utilities.Attributes;
using Submerged.Minigames.MonoBehaviours;
using Submerged.Localization.Strings;
using TMPro;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.CycleReactor;

[RegisterInIl2Cpp]
public sealed class CycleReactorMinigame(nint ptr) : Minigame(ptr)
{
    private Transform _barMask;

    private List<float> _deltas;

    private bool _dragging;

    private Transform _handle;
    private Transform _handleArm;
    private PolygonCollider2D _handleCollider;
    private float _lastTotalAngle;

    private Camera _mainCam;

    private MinigameProperties _minigameProperties;
    private Transform _needle;
    private Vector2 _offset;
    private Vector3 _originalMaskPos;

    private float _prevAngle;
    private float _rpm;
    private TextMeshPro _text;
    private float _timer;
    private float _totalAngle;

    private void Start()
    {
        _minigameProperties = GetComponent<MinigameProperties>();

        _needle = transform.Find("Needle");
        _handle = transform.Find("Handle");
        _handleArm = _handle.Find("HandleArm");
        _barMask = transform.Find("Bar/Mask");
        _originalMaskPos = _barMask.localPosition;
        _handleCollider = _handleArm.GetComponent<PolygonCollider2D>();

        _mainCam = Camera.main;
        _deltas = Enumerable.Repeat(0f, 10).ToList();
        _text = GetComponentInChildren<TextMeshPro>();
    }

    private void Update()
    {
        if (amClosing != CloseState.None) return;

        if (_totalAngle >= 9000)
        {
            MyNormTask!?.NextStep();
            StartCoroutine(CoStartClose());

            return;
        }

        _barMask.localPosition = new Vector3(_originalMaskPos.x, Mathf.Lerp(_originalMaskPos.y, _originalMaskPos.y + 18.5f, Mathf.InverseLerp(0, 9000, _totalAngle)), _originalMaskPos.z);
        Vector2 mousePosition = _mainCam.ScreenToWorldPoint(Input.mousePosition);
        bool overlap = _handleCollider.OverlapPoint(mousePosition);
        _minigameProperties.dontCloseOnBgClick = overlap;

        if (Input.GetMouseButtonDown(0) && overlap)
        {
            _offset = mousePosition - (Vector2) _handleArm.position;
            _dragging = true;
        }

        if (Input.GetMouseButtonUp(0)) _dragging = false;

        if (!_dragging) return;
        Vector3 handlePos = _handle.position;
        float deltaX = mousePosition.x - handlePos.x - _offset.x;
        float deltaY = mousePosition.y - handlePos.y - _offset.y;
        float angle = (Mathf.Atan2(deltaY, deltaX) * Mathf.Rad2Deg + 720) % 360;
        float lastAngle = _handle.localEulerAngles.z;
        _totalAngle += Mathf.Abs(Mathf.DeltaAngle(angle, lastAngle));
        _handle.localEulerAngles = new Vector3(0, 0, angle);
        _handleArm.eulerAngles = Vector3.zero;
    }

    private void FixedUpdate()
    {
        if (amClosing != CloseState.None) return;

        _timer += Time.deltaTime;
        _needle.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(_prevAngle, -_rpm * 0.5f + 120, _timer / 0.1f));

        if (_timer < 0.1f) return;
        _timer = 0;

        _deltas.Add(_totalAngle - _lastTotalAngle);
        _lastTotalAngle = _totalAngle;
        _deltas.RemoveAt(0);
        _rpm = _deltas.Average() * 600f / 360f;
        _rpm = Mathf.Clamp(_rpm, 0, 10000);
        _text.text = string.Format(Tasks.CycleReactor_RPM, $"{_rpm:00}");
        _prevAngle = _needle.localEulerAngles.z;

        SoundManager.Instance.PlayDynamicSound("CycleSound",
                                               _minigameProperties.audioClips[0],
                                               true,
                                               new Action<AudioSource, float>(CycleSound),
                                               SoundManager.Instance.SfxChannel);
    }

    private void CycleSound(AudioSource source, float t)
    {
        if (!this) source.Stop();

        source.volume = Mathf.Lerp(0, 1f, _totalAngle / 9000f) * 0.5f;
        source.pitch = Mathf.Lerp(0, 5f, _totalAngle / 9000f);
    }
}
