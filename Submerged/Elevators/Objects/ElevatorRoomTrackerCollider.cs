using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Submerged.Elevators.Objects;

[RegisterInIl2Cpp]
public sealed class ElevatorRoomTrackerCollider(nint ptr) : MonoBehaviour(ptr)
{
    public Il2CppReferenceField<PolygonCollider2D> targetCollider;
    public Il2CppReferenceField<BoxCollider2D> collider1;
    public Il2CppReferenceField<BoxCollider2D> collider2;

    private void Awake()
    {
        PolygonCollider2D col = targetCollider.Value;
        col.pathCount = 2;
        col.SetPath(0, GetPath(collider1.Value));
        col.SetPath(1, GetPath(collider2.Value));
    }

    [HideFromIl2Cpp]
    private Vector2[] GetPath(BoxCollider2D col)
    {
        Vector2 ext = col.size * 0.5f;
        Vector2 off = col.offset;
        Transform t = col.transform;
        Transform myT = transform;

        return
        [
            myT.InverseTransformPoint(t.TransformPoint(off + new Vector2(-ext.x, -ext.y))),
            myT.InverseTransformPoint(t.TransformPoint(off + new Vector2(-ext.x,  ext.y))),
            myT.InverseTransformPoint(t.TransformPoint(off + new Vector2( ext.x,  ext.y))),
            myT.InverseTransformPoint(t.TransformPoint(off + new Vector2( ext.x, -ext.y)))
        ];
    }
}
