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
        other.killerParts.gameObject.SetActive(false);
        other.transform.Find("killstabknife").gameObject.SetActive(false);
        other.transform.Find("killstabknifehand").gameObject.SetActive(false);

        // Copy data
        petObjects = other.petObjects;
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

    }
}
