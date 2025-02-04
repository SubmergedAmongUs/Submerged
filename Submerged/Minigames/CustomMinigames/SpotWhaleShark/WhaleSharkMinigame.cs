using Reactor.Utilities.Attributes;
using Submerged.Minigames.MonoBehaviours;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.SpotWhaleShark;

[RegisterInIl2Cpp]
public sealed class WhaleSharkMinigame(nint ptr) : Minigame(ptr)
{
    private ClickableSprite _button;

    private bool _finished;

    private Vector3 _originalPosition;
    private Transform _target;
    private Vector3 _targetPosition;

    private WhaleSharkTask _task;
    private Transform _whaleShark;

    private void Start()
    {
        _task = MyTask.Cast<WhaleSharkTask>();

        _whaleShark = transform.Find("WhaleShark");
        _target = transform.Find("Target");

        _originalPosition = _whaleShark.localPosition;
        _targetPosition = _target.localPosition;

        _button = _whaleShark.gameObject.AddComponent<ClickableSprite>();
        _button.onDown += () =>
        {
            if (!_task.visible || _finished) return;
            if (MyNormTask != null)
            {
                MyNormTask.NextStep();
            }
            StartCoroutine(CoStartClose(1f));
            _finished = true;
        };
    }

    private void Update()
    {
        _whaleShark.localPosition = !_task.visible ? _originalPosition : Vector3.Lerp(_originalPosition, _targetPosition, _task.timer / _task.visibleDuration);
    }
}
