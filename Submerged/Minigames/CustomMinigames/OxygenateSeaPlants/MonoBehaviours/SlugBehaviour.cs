using System;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.OxygenateSeaPlants.MonoBehaviours;

[RegisterInIl2Cpp]
public sealed class SlugBehaviour(nint ptr) : MonoBehaviour(ptr)
{
    public Collider2D collider2D;
    public Rigidbody2D body;

    public bool entered;
    public float timer;

    public GameObject @object;
    public OxygenateCoralMinigame minigame;

    private void Start()
    {
        Destroy(GetComponent<BoxCollider2D>());
        GetComponent<PolygonCollider2D>().isTrigger = true;
    }

    private void Update()
    {
        if (!entered) return;

        timer += Time.deltaTime;

        if (timer > 0.05f)
        {
            timer = 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name.Contains("Bubble", StringComparison.OrdinalIgnoreCase))
        {
            minigame.ResetBubble();

            @object = other.gameObject;
            entered = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.name.Contains("Bubble", StringComparison.OrdinalIgnoreCase))
        {
            entered = false;
        }
    }
}
