using System;
using UnityEngine;

namespace PowerTools
{
    public class SpriteAnimNodeSync : MonoBehaviour
    {
        public int NodeId;

        public SpriteAnimNodes Parent;

        public SpriteRenderer ParentRenderer;

        public SpriteRenderer Renderer;

        public void LateUpdate()
        {
            bool flag = false;
            if (this.ParentRenderer)
            {
                flag = this.ParentRenderer.flipX;
                if (this.Renderer)
                {
                    this.Renderer.flipX = this.ParentRenderer.flipX;
                }
                else
                {
                    flag = flag;
                }
            }
            else if (this.Renderer)
            {
                flag = this.Renderer.flipX;
            }
            Vector3 localPosition = base.transform.localPosition;
            Vector3 localPosition2 = this.Parent.GetLocalPosition(this.NodeId, false);
            localPosition.x = localPosition2.x;
            localPosition.y = localPosition2.y;
            base.transform.localPosition = localPosition;
            float angle = this.Parent.GetAngle(this.NodeId);
            if (flag)
            {
                base.transform.eulerAngles = new Vector3(0f, 0f, -angle);
                return;
            }
            base.transform.eulerAngles = new Vector3(0f, 0f, angle);
        }
    }
}