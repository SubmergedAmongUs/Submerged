using Reactor.Utilities.Attributes;
using Submerged.BaseGame.Interfaces;
using UnityEngine;

namespace Submerged.Minigames.MonoBehaviours;

[RegisterInIl2Cpp(typeof(IUsableCoolDown))]
public sealed class CooldownConsole(nint ptr) : Console(ptr), AU.IUsableCoolDown
{
    public float CoolDown { get; set; }
    public float MaxCoolDown { get; set; } = 3;

    public bool IsCoolingDown() => CoolDown > 0;

    private void Update()
    {
        CoolDown = Mathf.Max(0, CoolDown - Time.deltaTime);
    }

    public override float PercentCool => CoolDown / MaxCoolDown;
}
