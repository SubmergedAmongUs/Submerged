using System.Linq;
using Il2CppSystem.IO;
using Reactor.Utilities.Attributes;
using Rewired;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.ResetBreakers.MonoBehaviours;

[RegisterInIl2Cpp]
public sealed class CircuitBreaker(nint ptr) : MonoBehaviour(ptr)
{
    public GameObject on;
    public GameObject off;
    public SpriteRenderer character;

    public AudioClip breakerClick;

    public KeyCode targetKey;

    public bool complete;

    public void Awake()
    {
        complete = UnityRandom.Range(0, 1f) > 0.5f;

        on = transform.Find("On").gameObject;
        off = transform.Find("Off").gameObject;
        character = transform.Find("Character").GetComponent<SpriteRenderer>();

        on.SetActive(complete);
        off.SetActive(!complete);

#if ANDROID
        ButtonBehavior click = gameObject.AddComponent<ButtonBehavior>();
        click.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        click.OnClick.AddListener(new System.Action(() =>
        {
            ToggleBreaker();
        }));
#endif
    }

    public void Update()
    {
        if (Input.GetKeyDown(targetKey))
        {
            ToggleBreaker();
        }
    }

    public void ToggleBreaker()
    {
        SoundManager.Instance.PlaySound(breakerClick, false);
        SetState(!complete);
    }

    public void SetState(bool state)
    {
        complete = state;
        on.SetActive(complete);
        off.SetActive(!complete);
    }
}
