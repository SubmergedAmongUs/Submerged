using Reactor.Utilities.Attributes;

namespace Submerged.Floors.Objects;

[RegisterInIl2Cpp]
public class PlayerShadowRenderer(nint ptr) : RelativeShadowRenderer(ptr)
{
    public PlayerControl player;

    protected override void Start()
    {
        base.Start();
        player = GetComponentInParent<PlayerControl>();
    }

    public override bool EnableShadow => player.isDummy || (!player.Data.IsDead && !player.Data.Disconnected);
}
