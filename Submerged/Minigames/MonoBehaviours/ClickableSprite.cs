using System;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Submerged.Minigames.MonoBehaviours;

[RegisterInIl2Cpp]
public class ClickableSprite(nint ptr) : MonoBehaviour(ptr)
{
    public Action onDown;
    public Action onDrag;
    public Action onEnter;
    public Action onExit;

    public Action onOver;

    public Action<Collider2D> onTriggerEnter;
    public Action<Collider2D> onTriggerExit;
    public Action<Collider2D> onTriggerStay;
    public Action onUp;
    public Action onUpAsButton;

    #region Events

    public virtual void OnMouseDown() => onDown?.Invoke();

    public virtual void OnMouseUp() => onUp?.Invoke();

    public virtual void OnMouseUpAsButton() => onUpAsButton?.Invoke();

    public virtual void OnMouseEnter() => onEnter?.Invoke();

    public virtual void OnMouseOver() => onOver?.Invoke();

    public virtual void OnMouseExit() => onExit?.Invoke();

    public virtual void OnMouseDrag() => onDrag?.Invoke();

    public virtual void OnTriggerEnter2D(Collider2D collider) => onTriggerEnter?.Invoke(collider);

    public virtual void OnTriggerStay2D(Collider2D collider) => onTriggerStay?.Invoke(collider);

    public virtual void OnTriggerExit2D(Collider2D collider) => onTriggerExit?.Invoke(collider);

    #endregion
}
