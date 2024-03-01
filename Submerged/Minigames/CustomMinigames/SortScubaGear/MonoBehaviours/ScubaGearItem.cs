using System;
using System.Linq;
using Reactor.Utilities.Attributes;
using Submerged.Minigames.CustomMinigames.SortScubaGear.Enums;
using Submerged.Minigames.MonoBehaviours;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.SortScubaGear.MonoBehaviours;

[RegisterInIl2Cpp]
public sealed class ScubaGearItem(nint ptr) : MonoBehaviour(ptr)
{
    public SortScubaMinigame minigame;

    public ScubaGearType itemType;
    public Draggable draggable;

    private void Awake()
    {
        draggable = gameObject.AddComponent<Draggable>();

        Enum.TryParse(name, out itemType);
    }

    private void Update()
    {
        if (transform.localPosition.z > -2f)
        {
            bool correctPosition = (minigame.scubaBoxes.Values.Any(b =>
                                                                       b.polygonCollider2D.ClosestPoint(transform.position) == (Vector2) transform.position) &&
                                    transform.localPosition.y > 5) ||
                                   transform.localPosition.y is > -4 and < 1;

            Vector3 pos = transform.localPosition;
            pos.z = correctPosition ? -0.05f : -1.05f;
            transform.localPosition = pos;
        }
    }
}
