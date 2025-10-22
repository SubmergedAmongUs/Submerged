using System.Collections.Generic;
using System.Linq;
using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using Submerged.Minigames.CustomMinigames.ResetBreakers.MonoBehaviours;
using Submerged.Minigames.MonoBehaviours;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.ResetBreakers;

[RegisterInIl2Cpp]
public sealed class ResetBreakersMinigame(nint ptr) : Minigame(ptr)
{
    public MinigameProperties minigameProperties;

    public List<Sprite> letters;
    public GameObject switches;

    public AudioClip breakerClick;

    public List<CircuitBreaker> circutBreakers = [];

    private void Start()
    {
        minigameProperties = GetComponent<MinigameProperties>();
        letters = minigameProperties.sprites.Take(36).ToList();
        switches = transform.Find("Switches").gameObject;
        breakerClick = minigameProperties.audioClips[0];

        for (int i = 0; i < switches.transform.childCount; i++)
        {
            CircuitBreaker breaker = switches.transform.GetChild(i).gameObject.AddComponent<CircuitBreaker>();
            circutBreakers.Add(breaker);
            breaker.breakerClick = breakerClick;
        }

        circutBreakers.Random().SetState(false);
        SetRandomChars();
    }

    public void Update()
    {
        if (amClosing != CloseState.None || MyNormTask.IsComplete) return;

        if (CheckSwitches())
        {
            foreach (CircuitBreaker circutBreaker in circutBreakers)
            {
                circutBreaker.enabled = false;
            }
            if (MyNormTask != null)
            {
                MyNormTask.NextStep();
            }
            StartCoroutine(CoStartClose());
        }
    }

    public void SetRandomChars()
    {
        KeyCode[] keys = Enumerable.Range((int) KeyCode.A, 26)
            .Concat(Enumerable.Range((int) KeyCode.Alpha0, 10))
            .Select(k => (KeyCode) k)
            .ToArray();
        keys.Shuffle();

        Warning(string.Join(", ", keys));

        for (int i = 0; i < circutBreakers.Count; i++)
        {
            circutBreakers[i].targetKey = keys[i];
            circutBreakers[i].character.sprite = letters.FirstOrDefault(l => l.name.Contains($"_{keys[i].ToString()[^1..].ToUpper()}"));
        }
    }

    public bool CheckSwitches()
    {
        foreach (CircuitBreaker circutBreaker in circutBreakers)
        {
            if (!circutBreaker.complete) return false;
        }

        return true;
    }
}
