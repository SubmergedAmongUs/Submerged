using System;
using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using Submerged.Map;
using UnityEngine;

namespace Submerged.Floors.Objects;

[RegisterInIl2Cpp]
public class RelativeShadowRenderer(nint ptr) : MonoBehaviour(ptr)
{
    #region Template stuff

    static RelativeShadowRenderer()
    {
        ComponentClone.RegisterTemplate(new SpriteRendererCloneTemplate());
    }

    public const string SHADOW_CLONE_PURPOSE = "SubmergedShadows";

    public sealed class SpriteRendererCloneTemplate : ComponentClone.ITemplate<SpriteRenderer>
    {
        public string Purpose => SHADOW_CLONE_PURPOSE;

        public SpriteRenderer AddComponentTo(GameObject target) => target.AddComponent<SpriteRenderer>();

        public void CopyComponent(SpriteRenderer source, SpriteRenderer target)
        {
            if (!SubmarineStatus.instance)
            {
                throw new InvalidOperationException("SubmarineStatus.instance is null, cannot clone shadow sprite renderer");
            }

            target.enabled = source.enabled && source.gameObject.activeInHierarchy;
            target.sprite = SubmarineStatus.instance.GetReplacementShadowSprite(source.sprite, source.name);
            target.SetColorAlpha(source.color.a);
            target.flipX = source.flipX;
            target.flipY = source.flipY;
            target.size = source.size;
            target.drawMode = source.drawMode;
            target.tileMode = source.tileMode;
            target.adaptiveModeThreshold = source.adaptiveModeThreshold;
        }
    }

    #endregion

    public Transform target;

    protected void Awake()
    {
        gameObject.layer = 4;
    }

    protected virtual void Start()
    {
        foreach (SpriteRenderer rend in target.GetComponents<SpriteRenderer>())
        {
            ComponentClone.CloneIfPossible(rend, gameObject, SHADOW_CLONE_PURPOSE);
        }
    }

    protected virtual void LateUpdate()
    {
        transform.localPosition = target.transform.localPosition;
        transform.localScale = target.transform.localScale;
        transform.localRotation = target.transform.localRotation;
    }
}
