using System;
using System.Collections.Generic;
using System.Linq;
using Reactor.Utilities.Attributes;
using Submerged.BaseGame.Extensions;
using Submerged.Extensions;
using Submerged.Minigames.CustomMinigames.SortScubaGear.Enums;
using Submerged.Minigames.CustomMinigames.SortScubaGear.MonoBehaviours;
using Submerged.Minigames.MonoBehaviours;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.SortScubaGear;

[RegisterInIl2Cpp]
public sealed class SortScubaMinigame(nint ptr) : Minigame(ptr)
{
    public MinigameProperties properties;

    public List<ScubaGearItem> scubaGearItems = [];

    public List<Transform> boxes = [];

    public List<Draggable> hovering = [];

    private bool _forceClose;
    public Dictionary<ScubaGearType, SortScubaBox> scubaBoxes = new();

    private void Start()
    {
        properties = GetComponent<MinigameProperties>();

        Transform itemsParent = transform.Find("Items");

        foreach (Transform t in itemsParent.GetChildren())
        {
            ScubaGearItem scubaGearItem = t.gameObject.AddComponent<ScubaGearItem>();
            scubaGearItem.minigame = this;

            AudioClip pickup, drop;

            switch (t.name)
            {
                case "Flipper":
                    pickup = properties.audioClips[0];
                    drop = properties.audioClips[1];

                    break;

                case "Wetsuit":
                    pickup = properties.audioClips[2];
                    drop = properties.audioClips[3];

                    break;

                case "Mask":
                    pickup = properties.audioClips[4];
                    drop = properties.audioClips[5];

                    break;

                default:
                    pickup = properties.audioClips[6];
                    drop = properties.audioClips[7];

                    break;
            }

            scubaGearItem.draggable.onDown += () => { SoundManager.Instance.PlaySound(pickup, false, 0.5f); };

            scubaGearItem.draggable.onUp += () =>
            {
                SoundManager.Instance.PlaySound(drop, false, 0.5f);

                Vector3 pos = scubaGearItem.transform.localPosition;
                pos.z = -1.05f;
                scubaGearItem.transform.localPosition = pos;

                if (CheckBoxes())
                {
                    _forceClose = true;
                    if (MyNormTask != null)
                    {
                        MyNormTask.NextStep();
                    }
                    StartCoroutine(CoStartClose());
                }
            };

            scubaGearItem.draggable.onDrag += () =>
            {
                Transform scubaTransform = scubaGearItem.transform;
                Vector3 pos = scubaTransform.localPosition;
                pos.z = -15f;
                scubaTransform.localPosition = pos;
            };

            scubaGearItem.draggable.onEnter += () => hovering.Add(scubaGearItem.draggable);
            scubaGearItem.draggable.onExit += () => hovering.RemoveAll(d => d.GetHashCode() == scubaGearItem.draggable.GetHashCode());

            scubaGearItems.Add(scubaGearItem);
        }

        Transform boxParent = transform.Find("Boxes");
        boxes = boxParent.GetChildren().ToList();
        boxes.ForEach(t =>
        {
            SortScubaBox scubaBox = t.gameObject.AddComponent<SortScubaBox>();

            Enum.TryParse(t.name, out scubaBox.targetType);

            scubaBox.polygonCollider2D = t.GetComponent<PolygonCollider2D>();

            scubaBoxes.Add(scubaBox.targetType, scubaBox);
        });

        do
        {
            ShuffleGear();
        }
        while (CheckBoxes());
    }

    public void ShuffleGear()
    {
        List<int> scubaNums = Enumerable.Range(0, 8).ToList();
        scubaNums.Shuffle();

        List<Vector3> scubaGearPositions = scubaGearItems.Select(s => s.transform.position).ToList();

        for (int i = 0; i < 8; i++)
        {
            scubaGearItems[i].transform.position = scubaGearPositions[scubaNums[i]];
        }

        List<int> boxNums = Enumerable.Range(0, 4).ToList();
        boxNums.Shuffle();
    }

    public bool CheckBoxes()
    {
        if (amClosing != CloseState.None) return false;

        foreach (ScubaGearItem scubaGearItem in scubaGearItems)
        {
            bool correctPosition = scubaBoxes[scubaGearItem.itemType].polygonCollider2D.OverlapPoint(scubaGearItem.transform.position);

            if (!correctPosition) return false;
        }

        return true;
    }

    public override void Close()
    {
        if (hovering.Count == 0 || _forceClose) this.BaseClose();
    }
}
