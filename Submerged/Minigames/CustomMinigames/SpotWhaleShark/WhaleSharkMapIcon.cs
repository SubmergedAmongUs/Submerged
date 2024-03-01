using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.SpotWhaleShark;

[RegisterInIl2Cpp]
public sealed class WhaleSharkMapIcon(nint ptr) : MonoBehaviour(ptr)
{
    public WhaleSharkTask task;

    public bool lastVisible;

    private SpriteRenderer _icon;

    public void Awake()
    {
        _icon = GetComponent<SpriteRenderer>();
    }

    public void Update()
    {
        bool isVisible = task.visible || !WhaleSharkTask.CanComplete(PlayerControl.LocalPlayer);

        if (isVisible != lastVisible)
        {
            lastVisible = isVisible;
            UpdateIcon();
        }
    }

    public void UpdateIcon()
    {
        _icon.color = lastVisible ? new Color(1, 0.9216f, 0.0157f) : new Color(0.8f, 0.8f, 0.8f, 0.8f);
    }
}
