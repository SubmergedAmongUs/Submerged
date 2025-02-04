using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using PowerTools;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Submerged.ExileCutscene;

[RegisterInIl2Cpp]
public sealed class FishController(nint ptr) : MonoBehaviour(ptr)
{
    public AnimationClip swim;
    public AnimationClip bite;

    public SpriteAnim anim;

    public Transform targetObj;
    public ParticleSystem bubbles;

    public bool started;
    private bool _bit;

    private float _speed = 10f;

    private void Start()
    {
        anim.Speed = 1;
        anim.Paused = false;
        anim.Play(swim);
    }

    private void Update()
    {
        if (!started) return;

        Vector3 localPosition = transform.localPosition;
        Vector3 localObjPosition = targetObj.transform.localPosition;

        float distance = localObjPosition.y - localPosition.y;

        localPosition.y -= _speed * Time.deltaTime;
        transform.localPosition = localPosition;

        anim.Speed = 1;
        anim.Paused = false;

        if (distance > -7.5f && !_bit)
        {
            _speed = 5f;
            PlayBite();

            anim.Play(bite);
        }
    }

    public void PlaySwim()
    {
        anim.Play(swim);
    }

    public void PlayBite()
    {
        _bit = true;
        anim.Play(bite);
    }

    public void Closed()
    {
        if (targetObj != null)
        {
            targetObj.gameObject.SetActive(false);
        }
        if (bubbles != null)
        {
            bubbles.Stop();
        }
        this.StartCoroutine(LerpSpeed(0.5f, _speed, 25f));
    }

    [HideFromIl2Cpp]
    public IEnumerator LerpSpeed(float duration, float start, float end)
    {
        for (float i = 0; i < duration; i += Time.deltaTime)
        {
            _speed = Mathf.Lerp(start, end, i / duration);

            yield return null;
        }

        _speed = end;
    }
}
