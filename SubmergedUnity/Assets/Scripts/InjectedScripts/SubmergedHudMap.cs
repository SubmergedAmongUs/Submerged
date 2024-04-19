using JetBrains.Annotations;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Submerged.HudMap
{
    public class SubmergedHudMap : MonoBehaviour
    {
        public Transform hudTransform;
        public MapBehaviour map;

        public GameObject upArrow;
        public GameObject downArrow;

        [UsedImplicitly]
        public void MoveMapUp()
        {
        }

        [UsedImplicitly]
        public void MoveMapDown()
        {
        }
    }
}
