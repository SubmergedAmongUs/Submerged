using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.ShootDepthCharges.MonoBehaviours;

[RegisterInIl2Cpp]
public sealed class DepthCharge(nint ptr) : MonoBehaviour(ptr)
{
    public ShootDepthChargesMinigame minigame;

    private Collider2D _collider2D;
    private Transform _explode;
    private Transform _flash;

    private bool _hasBeenShot;
    private Camera _mainCam;

    private bool _visible;

    private void Start()
    {
        _mainCam = Camera.main;

        _collider2D = GetComponent<Collider2D>();
        _explode = transform.Find("Explode");
        _flash = transform.Find("Flash");
    }

    private void Update()
    {
        if (minigame.IsFinished || _hasBeenShot) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = _mainCam.ScreenToWorldPoint(Input.mousePosition);
            if (_collider2D.OverlapPoint(mousePos)) this.StartCoroutine(Shoot());
        }
    }

    private void OnBecameInvisible()
    {
        if (_visible) Destroy(gameObject);
    }

    private void OnBecameVisible() => _visible = true;

    [HideFromIl2Cpp]
    private IEnumerator Shoot()
    {
        SoundManager.Instance.PlaySound(minigame.hitClips.Random(), false);
        minigame.Shoot();

        _hasBeenShot = true;

        _visible = false;

        Rigidbody2D body = GetComponent<Rigidbody2D>();
        body.simulated = false;

        const float DURATION = 0.15f;
        Vector3 vectorZero = new(0, 0, 1);
        Vector3 vectorFull = new(0.75f, 0.75f, 1);
        _flash.localScale = vectorZero;
        _flash.gameObject.SetActive(true);

        for (float t = 0; t < DURATION / 2; t += Time.deltaTime)
        {
            _flash.localScale = Vector3.Lerp(vectorZero, vectorFull, t / (DURATION / 2));

            yield return null;
        }

        for (float t = 0; t < DURATION / 2; t += Time.deltaTime)
        {
            _flash.localScale = Vector3.Lerp(vectorFull, vectorZero, t / (DURATION / 2));

            yield return null;
        }

        GetComponent<SpriteRenderer>().enabled = false;
        _flash.gameObject.SetActive(false);
        _explode.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.25f);
        Destroy(gameObject);
    }
}
