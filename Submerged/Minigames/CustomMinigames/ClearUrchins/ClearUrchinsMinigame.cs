using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using Submerged.Minigames.CustomMinigames.ClearUrchins.MonoBehaviours;
using Submerged.Minigames.MonoBehaviours;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.ClearUrchins;

[RegisterInIl2Cpp]
public sealed class ClearUrchinsMinigame(nint ptr) : Minigame(ptr)
{
    private const float ANGLE_OFFSET = 137f;
    private const float PELLET_ANGLE_OFFSET = 47.3f;

    private readonly FloatRange _angleRange = new(-20.3f, 16.8f);
    private readonly List<Urchin> _urchins = [];
    private Camera _camera;
    private bool _finished;

    private Transform _gun;

    private MinigameProperties _minigameProperties;
    private Transform _pellet;

    private Rigidbody2D _pelletBody;

    private bool _shooting;

    private void Start()
    {
        _minigameProperties = GetComponent<MinigameProperties>();

        _gun = transform.Find("Rocket");
        _pellet = transform.Find("Pellet");
        _pelletBody = _pellet.GetComponent<Rigidbody2D>();
        _camera = Camera.main;

        List<int> active = Enumerable.Range(0, 11).ToList();
        active.Shuffle();
        active = active.Take(UnityRandom.Range(0, 1f) > 0.5f ? 4 : 5).ToList();

        Transform urchins = transform.Find("Urchins");

        for (int i = 0; i < urchins.childCount; i++)
        {
            Transform t = urchins.GetChild(i);
            Urchin urchin = t.gameObject.AddComponent<Urchin>();
            urchin.minigame = this;
            urchin.urchinHit = _minigameProperties.audioClips[1];
            _urchins.Add(urchin);

            if (!active.Contains(i))
            {
                t.gameObject.SetActive(false);
                urchin.hit = true;
            }
        }
    }

    private void Update()
    {
        Vector3 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 gunPosition = _gun.position;

        float deltaX = mousePosition.x - gunPosition.x;
        float deltaY = mousePosition.y - gunPosition.y;

        float angle = Mathf.Atan2(deltaY, deltaX) * Mathf.Rad2Deg - ANGLE_OFFSET;
        angle = Mathf.Clamp(angle, _angleRange.min, _angleRange.max);
        _gun.eulerAngles = new Vector3(0, 0, angle);

        if (_shooting) return;

        if (Input.GetMouseButtonDown(0)) this.StartCoroutine(Shoot());

        if (!_finished && amClosing == CloseState.None)
        {
            _finished = CheckFinished();

            if (!_finished) return;
            if (MyNormTask != null)
            {
                MyNormTask.NextStep();
            }
            StartCoroutine(CoStartClose());
        }
    }

    private bool CheckFinished()
    {
        foreach (Urchin urchin in _urchins)
        {
            if (!urchin.hit) return false;
        }

        return true;
    }

    [HideFromIl2Cpp]
    private IEnumerator Shoot()
    {
        if (!_shooting)
        {
            _shooting = true;
            SoundManager.Instance.StopSound(_minigameProperties.audioClips[0]);
            SoundManager.Instance.PlaySound(_minigameProperties.audioClips[0], false);
        }

        Vector3 gunPosition = _gun.position;
        _pellet.gameObject.SetActive(true);
        Vector2 rotatedPelletVector = Vector2.up.Rotate(_gun.eulerAngles.z + PELLET_ANGLE_OFFSET) * 1.2419839f;
        Vector3 newPelletPos = gunPosition + (Vector3) rotatedPelletVector;
        newPelletPos.z = transform.localPosition.z - 0.75f;
        _pellet.position = newPelletPos;
        rotatedPelletVector.Normalize();
        _pelletBody.AddForce(rotatedPelletVector * 4, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.1f);

        while (_pellet.gameObject.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
            {
                yield return this.StartCoroutine(StopAndShrinkPellet());
            }
            else
            {
                yield return null;
            }
        }

        _shooting = false;
    }

    [HideFromIl2Cpp]
    public IEnumerator StopAndShrinkPellet()
    {
        const float DURATION = 0.3f;
        Vector3 pos = _pellet.position;

        for (float t = 0; t < DURATION; t += Time.deltaTime)
        {
            _pellet.localScale = Vector3.one * (DURATION - t) / DURATION;
            _pelletBody.velocity = Vector2.zero;
            _pellet.position = pos;

            yield return null;
        }

        _pellet.gameObject.SetActive(false);
        _pellet.localScale = Vector3.one;
        _shooting = false;
    }
}
