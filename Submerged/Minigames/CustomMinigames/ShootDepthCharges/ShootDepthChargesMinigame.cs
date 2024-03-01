using System.Linq;
using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using Submerged.Minigames.CustomMinigames.ShootDepthCharges.MonoBehaviours;
using Submerged.Minigames.MonoBehaviours;
using Submerged.Localization.Strings;
using TMPro;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.ShootDepthCharges;

[RegisterInIl2Cpp]
public sealed class ShootDepthChargesMinigame(nint ptr) : Minigame(ptr)
{
    public float yStart;

    public AudioClip[] hitClips;

    private Transform _background;
    private BoxCollider2D _doNotCloseBoxCollider2D;
    private LineRenderer _lineRenderer;
    private Transform _lineRendererTransform;
    private Camera _mainCam;

    private MinigameProperties _minigameProperties;
    private float _nextWait;

    private int _remainingCharges = 20;
    private AudioClip _shootClip;
    private Transform _staticScreen;

    private BoxCollider2D _staticScreenBoxCollider2D;
    private Transform _target;
    private TextMeshPro _text;

    private float _timer;
    private FloatRange _xRange;

    public bool IsFinished => MyNormTask.taskStep >= MyNormTask.MaxStep;

    private void Start()
    {
        _remainingCharges = MyNormTask.MaxStep - MyNormTask.taskStep;
        _minigameProperties = GetComponent<MinigameProperties>();
        _shootClip = _minigameProperties.audioClips[3];
        hitClips = _minigameProperties.audioClips.Take(3).ToArray();

        _mainCam = Camera.main;

        _background = transform.Find("Background");
        _staticScreen = _background.Find("StaticScreen");
        _target = transform.Find("Target");
        _staticScreenBoxCollider2D = _staticScreen.GetComponent<BoxCollider2D>();
        _doNotCloseBoxCollider2D = _background.GetComponent<BoxCollider2D>();

        _xRange = new FloatRange(-9f, 9f);
        yStart = 2 * 10f;

        _lineRendererTransform = transform.Find("LineRenderer");
        _lineRenderer = _lineRendererTransform.GetComponent<LineRenderer>();

        _text = transform.Find("Text").GetComponent<TextMeshPro>();
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        _text.text = string.Format(Tasks.ShootDepthCharges_Remaining, _remainingCharges);

        if (IsFinished) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = _mainCam.ScreenToWorldPoint(Input.mousePosition);

            if (!_staticScreenBoxCollider2D.OverlapPoint(mousePos))
            {
                if (!_doNotCloseBoxCollider2D.OverlapPoint(mousePos)) StartCoroutine(CoStartClose(0));

                return;
            }

            _target.position = new Vector3(mousePos.x, mousePos.y, _target.position.z);
            Vector3 clickPosition = new(mousePos.x, mousePos.y, _text.transform.position.z + 20f);
            _lineRenderer.SetPosition(1, transform.InverseTransformPoint(clickPosition));
            SoundManager.Instance.PlaySound(_shootClip, false);
        }

        if (_timer >= _nextWait)
        {
            _timer = 0;
            _nextWait = UnityRandom.Range(0.30f, 0.75f);

            GameObject newBomb = Instantiate(_minigameProperties.gameObjects.Random(), transform);
            newBomb.GetComponent<Rigidbody2D>().gravityScale *= 1.5f;
            DepthCharge depthCharge = newBomb.AddComponent<DepthCharge>();
            depthCharge.minigame = this;
            Vector3 pos = newBomb.transform.position;
            pos.x = _xRange.Next();
            pos.y = yStart;
            pos.z = -2f;
            newBomb.transform.localPosition = pos;
        }
    }

    public void Shoot()
    {
        if (amClosing != CloseState.None || IsFinished) return;
        _remainingCharges--;

        if (MyNormTask)
        {
            MyNormTask.ShowTaskStep = false;
            MyNormTask.NextStep();
            MyNormTask.ShowTaskStep = true;

            if (MyNormTask.taskStep == MyNormTask.MaxStep)
            {
                StartCoroutine(CoStartClose());
            }
        }
        else
        {
            if (_remainingCharges == 0)
            {
                StartCoroutine(CoStartClose());
            }
        }
    }
}
