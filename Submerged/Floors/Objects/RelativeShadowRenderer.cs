using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using UnityEngine;

namespace Submerged.Floors.Objects;

[RegisterInIl2Cpp]
public class RelativeShadowRenderer(nint ptr) : MonoBehaviour(ptr)
{
    public Transform target;
    public SpriteRenderer targetRenderer;

    public Sprite[] replacementSprites;
    public SpriteRenderer shadowRenderer;

    private readonly Dictionary<Sprite, Sprite> _cachedSprites = [];

    protected virtual void Awake()
    {
        gameObject.layer = 4;
    }

    protected virtual void Start()
    {
        shadowRenderer = gameObject.AddComponent<SpriteRenderer>();
        this.StartCoroutine(UpdateTargetRenderer());
    }

    protected virtual void LateUpdate()
    {
        transform.localPosition = target.transform.localPosition;
        transform.localScale = target.transform.localScale;
        transform.localRotation = target.transform.localRotation;

        if (targetRenderer)
        {
            shadowRenderer.enabled = targetRenderer.enabled && targetRenderer.gameObject.activeInHierarchy;
            shadowRenderer.sprite = GetReplacementSprite(targetRenderer.sprite);
            shadowRenderer.SetColorAlpha(targetRenderer.color.a);
            shadowRenderer.flipX = targetRenderer.flipX;
            shadowRenderer.flipY = targetRenderer.flipY;
            shadowRenderer.size = targetRenderer.size;
            shadowRenderer.drawMode = targetRenderer.drawMode;
            shadowRenderer.tileMode = targetRenderer.tileMode;
            shadowRenderer.adaptiveModeThreshold = targetRenderer.adaptiveModeThreshold;
        }
    }

    protected virtual Sprite GetReplacementSprite(Sprite spriteToGet)
    {
        if (!spriteToGet) return null;

        if (_cachedSprites.TryGetValue(spriteToGet, out Sprite cachedShadowSprite) && cachedShadowSprite)
        {
            return cachedShadowSprite;
        }

        string spriteName = spriteToGet.name;
        Sprite newShadowSprite = replacementSprites.FirstOrDefault(s => s.name == spriteName);

        if (newShadowSprite)
        {
            _cachedSprites[spriteToGet] = newShadowSprite;
            return newShadowSprite;
        }

        return null;
    }

    [HideFromIl2Cpp]
    // ReSharper disable once FunctionRecursiveOnAllPaths
    private IEnumerator UpdateTargetRenderer()
    {
        targetRenderer = target.GetComponent<SpriteRenderer>();
        yield return new WaitForSeconds(1);
        yield return UpdateTargetRenderer();
    }
}
