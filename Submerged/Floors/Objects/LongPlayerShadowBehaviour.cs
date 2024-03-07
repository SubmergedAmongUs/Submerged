using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Submerged.Floors.Objects;

[RegisterInIl2Cpp]
public class LongPlayerShadowBehaviour(nint ptr) : PlayerShadowBehaviour(ptr)
{
    public LongBoiPlayerBody longPlayerBody;
    public SpriteRenderer neckRend;
    public SpriteRenderer headRend;

    protected override void Start()
    {
        base.Start();

        /* example
        longPlayerBody = playerControl.GetComponentInChildren<LongBoiPlayerBody>();

        neckRend = new GameObject("Neck Shadow")
        {
            layer = 4,
            transform =
            {
                parent = shadowObj.transform,
                localScale = new Vector3(1, 25, 1)
            }
        }.AddComponent<SpriteRenderer>();

        headRend = new GameObject("Head Shadow")
        {
            layer = 4,
            transform =
            {
                parent = shadowObj.transform,
                localPosition = new Vector3(0, 1.9f, 0)
            }
        }.AddComponent<SpriteRenderer>();
        */
    }

    protected override void UpdateSprite()
    {
        base.UpdateSprite();

        /* Whatever else here
        if (!longPlayerBody) return;

        if (longPlayerBody.isSeekerHorse)
        {
            // TODO: Implement seeker horse
        }

        if (!longPlayerBody.neckSprite || !longPlayerBody.headSprite)
        {
            neckRend.sprite = headRend.sprite = null;
            return;
        }

        neckRend.sprite = longPlayerBody.neckSprite.sprite;
        headRend.sprite = longPlayerBody.headSprite.sprite;
        */
    }
}
