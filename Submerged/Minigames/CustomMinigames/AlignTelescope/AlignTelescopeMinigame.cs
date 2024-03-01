using System.Linq;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using Submerged.Minigames.MonoBehaviours;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.AlignTelescope;

[RegisterInIl2Cpp]
public sealed class AlignTelescopeMinigame(nint ptr) : Minigame(ptr)
{
    private readonly FloatRange _blipDelay = new(0.01f, 1f);

    private readonly Controller _controller = new();

    private Transform _background;
    private Coroutine _blinky;
    private AudioClip _blipSound;

    private bool _grabbed;
    private SpriteRenderer _itemDisplay;
    private Collider2D[] _items;

    private MinigameProperties _minigameProperties;
    private Collider2D _reticle;
    private SpriteRenderer _reticleImage;
    private Collider2D _targetItem;

    public void Start()
    {
        _minigameProperties = GetComponent<MinigameProperties>();
        _items = _minigameProperties.colliders.Take(7).ToArray();
        _blipSound = _minigameProperties.audioClips[0];
        _background = _minigameProperties.transforms[0];
        _reticle = _minigameProperties.colliders[7];
        _reticleImage = _reticle.GetComponent<SpriteRenderer>();
        _itemDisplay = transform.Find("TargetDisplay/TargetItem").GetComponent<SpriteRenderer>();
        _targetItem = _items.Random();
        _itemDisplay.sprite = _targetItem.GetComponent<SpriteRenderer>().sprite;

        this.StartCoroutine(RunBlipSound());
    }

    public void Update()
    {
        _controller.Update();
        DragState dragState = _controller.CheckDrag(_background.GetComponent<BoxCollider2D>());

        if (dragState is DragState.Dragging or DragState.Holding)
        {
            Vector2 vector = _controller.DragPosition - _controller.DragStartPosition;
            Vector3 localPosition = _background.transform.localPosition;
            localPosition.x = Mathf.Clamp(localPosition.x + vector.x, -10.88f, 10.88f);
            localPosition.y = Mathf.Clamp(localPosition.y + vector.y, -10.85f, 10.85f);
            _background.transform.localPosition = localPosition;
            _controller.ResetDragPosition();
        }

        if (Vector2.Distance(_targetItem.transform.position, _reticle.transform.position) <= 1f)
        {
            _blinky ??= this.StartCoroutine(CoBlinky());
        }
        else if (_blinky != null)
        {
            StopCoroutine(_blinky);
            _blinky = null;
            _reticleImage.color = Color.white;
        }
    }

    [HideFromIl2Cpp]
    private IEnumerator RunBlipSound()
    {
        while (this)
        {
            for (float time = 0f; time < _blipDelay.Lerp(Vector2.Distance(_targetItem.transform.position, _reticle.transform.position) / 10f); time += Time.deltaTime)
            {
                yield return null;
            }

            if (Constants.ShouldPlaySfx())
            {
                SoundManager.Instance.PlaySoundImmediate(_blipSound, false);
            }
        }
    }

    [HideFromIl2Cpp]
    private IEnumerator CoBlinky()
    {
        int num;

        for (int i = 0; i < 3; i = num)
        {
            _reticleImage.color = Color.green;

            yield return new WaitForSeconds(0.1f);
            _reticleImage.color = Color.white;

            yield return new WaitForSeconds(0.2f);
            num = i + 1;
        }

        _blinky = null;
        _reticleImage.color = Color.green;

        if (amClosing == CloseState.None)
        {
            MyNormTask!?.NextStep();
            StartCoroutine(CoStartClose());
        }
    }

    public void Grab()
    {
        _grabbed = !_grabbed;
    }
}
