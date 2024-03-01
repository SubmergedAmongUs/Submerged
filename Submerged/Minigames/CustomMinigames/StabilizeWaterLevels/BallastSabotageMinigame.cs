using Reactor.Utilities.Attributes;
using Submerged.BaseGame.Extensions;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.StabilizeWaterLevels;

[RegisterInIl2Cpp]
public sealed class BallastSabotageMinigame(nint ptr) : Minigame(ptr)
{
    private const float MIN_VALUE = -2.65f;
    private const float MAX_VALUE = 0f;
    private const float STEP = 0.01f;

    public SpriteRenderer gauge;
    public bool isDefaultIncreasing = true;
    private float _elapsed;

    private float _gaugeValue;

    private bool _inRange;
    private ReactorSystemType _reactor;

    private void Start()
    {
        _reactor = ShipStatus.Instance.Systems[MyTask.StartAt].Cast<ReactorSystemType>();
        gauge = transform.Find("Background/Bar").GetComponent<SpriteRenderer>();
        _elapsed = 0f;
        SetToValue(UnityRandom.Range(0, 1f));
    }

    private void Update()
    {
        if (!_reactor.IsActive && amClosing == CloseState.None)
        {
            StartCoroutine(CoStartClose());

            return;
        }

        if (!_reactor.IsActive || amClosing != CloseState.None)
        {
            return;
        }

        _elapsed += Time.deltaTime;

        if (_elapsed > 0.1f)
        {
            _elapsed -= 0.1f;

            _gaugeValue = isDefaultIncreasing ? Mathf.Clamp(_gaugeValue + STEP, 0f, 1f) : Mathf.Clamp(_gaugeValue - STEP, 0f, 1f);
            UpdateGaugeDisplay();
        }

        bool newInRange = _gaugeValue is >= 0.45f and <= 0.55f;

        if (newInRange != _inRange)
        {
            _inRange = newInRange;

            if (newInRange)
            {
                ShipStatus.Instance.RpcUpdateSystem(MyTask.StartAt, (byte) (64 | ConsoleId));
            }
            else
            {
                ShipStatus.Instance.RpcUpdateSystem(MyTask.StartAt, (byte) (32 | ConsoleId));
            }
        }
    }

    public void SetToValue(float value)
    {
        _gaugeValue = Mathf.Clamp(value / 100f, 0f, 1f);
        UpdateGaugeDisplay();
    }

    public void IncreaseLevel()
    {
        if (!_reactor.IsActive) return;

        _gaugeValue = Mathf.Clamp(_gaugeValue + STEP * 5, 0f, 1f);
        UpdateGaugeDisplay();
    }

    public void DecreaseLevel()
    {
        if (!_reactor.IsActive) return;

        _gaugeValue = Mathf.Clamp(_gaugeValue - STEP * 5, 0f, 1f);
        UpdateGaugeDisplay();
    }

    private void UpdateGaugeDisplay()
    {
        Vector3 position = gauge.transform.localPosition;
        position.y = Mathf.Lerp(MIN_VALUE, MAX_VALUE, _gaugeValue);
        gauge.transform.localPosition = position;
    }

    public override void Close()
    {
        if (ShipStatus.Instance)
        {
            ShipStatus.Instance.RpcUpdateSystem(MyTask.StartAt, (byte) (32 | ConsoleId));
        }

        this.BaseClose();
    }
}
