using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.BaseGame.Extensions;
using Submerged.Extensions;
using Submerged.Minigames.CustomMinigames.PurchaseBreakfast.MonoBehaviour;
using Submerged.Minigames.MonoBehaviours;
using Submerged.Localization.Strings;
using TMPro;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.PurchaseBreakfast;

// I have no idea wtf was going on with us when we wrote this task

[RegisterInIl2Cpp]
public sealed class PurchaseBreakfastMinigame(nint ptr) : Minigame(ptr)
{
    public MinigameProperties properties;

    // Prices
    public int[] currencyDenominations = [1, 5, 10, 20];
    public List<int> possiblePrices = [];
    public List<int> existingBillConfigurations = [];

    // Selections
    public List<int> pickedPrices = [];
    public List<int> pickedSlots = [];
    public List<int> pickedSales = [];
    public List<SpriteRenderer> billRenderers = [];
    public List<Transform> billTransforms = [];

    public List<Transform> traysMovingParts = [];
    public List<Transform> salesMovingParts = [];
    private int _amountOfBills = 5;

    private int _callCount;

    private bool _canComplete;
    private bool _hasShownEasterEgg;
    private SpriteRenderer _leftFailRenderer;
    private int _pastryAmount;

    private Sprite _previousSprite;
    private SystemRandom _random;
    private SpriteRenderer _rightFailRenderer;
    private int _seed;

    public Dictionary<int, CakeBehaviour> cakeBehaviours = new();
    public List<(Sprite single, Sprite multiple)> foodSprites = [];
    public Dictionary<int, Sprite> moneySprites = new();

    // Sprites
    public Dictionary<int, Sprite> numbers = new();
    public List<(int price, int[] bills)> priceToBill = [];

    private void Awake()
    {
        transform.Find("Cards/specials card+shadow/text").GetComponent<TextMeshPro>().text = Tasks.PurchaseBreakfast_Offers;
    }

    private void Start()
    {
        try
        {
            _callCount++;

            if (!properties) properties = GetComponent<MinigameProperties>();

            possiblePrices.Clear();
            priceToBill.Clear();
            existingBillConfigurations.Clear();
            moneySprites.Clear();
            foodSprites.Clear();
            cakeBehaviours.Clear();
            billRenderers.Clear();
            billTransforms.Clear();

            pickedPrices.Clear();
            pickedSlots.Clear();
            pickedSales.Clear();

            GeneratePossiblePrices();
            FindSprites();

            _seed = UnityRandom.Range(0, int.MaxValue);
            _random = new SystemRandom(_seed);

            ShuffleWithRandom(foodSprites, _random);
            ShuffleWithRandom(possiblePrices, _random);

            _pastryAmount = _random.Next(6, 9);

            pickedPrices.AddRange(possiblePrices.Take(_pastryAmount));
            pickedSlots.AddRange(ShuffleWithRandom(Enumerable.Range(0, 12).ToList(), _random).Take(_pastryAmount));
            pickedSales.AddRange(ShuffleWithRandomCopy(pickedSlots, _random).Take(3));

            Transform slots = transform.Find("Slots");

            for (int i = 0; i < slots.childCount; i++)
            {
                Transform t = slots.GetChild(i);
                bool active = pickedSlots.Contains(i);
                t.gameObject.SetActive(active);

                if (active)
                {
                    CakeBehaviour c = t.GetComponent<CakeBehaviour>();
                    if (c) DestroyImmediate(c);

                    int count = cakeBehaviours.Count;
                    (Sprite single, Sprite multiple) sprites = foodSprites[count];
                    CakeBehaviour cakeBehaviour = t.gameObject.AddComponent<CakeBehaviour>();
                    cakeBehaviour.minigame = this;
                    cakeBehaviour.index = i;
                    cakeBehaviour.numbers = numbers;
                    cakeBehaviour.price = pickedPrices[count];
                    cakeBehaviour.singleSprite = sprites.single;
                    cakeBehaviour.traySprite = sprites.multiple;

                    cakeBehaviours[i] = cakeBehaviour;
                }
            }

            Transform[] sales = new Transform[3];
            sales[0] = transform.Find("Sales/Left");
            sales[1] = transform.Find("Sales/Mid");
            sales[2] = transform.Find("Sales/Right");

            for (int i = 0; i < 3; i++)
            {
                Transform sale = sales[i];
                CakeBehaviour cakeBehaviour = cakeBehaviours[pickedSales[i]];
                int discount = _random.Next(1, 100 - cakeBehaviour.price);
                cakeBehaviour.discount = discount;

                sale.Find("Pastry").GetComponent<SpriteRenderer>().sprite = cakeBehaviour.singleSprite;
                sale.Find("LeftMoney").GetComponent<SpriteRenderer>().sprite = numbers[discount / 10];
                sale.Find("RightMoney").GetComponent<SpriteRenderer>().sprite = numbers[discount % 10];
            }

            List<int> cakePrices = [..pickedPrices];
            cakePrices.Sort();
            int chosenPrice = cakePrices[0];
            int[] bills = ShuffleWithRandom(ShuffleWithRandom(priceToBill, _random)
                                            .First(p => p.price == chosenPrice)
                                            .bills.ToList(),
                                            _random)
                .ToArray();

            for (int i = 0; i < bills.Length; i++)
            {
                billRenderers[i].sprite = moneySprites[bills[i]];
            }

            foreach (CakeBehaviour cake in cakeBehaviours.Values)
            {
                cake.balance = chosenPrice;
            }

            traysMovingParts =
            [
                transform.Find("Trays"),
                transform.Find("Slots"),
                transform.Find("TrayFronts")
            ];

            salesMovingParts =
            [
                transform.Find("Cards/Sliding"),
                transform.Find("Sales")
            ];

            if (!_hasShownEasterEgg && UnityRandom.Range(0, 40) == 0)
            {
                _hasShownEasterEgg = true;
                List<CakeBehaviour> range = cakeBehaviours.Values.ToList();
                range.Shuffle();
                CakeBehaviour cake = range.First(c => c.price != chosenPrice);
                List<Transform> eggs = transform.Find("EasterEggs").GetChildren().ToList();
                MatchCollection matches = Regex.Matches(cake.name, @"\d+");

                if (int.Parse(matches[0].ToString()) % 3 == 1)
                {
                    eggs.RemoveAt(0);
                }

                cake.traySprite = eggs.Random().GetComponent<SpriteRenderer>().sprite;
            }

            _canComplete = true;
        }
        catch
        {
            Message("Caught exception in PurchaseBreakfastMinigame.Start");

            if (_callCount == 10)
            {
                Close();
                Close();
                Error("Preventing crash caused by recursive method call in PurchaseBreakfastMinigame.Start!");

                throw;
            }

            possiblePrices.Clear();
            priceToBill.Clear();
            existingBillConfigurations.Clear();
            numbers.Clear();
            moneySprites.Clear();
            foodSprites.Clear();
            pickedPrices.Clear();
            pickedSlots.Clear();
            pickedSales.Clear();
            cakeBehaviours.Clear();
            billRenderers.Clear();
            billTransforms.Clear();
            traysMovingParts.Clear();
            salesMovingParts.Clear();
            _seed = 0;
            _amountOfBills = 5;
            _pastryAmount = 0;
            _random = null;

            Start();
        }
    }

    public void GeneratePossiblePrices()
    {
        possiblePrices.Clear();
        priceToBill.Clear();
        existingBillConfigurations.Clear();

        for (int i = 0; i < Math.Pow(currencyDenominations.Length, _amountOfBills); i++)
        {
            int[] bills = new int[5];
            int sum = 0;

            for (int j = 0; j < _amountOfBills; j++)
            {
                int increase = currencyDenominations[i / (int) Math.Pow(4, j) % 4];
                bills[j] = increase;
                sum += increase;
            }

            if (!possiblePrices.Contains(sum) && sum is > 9 and < 100) possiblePrices.Add(sum);

            int billIdentifier = GetBillIdentifier(bills);

            if (!existingBillConfigurations.Contains(billIdentifier) && sum is > 9 and < 100)
            {
                priceToBill.Add((sum, bills));
                existingBillConfigurations.Add(billIdentifier);
            }
        }

        possiblePrices.Sort();
    }

    [HideFromIl2Cpp]
    public int GetBillIdentifier(int[] bills) => bills[0] * 2 + bills[1] * 3 + bills[2] * 5 + bills[3] * 7 + bills[4] * 11;

    public void FindSprites()
    {
        for (int i = 0; i < 10; i++)
        {
            numbers[i] = properties.sprites[i];
        }

        Sprite[] singleSprites = properties.sprites.ToList().GetRange(10, 8).ToArray();
        Sprite[] multipleSprites = properties.sprites.ToList().GetRange(18, 8).ToArray();

        for (int i = 0; i < 8; i++)
        {
            foodSprites.Add((singleSprites[i], multipleSprites[i]));
        }

        Transform money = transform.Find("Money");

        for (int i = 0; i < money.childCount; i++)
        {
            Transform t = money.GetChild(i);
            SpriteRenderer spriteRenderer = t.GetComponent<SpriteRenderer>();
            billRenderers.Add(spriteRenderer);
            billTransforms.Add(spriteRenderer.transform);
            if (i < 4) moneySprites[currencyDenominations[i]] = spriteRenderer.sprite;
        }
    }

    [HideFromIl2Cpp]
    public IEnumerator CompleteTask(CakeBehaviour cake)
    {
        if (!_canComplete) yield break;
        _canComplete = false;

        MyNormTask.NextStep();
        StartCoroutine(CoStartClose());

        const float GATHER_DURATION = 0.15f;
        Vector3[] originalPositions = billTransforms.Select(t => t.localPosition).ToArray();

        for (float t = 0; t < GATHER_DURATION; t += Time.deltaTime)
        {
            for (int i = 0; i < originalPositions.Length; i++)
            {
                Vector3 targetPos = Vector3.Lerp(originalPositions[i], originalPositions[2], t / GATHER_DURATION);
                targetPos.z = originalPositions[i].z;
                billTransforms[i].transform.localPosition = targetPos;
            }

            yield return null;
        }

        Vector3 originalPos = billTransforms[0].parent.localPosition;
        const float DURATION = 0.15f;

        for (float t = 0; t < DURATION; t += Time.deltaTime)
        {
            billTransforms[0].parent.localPosition = Vector3.Lerp(originalPos, new Vector3(originalPos.x, -8, originalPos.z), t / DURATION);

            yield return null;
        }

        cake.StartCoroutine(cake.ItemAnimation());
    }

    [HideFromIl2Cpp]
    public IEnumerator FailTask()
    {
        if (!_canComplete) yield break;
        _canComplete = false;

        if (!_hasShownEasterEgg && UnityRandom.Range(0, 5) == 0)
        {
            _hasShownEasterEgg = true;

            int index = UnityRandom.Range(0, 7);
            Transform transition = transform.Find("Trays/FailTransition");
            _leftFailRenderer = transition.Find("Left").GetChild(1).GetChild(index).Find("Pastery").GetComponent<SpriteRenderer>();
            _rightFailRenderer = transition.Find("Right").GetChild(1).GetChild(index).Find("Pastery").GetComponent<SpriteRenderer>();

            List<Transform> eggs = transform.Find("EasterEggs").GetChildren().ToList();
            MatchCollection matches = Regex.Matches(_leftFailRenderer.transform.parent.name, @"\d+");

            if (int.Parse(matches[0].ToString()) % 3 == 1)
            {
                eggs.RemoveAt(0);
            }

            Sprite sprite = eggs.Random().GetComponent<SpriteRenderer>().sprite;

            _previousSprite = _leftFailRenderer.sprite;
            _leftFailRenderer.sprite = sprite;
            _rightFailRenderer.sprite = sprite;
        }

        Vector3 originalPos = billTransforms[0].parent.localPosition;
        float duration = 0.15f;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            billTransforms[0].parent.localPosition = Vector3.Lerp(originalPos, new Vector3(originalPos.x, -8, originalPos.z), t / duration);

            yield return null;
        }

        billTransforms[0].parent.localPosition = new Vector3(originalPos.x, -8, originalPos.z);

        Vector3[] originalPositions = salesMovingParts.Select(t => t.localPosition).ToArray();
        duration = 0.15f;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            for (int i = 0; i < salesMovingParts.Count; i++)
            {
                originalPos = originalPositions[i];

                salesMovingParts[i].localPosition = Vector3.Lerp(originalPos, new Vector3(originalPos.x, 5.5f, originalPos.z), t / duration);
            }

            yield return null;
        }

        for (int i = 0; i < salesMovingParts.Count; i++)
        {
            originalPos = originalPositions[i];

            salesMovingParts[i].localPosition = new Vector3(originalPos.x, 5.5f, originalPos.z);
        }

        originalPositions = traysMovingParts.Select(t => t.localPosition).ToArray();
        duration = 0.40f;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            for (int i = 0; i < traysMovingParts.Count; i++)
            {
                originalPos = originalPositions[i];

                traysMovingParts[i].localPosition = Vector3.Lerp(originalPos, new Vector3(22.75f, originalPos.y, originalPos.z), t / duration);
            }

            yield return null;
        }

        for (int i = 0; i < traysMovingParts.Count; i++)
        {
            originalPos = originalPositions[i];

            traysMovingParts[i].localPosition = new Vector3(22.75f, originalPos.y, originalPos.z);
        }

        for (int i = 0; i < 4; i++) billRenderers[i].sprite = moneySprites[currencyDenominations[i]];

        possiblePrices.Clear();
        priceToBill.Clear();
        existingBillConfigurations.Clear();
        numbers.Clear();
        moneySprites.Clear();
        foodSprites.Clear();
        pickedPrices.Clear();
        pickedSlots.Clear();
        pickedSales.Clear();
        cakeBehaviours.Clear();
        billRenderers.Clear();
        billTransforms.Clear();
        traysMovingParts.Clear();
        salesMovingParts.Clear();
        _seed = 0;
        _amountOfBills = 5;
        _pastryAmount = 0;
        _random = null;
        _callCount = 0;

        Start();
        _canComplete = false;

        foreach (Transform t in traysMovingParts)
        {
            Vector3 pos = t.localPosition;
            pos.x = -22.75f;
            t.localPosition = pos;
        }

        originalPositions = traysMovingParts.Select(t => t.localPosition).ToArray();
        duration = 0.40f;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            for (int i = 0; i < traysMovingParts.Count; i++)
            {
                originalPos = originalPositions[i];

                traysMovingParts[i].localPosition = Vector3.Lerp(originalPos, new Vector3(0, originalPos.y, originalPos.z), t / duration);
            }

            yield return null;
        }

        for (int i = 0; i < traysMovingParts.Count; i++)
        {
            originalPos = originalPositions[i];

            traysMovingParts[i].localPosition = new Vector3(0, originalPos.y, originalPos.z);
        }

        originalPositions = salesMovingParts.Select(t => t.localPosition).ToArray();
        duration = 0.15f;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            for (int i = 0; i < salesMovingParts.Count; i++)
            {
                originalPos = originalPositions[i];

                salesMovingParts[i].localPosition = Vector3.Lerp(originalPos, new Vector3(originalPos.x, -1.27f, originalPos.z), t / duration);
            }

            yield return null;
        }

        for (int i = 0; i < salesMovingParts.Count; i++)
        {
            originalPos = originalPositions[i];

            salesMovingParts[i].localPosition = new Vector3(originalPos.x, -1.27f, originalPos.z);
        }

        originalPos = billTransforms[0].parent.localPosition;
        duration = 0.15f;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            billTransforms[0].parent.localPosition = Vector3.Lerp(originalPos, new Vector3(originalPos.x, -1.27f, originalPos.z), t / duration);

            yield return null;
        }

        billTransforms[0].parent.localPosition = new Vector3(originalPos.x, -1.27f, originalPos.z);
        _canComplete = true;

        if (_leftFailRenderer)
        {
            _leftFailRenderer.sprite = _previousSprite;
            _rightFailRenderer.sprite = _previousSprite;
        }
    }

    public static List<T> ShuffleWithRandom<T>(List<T> list, SystemRandom random)
    {
        int index = list.Count;

        while (index > 1)
        {
            index--;
            int nextIndex = random.Next(index + 1);
            (list[nextIndex], list[index]) = (list[index], list[nextIndex]);
        }

        return list;
    }

    public static List<T> ShuffleWithRandomCopy<T>(List<T> list, SystemRandom random)
    {
        List<T> newList = [..list];
        int index = newList.Count;

        while (index > 1)
        {
            index--;
            int nextIndex = random.Next(index + 1);
            (newList[nextIndex], newList[index]) = (newList[index], newList[nextIndex]);
        }

        return newList;
    }

    public override void Close()
    {
        CooldownConsole cc = Console.Cast<CooldownConsole>();
        cc.CoolDown = cc.MaxCoolDown;
        this.BaseClose();
    }
}
