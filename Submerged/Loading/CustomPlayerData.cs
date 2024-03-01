using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Submerged.Loading;

[RegisterInIl2Cpp]
public sealed class CustomPlayerData : MonoBehaviour
{
    private bool _hasMap;

    public bool HasMap
    {
        get => player.AmOwner ? !AssetLoader.Errored : _hasMap || player.isDummy;
        set => _hasMap = value;
    }

    public PlayerControl player;

    public CustomPlayerData(IntPtr ptr) : base(ptr) { }

    public void Awake()
    {
        player = GetComponent<PlayerControl>();
    }
}
