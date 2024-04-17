using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Submerged.Systems.Elevator
{
    public class SubmarineElevator : MonoBehaviour
    {
        public MeshRenderer lowerElevator;
        public MeshFilter lowerElevatorFilter;
        public MeshRenderer upperElevator;
        public MeshFilter upperElevatorFilter;

        public BoxCollider2D lowerElevatorCollider;
        public BoxCollider2D upperElevatorCollider;

        public SpriteRenderer lowerBlackout;
        public SpriteRenderer upperBlackout;

        public Texture elevatorWithoutDoorL1;
        public Texture elevatorWithDoorL1;

        public Texture elevatorWithoutDoorL2;
        public Texture elevatorWithDoorL2;

        public PlainDoor lowerOuterDoor;
        public PlainDoor lowerInnerDoor;
        public SpriteRenderer lowerInnerRend;

        public PlainDoor upperOuterDoor;
        public PlainDoor upperInnerDoor;
        public SpriteRenderer upperInnerRend;
        
        public SpriteRenderer lowerLight;
        public SpriteRenderer upperLight;
    }
}