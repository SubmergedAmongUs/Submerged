using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using PowerTools;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Submerged.KillAnimation;

[RegisterInIl2Cpp]
public sealed class CustomKillAnimationPlayer : MonoBehaviour
{
    private const float ANIM_DURATION = 2.5f;

    public const string OXYGEN_DEATH_ANIM = "0,0,4,0,0;" +
                                            "0,1.08,2,0,0;" +
                                            "0,1.15,2,0,0;" +
                                            "0,1.28,2,0,0;" +
                                            "0,1.33,4,0,0;" +
                                            "0,1.37,2,0,0;" +
                                            "0,1.39,4,0,0;" +
                                            "0,1.42,4,0,0;" +
                                            "0,1.46,4,0,0;" +
                                            "0,1.5,4,0,0;" +
                                            "0,1.56,4,0,0;" +
                                            "3,0.43,3,-0.3,0;" +
                                            "3,0.66,4,-0.5,0;" +
                                            "3,0.89,4,-0.5,0;" +
                                            "3,0.97,4,-0.5,0;" +
                                            "3,1.01,4,-0.5,0;" +
                                            "3,1.05,4,-0.5,0;" +
                                            "3,1.11,4,-0.5,0;" +
                                            "3,1.14,4,-0.5,0;" +
                                            "3,1.17,4,-0.5,0;" +
                                            "3,1.23,4,-0.5,0;" +
                                            "3,1.27,4,-0.5,0;" +
                                            "3,1.01,3,-0.5,0;" +
                                            "3,0.97,4,-0.5,0;" +
                                            "3,0.93,16,-0.5,0;";

    public float playhead;
    public bool finished;

    private readonly Dictionary<int, KillAnimFrame> _frames = new();
    private OverlayKillAnimation _animOverlay;
    private SpriteAnim _bodyAnim;
    private SpriteAnim _skinAnim;
    private GameObject _victimObj;

    public void Awake()
    {
        _animOverlay = GetComponent<OverlayKillAnimation>();
        _victimObj = _animOverlay.victimParts.gameObject;
        _bodyAnim = _animOverlay.victimParts.cosmetics.bodySprites._items[0].BodySprite.GetComponent<SpriteAnim>();
        _skinAnim = _animOverlay.victimParts.GetSkinSpriteAnim();

        LoadFrom(OXYGEN_DEATH_ANIM);
    }

    private void Start()
    {
        _bodyAnim.Speed = 0;
        _bodyAnim.Stop();

        _skinAnim.Speed = 0;
        _skinAnim.Stop();
    }

    public void Update()
    {
        if (!_frames.Any()) return;

        playhead += Time.deltaTime;

        if (playhead > ANIM_DURATION)
        {
            finished = true;

            return;
        }

        float timePerFrame = ANIM_DURATION / _frames.Count;
        int currentPos = (int) Math.Floor(playhead / timePerFrame);

        KillAnimFrame currentFrame = _frames[currentPos];
        UpdateVisuals(currentFrame.time, currentFrame.offset, currentFrame.animation);
    }

    public void UpdateVisuals(float sampleTime, Vector2 characterOffset, int animation)
    {
        CosmeticsLayer animCosmetics = HudManager.Instance.KillOverlay.KillAnims[animation].victimParts.cosmetics;
        AnimationClip bodyClip = animCosmetics.bodySprites._items[0].BodySprite.GetComponent<SpriteAnim>().m_defaultAnim;

        SkinViewData skinData = _animOverlay.victimParts.cosmetics.skin.skin;
        AnimationClip skinClip = animation switch
        {
            0 => skinData.KillStabVictim,
            1 => skinData.KillNeckVictim,
            2 => skinData.KillTongueVictim,
            3 => skinData.KillShootVictim,
            _ => null
        };

        UpdateVisuals(sampleTime, characterOffset, bodyClip, skinClip);
    }

    public void UpdateVisuals(float sampleTime, Vector2 characterOffset, AnimationClip bodyAnimClip, AnimationClip skinAnimClip)
    {
        AnimationClip.SampleAnimation(_bodyAnim.gameObject, bodyAnimClip, sampleTime, WrapMode.Default);
        if (skinAnimClip) AnimationClip.SampleAnimation(_skinAnim.gameObject, skinAnimClip, sampleTime, WrapMode.Default);

        _victimObj.transform.localPosition = characterOffset + new Vector2(-1.5f, 0);
    }

    public void LoadFrom(string toLoad)
    {
        int frameCount = 0;
        _frames.Clear();

        foreach (string dataString in toLoad.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            KillAnimFrame frame = KillAnimFrame.Deserialize(dataString);

            for (int idx = frameCount; idx < frameCount + frame.length; idx++)
            {
                _frames[idx] = frame;
            }

            frameCount += frame.length;
        }

        if (frameCount == 0) return;
        _frames[frameCount] = _frames[frameCount - 1];
    }

    [HideFromIl2Cpp]
    public IEnumerator WaitForFinish()
    {
        while (!finished) yield return null;
    }
}
