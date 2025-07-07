using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Submerged.Floors.Objects;

[RegisterInIl2Cpp]
public class LongPlayerShadowRenderer(nint ptr) : PlayerShadowRenderer(ptr)
{
    public LongBoiPlayerBody body;

    protected override void Start()
    {
        base.Start();
        body = target.GetComponentsInParent<LongBoiPlayerBody>(true)[0];
        body.gameObject.layer = LayerMask.NameToLayer("Players");
    }

    private SpriteRenderer _lastRenderer;

    protected override void LateUpdate()
    {
        base.LateUpdate();

        if (!targetRenderer || _lastRenderer == targetRenderer) return;

        _lastRenderer = targetRenderer;

        switch (targetRenderer.name)
        {
            case "LongNeck":
                shadowRenderer.size = new Vector2(targetRenderer.size.x, 1.1f);
                break;

            case "ForegroundNeck":
                shadowRenderer.size = new Vector2(targetRenderer.size.x, 1.7f);
                break;

            case "LongHead":
                shadowRenderer.transform.localPosition = new Vector3(shadowRenderer.transform.localPosition.x, body.neckSprite.transform.localPosition.y + 2.79f, shadowRenderer.transform.localPosition.z);
                break;
        }
    }
}
