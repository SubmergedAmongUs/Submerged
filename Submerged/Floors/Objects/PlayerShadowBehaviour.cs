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
    public Sprite[] sprites;
    public Color shadowColor;

    public SpriteRenderer headRend;
    public SpriteRenderer neckRend;

    private readonly Dictionary<Sprite, Sprite> _spritesDict = [];

    public void Start()
    {
        InitializeShadowObject();
        sprites = SubmarineStatus.instance.minigameProperties.sprites;
    }

    public void LateUpdate()
    {
        if (playerControl == null || !playerControl.cosmetics.currentBodySprite.BodySprite.enabled || playerControl.Data.IsDead || neckRend == null || headRend == null)
        {
            shadowObj.SetActive(false);
            return;
        }

        UpdateShadow();
    }

    private void InitializeShadowObject()
    {
        Camera.main!.cullingMask = 1073969927; // yay magic numbers
        shadowObj = new GameObject("Submerged Shadow") { layer = 4 };
        shadowRend = shadowObj.AddComponent<SpriteRenderer>();
        neckRend = new GameObject("Neck Shadow") { layer = 4 }.AddComponent<SpriteRenderer>();
        headRend = new GameObject("Head Shadow") { layer = 4 }.AddComponent<SpriteRenderer>();
        neckRend.transform.SetParent(shadowObj.transform);
        headRend.transform.SetParent(shadowObj.transform);
        headRend.transform.localPosition = new Vector3(0f, 1.9f, 0f);
        neckRend.transform.localScale = new Vector3(1f, 25f, 1f);
        shadowObj.transform.SetParent(transform);
        shadowObj.transform.localPosition = Vector3.zero;
        shadowColor = shadowRend.color;
    }

    private void UpdateShadow()
    {
        UpdateScale();
        UpdateSprites();
        UpdateColorsAndFlip();
        shadowObj.SetActive(true);
    }

    private void UpdateScale()
    {
        shadowObj.transform.localScale = playerControl.MyPhysics.bodyType == PlayerBodyTypes.Horse ? new Vector3(0.3367f, 0.3367f, 1f) : Vector3.one * 0.5f;
    }

    private void UpdateSprites()
    {
        Sprite sprite = playerControl.cosmetics.currentBodySprite.BodySprite.sprite;
        Sprite newSprite;
        if (sprite != null && _spritesDict.TryGetValue(sprite, out Sprite cachedSprite))
        {
            newSprite = cachedSprite;
        }
        else
        {
            newSprite = FindMatchingSprite(sprite);
        }
        shadowRend.sprite = newSprite ? newSprite : sprite;

        if (playerControl.MyPhysics.bodyType is PlayerBodyTypes.Long or PlayerBodyTypes.LongSeeker)
        {
            LongBoiPlayerBody longPlayerBody = playerControl.GetComponentInChildren<LongBoiPlayerBody>();

            if (longPlayerBody is null || longPlayerBody.neckSprite is null || longPlayerBody.headSprite is null || longPlayerBody.isSeekerHorse)
            {
                shadowRend.sprite = neckRend.sprite = headRend.sprite = null;
                return;
            }

            shadowRend.sprite = sprite;
            neckRend.sprite = longPlayerBody.neckSprite.sprite;
            headRend.sprite = longPlayerBody.headSprite.sprite;
        }
        else
        {
            neckRend.sprite = headRend.sprite = null;
        }
    }

    private Sprite FindMatchingSprite(Sprite sprite)
    {
        string spriteName = sprite ? sprite.name : "LMAO NO SPRITE PRETTY CRING";
        Sprite newSprite = sprites.FirstOrDefault(s => s.name == spriteName);
        if (newSprite != null)
            _spritesDict.Add(sprite, newSprite);
        return newSprite;
    }

    private void UpdateColorsAndFlip()
    {
        Color col = shadowColor;
        col.a = playerControl.cosmetics.currentBodySprite.BodySprite.color.a;
        shadowRend.color = col;
        bool flipX = playerControl.cosmetics.currentBodySprite.BodySprite.flipX;
        shadowRend.flipX = flipX;
        neckRend.flipX = flipX;
        headRend.flipX = flipX;
    }
}
