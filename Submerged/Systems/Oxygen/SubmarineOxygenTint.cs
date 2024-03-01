using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Submerged.Systems.Oxygen;

[RegisterInIl2Cpp]
public sealed class SubmarineOxygenTint(nint ptr) : MonoBehaviour(ptr)
{
    private static readonly AnimationCurve _lerpCurve = new(new Keyframe(25, 0), new Keyframe(15, 0.33f), new Keyframe(0, 1.05f));

    private MeshRenderer _meshRenderer;
    private bool _lastActive;

    private void Awake()
    {
        MushroomMixupScreenTint tint = GetComponent<MushroomMixupScreenTint>();
        _meshRenderer = tint.meshRenderer;

        _meshRenderer.material = new Material(_meshRenderer.material);
        _meshRenderer.material.SetColor("_ColorInner", new Color(0f, 0f, 0f, 0.9f));
        _meshRenderer.material.SetColor("_ColorOuter", new Color(0f, 0f, 0f, 1));
        _meshRenderer.material.SetFloat(MushroomMixupScreenTint.Opacity, 0);
    }

    private void Start()
    {
        if (transform.parent != DestroyableSingleton<HudManager>.Instance.transform)
        {
            transform.SetParent(DestroyableSingleton<HudManager>.Instance.transform, false);
            transform.localPosition = new Vector3(0f, 0f, 10f);
        }

        _meshRenderer.enabled = true;
    }

    private void Update()
    {
        if (SubmarineOxygenSystem.Instance.IsActive && SubmarineOxygenSystem.Instance.LocalPlayerNeedsMask)
        {
            _meshRenderer.material.SetFloat(MushroomMixupScreenTint.Opacity, _lerpCurve.Evaluate(SubmarineOxygenSystem.Instance.countdown));
            _lastActive = true;
        }
        else if (_lastActive)
        {
            _lastActive = false;
            this.StartCoroutine(CoFadeOut(_meshRenderer.material.GetFloat(MushroomMixupScreenTint.Opacity)));
        }
    }

    [HideFromIl2Cpp]
    private IEnumerator CoFadeOut(float currentValue)
    {
        yield return new WaitForSeconds(0.5f);
        if (PlayerControl.LocalPlayer.Data.IsDead) yield return new WaitForSeconds(2.25f);

        while (currentValue > 0)
        {
            _meshRenderer.material.SetFloat(MushroomMixupScreenTint.Opacity, currentValue);
            currentValue -= Time.deltaTime;
            yield return null;
        }
        _meshRenderer.material.SetFloat(MushroomMixupScreenTint.Opacity, 0);
    }
}
