using System;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Reactor.Utilities.Attributes;
using Submerged.BaseGame;
using Submerged.Enums;
using UnityEngine;

namespace Submerged.KillAnimation;

[RegisterInIl2Cpp]
public class OxygenDeathAnimation(nint ptr) : OverlayKillAnimation(ptr)
{
    public void CreateFrom(OverlayKillAnimation other)
    {
        // Disable unused
        petObjects = new ICG.HashSet<GameObject>();
        other.killerParts.gameObject.SetActive(false);
        other.transform.Find("killstabknife").gameObject.SetActive(false);
        other.transform.Find("killstabknifehand").gameObject.SetActive(false);

        // Copy data
        Sfx = other.Sfx;
        Stinger = other.Stinger;
        StingerVolume = other.StingerVolume;
        victimHat = other.victimHat;
        victimParts = other.victimParts;
        VictimPetPosition = other.VictimPetPosition;

        // Modifications
        victimParts.transform.localPosition = new Vector3(-1.5f, 0, 0);
        KillType = CustomKillAnimTypes.Oxygen;

        // Components
        Destroy(other);
        gameObject.AddComponent<CustomKillAnimationPlayer>();
    }

    [BaseGameCode(LastChecked.v17_0_1, "Entire method is copied because WaitForFinish is not virtual.")]
    public override CppIEnumerator CoShow(KillOverlay parent)
    {
        return enumerator().WrapToIl2Cpp();

        IEnumerator enumerator()
        {
            if (Constants.ShouldPlaySfx())
            {
                SoundManager.Instance.PlaySound(Stinger, false, 1f, null).volume = StingerVolume;
            }

            parent.background.enabled = true;
            yield return Effects.Wait(0.083333336f);
            parent.background.enabled = false;
            parent.flameParent.SetActive(true);
            parent.flameParent.transform.localScale = new Vector3(1f, 0.3f, 1f);
            parent.flameParent.transform.localEulerAngles = new Vector3(0f, 0f, 25f);
            yield return Effects.Wait(0.083333336f);
            parent.flameParent.transform.localScale = new Vector3(1f, 0.5f, 1f);
            parent.flameParent.transform.localEulerAngles = new Vector3(0f, 0f, -15f);
            yield return Effects.Wait(0.083333336f);
            parent.flameParent.transform.localScale = new Vector3(1f, 1f, 1f);
            parent.flameParent.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            gameObject.SetActive(true);
            yield return GetComponent<CustomKillAnimationPlayer>().WaitForFinish(); // Only changed line
            gameObject.SetActive(false);
            yield return new WaitForLerp(0.16666667f, new Action<float>(t =>
            {
                parent.flameParent.transform.localScale = new Vector3(1f, 1f - t, 1f);
            }));
            parent.flameParent.SetActive(false);
        }
    }
}
