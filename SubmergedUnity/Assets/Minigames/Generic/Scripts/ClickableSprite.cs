using System;
using UnityEngine;
// ReSharper disable UnassignedField.Global

namespace Tasks.Tasks.PurchaseBreakfast
{
    public class ClickableSprite : MonoBehaviour
    {
        public Action OnDown;
        public Action OnUp;
        public Action OnUpAsButton;

        public Action OnOver;
        public Action OnEnter;
        public Action OnExit;
        public Action OnDrag;
        
        public Action<Collider2D> OnTriggerEnter;
        public Action<Collider2D> OnTriggerStay;
        public Action<Collider2D> OnTriggerExit;

        #region Events

        public virtual void OnMouseDown() => OnDown?.Invoke();
        
        public virtual void OnMouseUp() => OnUp?.Invoke();

        public virtual void OnMouseUpAsButton() => OnUpAsButton?.Invoke();

        public virtual void OnMouseEnter() => OnEnter?.Invoke();

        public virtual void OnMouseOver() => OnOver?.Invoke();
        
        public virtual void OnMouseExit() => OnExit?.Invoke();

        public virtual void OnMouseDrag() => OnDrag?.Invoke();

        public virtual void OnTriggerEnter2D(Collider2D collider) => OnTriggerEnter?.Invoke(collider);

        public virtual void OnTriggerStay2D(Collider2D collider) => OnTriggerStay?.Invoke(collider);

        public virtual void OnTriggerExit2D(Collider2D collider) => OnTriggerExit?.Invoke(collider);

        #endregion
    }
}