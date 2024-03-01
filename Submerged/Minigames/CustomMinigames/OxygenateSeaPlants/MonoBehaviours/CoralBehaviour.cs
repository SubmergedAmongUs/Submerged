using BepInEx.Unity.IL2CPP.Utils;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.OxygenateSeaPlants.MonoBehaviours;

[RegisterInIl2Cpp]
public sealed class CoralBehaviour(nint ptr) : MonoBehaviour(ptr)
{
    public OxygenateCoralMinigame minigame;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name == "Bubble")
        {
            minigame.StartCoroutine(minigame.Oxygenate());
        }
    }
}
