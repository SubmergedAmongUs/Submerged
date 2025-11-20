using UnityEngine;
using UnityEngine.Serialization;

// ReSharper disable once CheckNamespace
namespace Submerged.Elevators.Objects
{
    public class ElevatorRoomTrackerCollider : MonoBehaviour
    {
        public PolygonCollider2D targetCollider;
        [FormerlySerializedAs("topCollider")]
        public BoxCollider2D collider1;
        [FormerlySerializedAs("bottomCollider")]
        public BoxCollider2D collider2;
    }
}
