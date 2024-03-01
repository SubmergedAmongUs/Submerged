using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.Minigames.MonoBehaviours;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.CamsSabotage.MonoBehaviours;

[RegisterInIl2Cpp]
public sealed class CamSlider(nint ptr) : Draggable(ptr)
{
    public FloatRange localXRange = new(-1.84f, 1.84f);

    [HideFromIl2Cpp]
    public float SliderValue => (transform.localPosition.x - localXRange.min) / (localXRange.max - localXRange.min);

    private void Start()
    {
        if (PlayerControl.LocalPlayer.Data.IsDead)
        {
            enabled = false;
        }
    }

    public void SetSliderValue(float value)
    {
        Transform transform1 = transform;
        Vector3 pos = transform1.localPosition;
        transform1.localPosition = new Vector3(localXRange.min + (localXRange.max - localXRange.min) * value, pos.y, pos.z);
    }
}
