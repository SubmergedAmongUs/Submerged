using System.Collections.Generic;
using System.Linq;
using Reactor.Utilities.Attributes;
using Submerged.Map;
using UnityEngine;

namespace Submerged.Floors.Objects;

[RegisterInIl2Cpp]
public sealed class PlayerShadowBehaviour(nint ptr) : MonoBehaviour(ptr)
{
    public GameObject shadowObj;
    public SpriteRenderer shadowRend;
    public PlayerControl playerControl;
    public Color shadowColor;

    public SpriteRenderer headRend;
    public SpriteRenderer neckRend;

    // These arrays contains all the default player body sprites, but with their bottom shadows removed. The dictionaries are just a cached version of these arrays.
    public Sprite[] shadowlessPlayerSprites;
    public Sprite[] shadowlessAprilFoolsPlayerSprites;
    private readonly Dictionary<Sprite, Sprite> _spritesDict = [];
    private readonly Dictionary<Sprite, Sprite> _aprilFoolsSpritesDict = [];

    public void Start()
    {
        Camera.main!.cullingMask = 1073969927; // yay magic numbers

        shadowObj = new GameObject("Submerged Shadow") { layer = 4 };
        shadowRend = shadowObj.AddComponent<SpriteRenderer>();
        shadowObj.transform.SetParent(transform);
        shadowObj.AddComponent<RelativeShadowRenderer>();

        GameObject neckObj = new("Neck Shadow") { layer = 4 };
        neckObj.transform.SetParent(shadowObj.transform);
        neckRend = neckObj.AddComponent<SpriteRenderer>();
        neckRend.transform.localPosition = Vector3.zero;
        neckRend.transform.localScale = new Vector3(1f, 25f, 1f);

        GameObject headObj = new("Head Shadow") { layer = 4 };
        headObj.transform.SetParent(shadowObj.transform);
        headRend = headObj.AddComponent<SpriteRenderer>();
        headRend.transform.localPosition = new Vector3(0f, 1.9f, 0f);

        shadowColor = shadowRend.color;

        shadowlessPlayerSprites = SubmarineStatus.instance.minigameProperties.sprites;
        shadowlessAprilFoolsPlayerSprites = SubmarineStatus.instance.aprilFoolsShadowSpritesHolder.sprites;
    }

    public void LateUpdate()
    {
        if (!playerControl || !playerControl.cosmetics.currentBodySprite.BodySprite.enabled || playerControl.Data.IsDead)
        {
            shadowObj.SetActive(false);
            return;
        }

        UpdateSprites();
        UpdateScaleAndColor();
        shadowObj.SetActive(true);
    }

    private void UpdateSprites()
    {
        shadowRend.sprite = GetBodyShadowSprite(playerControl.cosmetics.currentBodySprite.BodySprite.sprite, playerControl.cosmetics.bodyType);

        if (playerControl.MyPhysics.bodyType is PlayerBodyTypes.Long or PlayerBodyTypes.LongSeeker)
        {
            LongBoiPlayerBody longPlayerBody = playerControl.GetComponentInChildren<LongBoiPlayerBody>();

            if (!longPlayerBody)
            {
                Error("Long player body not found! Reverting to regular player shadow.");
                return;
            }

            if (longPlayerBody.isSeekerHorse)
            {
                // TODO
                return;
            }

            neckRend.sprite = longPlayerBody.neckSprite.sprite;
            headRend.sprite = longPlayerBody.headSprite.sprite;
        }
        else
        {
            neckRend.sprite = headRend.sprite = null;
        }
    }

    private void UpdateScaleAndColor()
    {
        float scaleModifier = playerControl.MyPhysics.bodyType == PlayerBodyTypes.Horse ? 0.3367f : 0.5f;
        shadowObj.transform.localScale = Vector3.one * scaleModifier;

        Color color = shadowColor;
        color.a = playerControl.cosmetics.currentBodySprite.BodySprite.color.a;
        shadowRend.color = color;

        bool flipX = playerControl.cosmetics.currentBodySprite.BodySprite.flipX;
        shadowRend.flipX = flipX;
        neckRend.flipX = flipX;
        headRend.flipX = flipX;
    }

    private Sprite GetBodyShadowSprite(Sprite bodySprite, PlayerBodyTypes bodyType)
    {
        if (!bodySprite) return null;

        if (bodyType != PlayerBodyTypes.Normal)
        {
            if (tryGetBodyShadowSpriteInList(bodySprite, shadowlessAprilFoolsPlayerSprites, _aprilFoolsSpritesDict, out Sprite aprilFoolsResult)) return aprilFoolsResult;
        }

        if (tryGetBodyShadowSpriteInList(bodySprite, shadowlessPlayerSprites, _spritesDict, out Sprite result))
        {
            return result;
        }

        return bodySprite;

        static bool tryGetBodyShadowSpriteInList(Sprite spriteToGet, Sprite[] allSprites, Dictionary<Sprite, Sprite> cachedSprites, out Sprite result)
        {
            if (!spriteToGet)
            {
                result = null;
                return false;
            }

            if (cachedSprites.TryGetValue(spriteToGet, out Sprite cachedShadowSprite) && cachedShadowSprite)
            {
                result = cachedShadowSprite;
                return true;
            }

            string spriteName = spriteToGet.name;
            Sprite newShadowSprite = allSprites.FirstOrDefault(s => s.name == spriteName);

            if (newShadowSprite)
            {
                cachedSprites[spriteToGet] = newShadowSprite;
                result = newShadowSprite;
                return true;
            }

            result = null;
            return false;
        }
    }
}
