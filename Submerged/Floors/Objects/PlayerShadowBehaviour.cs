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
    public SpriteRenderer playerRend;
    public PlayerControl playerControl;
    public Sprite[] sprites;
    public Color shadowColor;

    private readonly Dictionary<Sprite, Sprite> _spritesDict = new();

    public void Start()
    {
        Camera.main!.cullingMask = 1073969927; // yay magic numbers
        shadowObj = new GameObject("Submerged Shadow") { layer = 4 };
        shadowRend = shadowObj.AddComponent<SpriteRenderer>();
        shadowObj.transform.parent = transform;
        shadowObj.transform.localPosition = Vector3.zero;
        shadowObj.transform.localScale = Constants.ShouldHorseAround() ? new Vector3(0.6734f * 0.5f, 0.6734f * 0.5f, 1f) : new Vector3(0.5f, 0.5f, 1f);
        shadowColor = shadowRend.color;
        sprites = SubmarineStatus.instance.minigameProperties.sprites;

        playerRend = playerControl.cosmetics.currentBodySprite.BodySprite;
    }

    public void LateUpdate()
    {
        if (playerControl == null) return;
        bool shouldBeActive = playerRend.enabled && !playerControl.Data.IsDead;
        shadowObj.SetActive(shouldBeActive);

        if (!shouldBeActive) return;

        Sprite sprite = playerRend.sprite;
        Sprite newSprite = null;

        if (sprite && !_spritesDict.TryGetValue(sprite, out newSprite))
        {
            string spriteName = sprite ? sprite.name : "LMAO NO SPRITE PRETTY CRING";
            newSprite = sprites.FirstOrDefault(s => s.name == spriteName);
            _spritesDict.Add(sprite, newSprite);
        }

        if (!newSprite) newSprite = sprite;

        shadowRend.sprite = newSprite;
        Color col = shadowColor;
        col.a = playerRend.color.a;
        shadowRend.color = col;
        shadowRend.flipX = playerRend.flipX;
    }
}
