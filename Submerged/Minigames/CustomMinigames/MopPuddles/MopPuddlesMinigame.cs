using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using Submerged.Minigames.CustomMinigames.MopPuddles.MonoBehaviours;
using Submerged.Minigames.MonoBehaviours;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.MopPuddles;

[RegisterInIl2Cpp]
public sealed class MopPuddlesMinigame(nint ptr) : Minigame(ptr)
{
    public MinigameProperties minigameProperties;

    public Transform[] puddleTransforms;
    // public SpriteRenderer[] PuddleSpriteRenderers;
    // public Sprite[] PuddleSprites;

    public Transform bucketWater;

    public Transform mop;
    public Draggable mopDraggable;
    public Material bucketBackMat;
    public Transform bucketTarget;

    public List<PuddleBehaviour> puddles = [];
    public int driedPuddles;

    public bool mopDirty;
    public GameObject wetMop;
    public GameObject dryMop;

    public Transform mopHead;
    private bool _blockClose;
    private Vector3 _bucketWaterEndPos;
    private Vector3 _bucketWaterStartPos;

    private void Start()
    {
        minigameProperties = GetComponent<MinigameProperties>();
        transform.Find("Backgrounds").GetChild(ConsoleId).gameObject.SetActive(true);

        puddleTransforms = transform.Find("Puddles").GetChildren();
        /*
        PuddleSpriteRenderers = PuddleTransforms.Select(t => t.GetComponent<SpriteRenderer>()).ToArray();
        PuddleSprites = PuddleSpriteRenderers.Select(r => r.sprite).ToArray();
        IList<int> shuffledRends = Enumerable.Range(0, PuddleSpriteRenderers.Length).ToList().Shuffle();
        for (int i = 0; i < shuffledRends.Count; i++)
        {
            PuddleSpriteRenderers[i].sprite = PuddleSprites[shuffledRends[i]];
        }
        */

        bucketWater = transform.Find("Bucket/BucketWater");
        _bucketWaterStartPos = bucketWater.localPosition;
        _bucketWaterEndPos = transform.Find("Bucket/BucketWaterPeak").localPosition;
        bucketBackMat = transform.Find("Bucket/BucketBack").GetComponent<SpriteRenderer>().material;
        bucketTarget = transform.Find("Bucket/BucketTarget");

        mop = transform.Find("Mop");
        mopDraggable = mop.gameObject.AddComponent<Draggable>();
        wetMop = mop.Find("WetMop").gameObject;
        dryMop = mop.Find("DryMop").gameObject;
        mopHead = transform.Find("Mop/MopHead");

        foreach (Transform puddleTransform in puddleTransforms)
        {
            puddleTransform.gameObject.SetActive(false);
        }

        puddles = puddleTransforms.Shuffle()
                                  .Take(3)
                                  .Select(t => t.gameObject.AddComponent<PuddleBehaviour>())
                                  .ToList();

        foreach (PuddleBehaviour puddle in puddles)
        {
            puddle.gameObject.SetActive(true);
            puddle.draggable = mopDraggable;
            puddle.minigame = this;
        }

        mopDraggable.onOver += () => _blockClose = true;
        mopDraggable.onExit += () => _blockClose = false;
    }

    private void Update()
    {
        float magnitude = ((Vector2) (bucketTarget.position - mopHead.transform.position)).magnitude;

        if (mopDirty && magnitude < 0.75)
        {
            SetMopDry();
        }
    }

    private void UpdatePuddle()
    {
        driedPuddles++;

        if (driedPuddles == puddles.Count)
        {
            SoundManager.Instance.PlaySound(minigameProperties.audioClips[0], false, 0.33f);
            this.StartCoroutine(FinishTasks());

            return;
        }

        SoundManager.Instance.PlaySound(minigameProperties.audioClips[1], false, 0.33f);
        bucketWater.localPosition = Vector3.Lerp(_bucketWaterStartPos, _bucketWaterEndPos, (float) driedPuddles / puddles.Count);
    }

    public void SetMopDirty()
    {
        mopDirty = true;
        wetMop.SetActive(true);
        dryMop.SetActive(false);
        bucketBackMat.SetFloat("_Outline", 1);
    }

    public void SetMopDry()
    {
        mopDirty = false;
        wetMop.SetActive(false);
        dryMop.SetActive(true);
        bucketBackMat.SetFloat("_Outline", 0);
        UpdatePuddle();
    }

    [HideFromIl2Cpp]
    private IEnumerator FinishTasks()
    {
        MyNormTask.GetComponent<MopPuddlesTask>().validConsoleIds.Remove(Console.ConsoleId);
        MyNormTask!?.NextStep();
        StartCoroutine(CoStartClose(1f));

        Animator anim = transform.Find("MopAnimation").GetComponent<Animator>();
        wetMop.SetActive(false);
        dryMop.SetActive(false);
        anim.gameObject.SetActive(true);
        bucketWater.localPosition = Vector3.Lerp(_bucketWaterStartPos, _bucketWaterEndPos, 1);

        yield return anim.PlayAndWaitForAnimation("MopAnim");
    }

    private void TryClose()
    {
        if (!_blockClose)
        {
            GetComponent<MinigameProperties>().CloseTask();
        }
    }
}
