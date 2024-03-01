using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.ClearUrchins.MonoBehaviours;

[RegisterInIl2Cpp]
public sealed class Urchin(nint ptr) : MonoBehaviour(ptr)
{
    public ClearUrchinsMinigame minigame;
    public bool hit;
    public AudioClip urchinHit;

    private void OnTriggerEnter2D(Collider2D other)
    {
        other.gameObject.SetActive(false);
        hit = true;
        this.StartCoroutine(Shrink());
    }

    [HideFromIl2Cpp]
    private IEnumerator Shrink()
    {
        SoundManager.Instance.PlaySound(urchinHit, false);
        SpriteRenderer rend = GetComponent<SpriteRenderer>();
        const float DURATION = 0.3f;

        for (float t = 0; t < DURATION; t += Time.deltaTime)
        {
            rend.color = rend.color * (DURATION - t) / DURATION;

            yield return null;
        }

        gameObject.SetActive(false);
    }
}
