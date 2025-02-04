using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.Enums;
using Submerged.Extensions;
using Submerged.Minigames.CustomMinigames.CleanGlass.MonoBehaviours;
using Submerged.Minigames.MonoBehaviours;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.CleanGlass;

[RegisterInIl2Cpp]
public sealed class CleanGlassMinigame(nint ptr) : Minigame(ptr)
{
    public GameObject[] smudges;
    public Draggable cloth;

    public List<GameObject> activeSmudges;
    public List<Smudge> activeSmudgeBehaviours = new();

    private bool _finished;
    private bool _hovering;

    private MinigameProperties _minigameProperties;

    private void Start()
    {
        _minigameProperties = GetComponent<MinigameProperties>();
        smudges = transform.Find("Stains").GetChildren().Select(t => t.gameObject).ToArray();
        cloth = transform.Find("Cloth").gameObject.AddComponent<Draggable>();
        cloth.onEnter += () => _hovering = true;
        cloth.onExit += () => _hovering = false;
        activeSmudges = smudges.ShuffleCopy().Take(UnityRandom.Range(2, 6)).ToList();
        AudioClip[] windowAudio = _minigameProperties.audioClips;

        foreach (GameObject smudgeObj in activeSmudges)
        {
            Smudge smudge = smudgeObj.AddComponent<Smudge>();
            smudge.draggable = cloth;
            smudgeObj.gameObject.SetActive(true);
            smudge.squeakSounds = windowAudio;
            activeSmudgeBehaviours.Add(smudge);
        }
    }

    private void Update()
    {
        if (_finished) return;
        if (!CheckComplete() || amClosing != CloseState.None) return;
        _finished = true;
        PlaySparkle();
    }

    [HideFromIl2Cpp]
    private IEnumerator Sparkle()
    {
        if (MyNormTask != null)
        {
            MyNormTask.NextStep();
        }
        StartCoroutine(CoStartClose());

        Animator anim = transform.Find("Sparkle").GetComponent<Animator>();

        yield return anim.PlayAndWaitForAnimation("SparkleAnimation");
        anim.GetComponent<SpriteRenderer>().enabled = false;

        if (!PlayerControl.LocalPlayer.myTasks.ToArray().Any(t => t.TaskType == CustomTaskTypes.CleanGlass && !t.IsComplete))
        {
            GameObject.Find("CleanGlassConsole").GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    private bool CheckComplete()
    {
        foreach (Smudge smudge in activeSmudgeBehaviours)
        {
            if (smudge.health <= 0) continue;

            return false;
        }

        return true;
    }

    private void PlaySparkle()
    {
        this.StartCoroutine(Sparkle());
    }

    [UsedImplicitly]
    private void TryClose()
    {
        if (!_hovering) _minigameProperties.CloseTask();
    }
}
