using UnityEngine;

namespace Submerged.Extensions;

public static class AnimatorExtensions
{
    public static IEnumerator PlayAndWaitForAnimation(this Animator animator, string stateName, int layerIndex = 0)
    {
        animator.Play(stateName);

        while (!animator.GetCurrentAnimatorStateInfo(layerIndex).IsName(stateName))
        {
            yield return null;
        }

        float length = animator.GetCurrentAnimatorStateInfo(layerIndex).length;
        float timer = 0f;

        while (timer <= length)
        {
            timer += Time.deltaTime;

            yield return null;
        }
    }
}
