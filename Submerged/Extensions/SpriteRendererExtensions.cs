using UnityEngine;

namespace Submerged.Extensions;

public static class SpriteRendererExtensions
{
    public static void SetColorAlpha(this SpriteRenderer rend, float alpha)
    {
        Color color = rend.color;
        color.a = alpha;
        rend.color = color;
    }
}
