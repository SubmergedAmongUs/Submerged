using System.Collections.Generic;
using System.Linq;
using Reactor.Utilities.Attributes;
using Submerged.Minigames.MonoBehaviours;
using Submerged.Localization.Strings;
using TMPro;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.SteadyHeartbeat;

[RegisterInIl2Cpp]
public sealed class SteadyHeartbeatMinigame(nint ptr) : Minigame(ptr)
{
    private const float IN_RANGE_TARGET = 5f;
    private readonly Vector3[] _default = [new(-4.65f, 0, 0), new(4.65f, 0, 0)];
    private Vector3[] _beat;

    private LineRenderer _beatLine;

    private List<float> _beats = [];
    private float _beatTimer;

    private int _bpmTarget;
    private TextMeshPro _bpmText;

    private TextMeshPro _bpmTextLeft;
    private float _inRangeTimer;

    private MinigameProperties _minigameProperties;

    private List<Vector3> _points;
    private FloatRange _range;
    private TextMeshPro _statusText;
    private TextMeshPro _targetText;

    private float _timer;

    // private List<float> TimeDeltas;

    private void Start()
    {
        _minigameProperties = GetComponent<MinigameProperties>();

        _bpmTextLeft = transform.Find("Text/HeartRate").GetComponent<TextMeshPro>();
        _bpmText = transform.Find("Text/BPM").GetComponent<TextMeshPro>();
        _targetText = transform.Find("Text/TargetText").GetComponent<TextMeshPro>();
        _statusText = transform.Find("Text/StatusText").GetComponent<TextMeshPro>();

        transform.Find("Button").gameObject.AddComponent<ClickableSprite>().onDown += () => { _beats.Add(_timer); };

        _beatLine = transform.Find("MainBeat").GetComponent<LineRenderer>();
        _beat = _minigameProperties.vector2S.Select(v => (Vector3) v).ToArray();

        _bpmTarget = UnityRandom.Range(5, 15) * 10;
        _targetText.text = string.Format(Tasks.SteadyHeartbeat_Target, _bpmTarget, _bpmTarget + 40);

        // TimeDeltas = Enumerable.Repeat(60f / (BPMTarget + Random.Range(40, 67)), 5).ToList();
        _timer = 0;

        _range = new FloatRange(-4.65f, 4.65f);
        _points = [];
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_inRangeTimer >= IN_RANGE_TARGET && amClosing == CloseState.None)
        {
            Color32 col = new(30, 150, 0, 255);
            _statusText.text = string.Format(Tasks.SteadyHeartbeat_Status, $"<color=#{col.r:X2}{col.g:X2}{col.b:X2}{col.a:X2}>{Tasks.SteadyHeartbeat_Status_Stable}</color>");
            if (MyNormTask != null)
            {
                MyNormTask.NextStep();
            }
            StartCoroutine(CoStartClose());
        }

        _beats = _beats.Where(t => t >= _timer - 5).ToList();

        int avgBpm = Mathf.RoundToInt(_beats.Count * 12f / 5f) * 5;
        BeatHeart(avgBpm);

        if (amClosing != CloseState.None) return;

        _bpmTextLeft.text = string.Format(Tasks.SteadyHeartbeat_BPMLeft, avgBpm);
        _bpmText.text = string.Format(Tasks.SteadyHeartbeat_BPMRight, avgBpm);

        if (_bpmTarget <= avgBpm && avgBpm <= _bpmTarget + 40)
        {
            _inRangeTimer += Time.deltaTime;

            Color32 col = _inRangeTimer / IN_RANGE_TARGET < 0.5
                ? Color32.Lerp(new Color32(255, 0, 0, 255), new Color32(255, 242, 0, 255), _inRangeTimer / IN_RANGE_TARGET * 2)
                : Color32.Lerp(new Color32(255, 242, 0, 255), new Color32(30, 150, 0, 255), _inRangeTimer / IN_RANGE_TARGET - 0.5f * 2);

            _statusText.text = string.Format(Tasks.SteadyHeartbeat_Status, $"<color=#{col.r:X2}{col.g:X2}{col.b:X2}{col.a:X2}>{Tasks.SteadyHeartbeat_Status_Stabilizing}</color>");
        }
        else
        {
            _statusText.text = string.Format(Tasks.SteadyHeartbeat_Status,
                                             $"<color=red>{Tasks.SteadyHeartbeat_Status_Irregular}</color>");
        }
    }

    private void BeatHeart(int bpm)
    {
        float delta = _range.Width / 3.5f * Time.deltaTime;

        for (int index = 0; index < _points.Count; index++)
        {
            Vector3 point = _points[index];
            point.x -= delta;
            _points[index] = point;
        }

        _points = _points.Where(t => t.x > -4.65f).ToList();

        Vector3[] endFiltered = _points.Where(v => v.x <= 4.65f).ToArray();

        if (endFiltered.Length == 0)
        {
            endFiltered = _default;
        }
        else
        {
            endFiltered[0] = new Vector3(-4.65f, 0, 0);
            endFiltered[^1] = new Vector3(4.65f, 0, 0);
        }

        _beatTimer += Time.deltaTime;

        if (_beatTimer >= 60f / Mathf.Clamp(bpm, 0, 180))
        {
            SoundManager.Instance.StopSound(_minigameProperties.audioClips[0]);
            SoundManager.Instance.PlaySound(_minigameProperties.audioClips[0], false);
            _beatTimer = 0;

            _points.AddRange(_beat);
        }

        _beatLine.positionCount = endFiltered.Length;
        _beatLine.SetPositions(endFiltered);
    }
}
