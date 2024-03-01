using Reactor.Utilities.Attributes;
using Submerged.Minigames.CustomMinigames.SortScubaGear.Enums;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.SortScubaGear.MonoBehaviours;

[RegisterInIl2Cpp]
public sealed class SortScubaBox(nint ptr) : MonoBehaviour(ptr)
{
    public ScubaGearType targetType;
    public PolygonCollider2D polygonCollider2D;
}
