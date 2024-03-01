using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.Minigames.MonoBehaviours;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.PurchaseBreakfast.MonoBehaviour;

[RegisterInIl2Cpp]
public sealed class CakeBehaviour(nint ptr) : UnityEngine.MonoBehaviour(ptr)
{
    public PurchaseBreakfastMinigame minigame;

    public int balance;
    public int index;
    public int price;
    public int discount;

    public Sprite traySprite;
    public Sprite singleSprite;

    public SpriteRenderer cakeRenderer;
    public SpriteRenderer leftNumber;
    public SpriteRenderer rightNumber;

    public Dictionary<int, Sprite> numbers;

    private void Awake()
    {
        cakeRenderer = transform.Find("Pastery").GetComponent<SpriteRenderer>();
        leftNumber = transform.Find("LeftMoney").GetComponent<SpriteRenderer>();
        rightNumber = transform.Find("RightMoney").GetComponent<SpriteRenderer>();
    }

    public void Start()
    {
        PolygonCollider2D polygon = cakeRenderer.gameObject.GetComponent<PolygonCollider2D>();
        ClickableSprite click = cakeRenderer.gameObject.GetComponent<ClickableSprite>();
        if (polygon) DestroyImmediate(polygon);
        if (click) DestroyImmediate(click);

        int discountedPrice = price + discount;
        leftNumber.sprite = numbers[discountedPrice / 10];
        rightNumber.sprite = numbers[discountedPrice % 10];

        cakeRenderer.sprite = traySprite;

        cakeRenderer.gameObject.AddComponent<PolygonCollider2D>();
        ClickableSprite clickableSprite = cakeRenderer.gameObject.AddComponent<ClickableSprite>();
        clickableSprite.onDown += Click;
    }

    private void OnMouseDown()
    {
        Click();
    }

    public void Click()
    {
        minigame.StartCoroutine(price == balance ? minigame.CompleteTask(this) : minigame.FailTask());
    }

    [HideFromIl2Cpp]
    public IEnumerator ItemAnimation()
    {
        const float DURATION = 0.15f;

        for (float t = 0; t < DURATION; t += Time.deltaTime)
        {
            cakeRenderer.color = Color.Lerp(Color.white, Color.clear, t / DURATION);

            yield return null;
        }
    }
}
