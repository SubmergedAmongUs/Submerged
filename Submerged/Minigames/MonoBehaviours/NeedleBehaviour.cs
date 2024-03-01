using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Submerged.Minigames.MonoBehaviours;

[RegisterInIl2Cpp]
public sealed class NeedleBehaviour(nint ptr) : MonoBehaviour(ptr)
{
    public enum Movement
    {
        ConstantBounce,
        RandomBounce
    }

    public float duration;
    public float initialAngle;
    public float amount;
    public Movement movementType;

    public bool randomInitialAngle;
    public FloatRange initialAngleRange;

    public void Start()
    {
        if (randomInitialAngle)
        {
            initialAngle = UnityRandom.Range(initialAngleRange.min, initialAngleRange.max);
        }

        switch (movementType)
        {
            case Movement.ConstantBounce:
                this.StartCoroutine(ConsistantBounce());

                break;
            case Movement.RandomBounce:
                this.StartCoroutine(RandomBounce());

                break;
        }
    }

    [HideFromIl2Cpp]
    public IEnumerator ConsistantBounce()
    {
        Quaternion initial = Quaternion.Euler(0, 0, -amount + initialAngle);
        Quaternion final = Quaternion.Euler(0, 0, amount + initialAngle);

        start:

        for (float time = 0; time < duration; time += Time.deltaTime)
        {
            transform.rotation = Quaternion.Slerp(initial, final, time / (duration / 2));

            yield return null;
        }

        for (float time = 0; time < duration; time += Time.deltaTime)
        {
            transform.rotation = Quaternion.Lerp(final, initial, time / (duration / 2));

            yield return null;
        }

        yield return null;

        goto start;
    }

    [HideFromIl2Cpp]
    public IEnumerator RandomBounce()
    {
        start:
        float random = UnityRandom.Range(0, amount);

        Quaternion initial = Quaternion.Euler(0, 0, -random + initialAngle);
        Quaternion final = Quaternion.Euler(0, 0, random + initialAngle);

        for (float time = 0; time < duration; time += Time.deltaTime)
        {
            transform.rotation = Quaternion.Slerp(initial, final, time / (duration / 2));

            yield return null;
        }

        for (float time = 0; time < duration; time += Time.deltaTime)
        {
            transform.rotation = Quaternion.Lerp(final, initial, time / (duration / 2));

            yield return null;
        }

        yield return null;

        goto start;
    }

    public void StartSpin(float _)
    {
        StopAllCoroutines();
        this.StartCoroutine(Spin());
    }

    [HideFromIl2Cpp]
    public IEnumerator Spin()
    {
        while (true)
        {
            transform.Rotate(0, 0, 360 * (Time.deltaTime / duration));
            yield return null;
        }
    }
}
