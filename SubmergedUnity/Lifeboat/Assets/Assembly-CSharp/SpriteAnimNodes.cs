using System;
using UnityEngine;

namespace PowerTools
{
	public class SpriteAnimNodes : MonoBehaviour
	{
		public static readonly int NUM_NODES = 10;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node0 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node1 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node2 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node3 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node4 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node5 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node6 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node7 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node8 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node9 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private float m_ang0;

		[SerializeField]
		[HideInInspector]
		private float m_ang1;

		[SerializeField]
		[HideInInspector]
		private float m_ang2;

		[SerializeField]
		[HideInInspector]
		private float m_ang3;

		[SerializeField]
		[HideInInspector]
		private float m_ang4;

		[SerializeField]
		[HideInInspector]
		private float m_ang5;

		[SerializeField]
		[HideInInspector]
		private float m_ang6;

		[SerializeField]
		[HideInInspector]
		private float m_ang7;

		[SerializeField]
		[HideInInspector]
		private float m_ang8;

		[SerializeField]
		[HideInInspector]
		private float m_ang9;

		private SpriteRenderer m_spriteRenderer;

		public Vector3 GetPosition(int nodeId, bool ignoredPivot = false)
		{
			if (this.m_spriteRenderer == null)
			{
				this.m_spriteRenderer = base.GetComponent<SpriteRenderer>();
			}
			if (this.m_spriteRenderer == null || this.m_spriteRenderer.sprite == null)
			{
				return Vector2.zero;
			}
			Vector3 vector = this.GetLocalPosition(nodeId, ignoredPivot);
			vector = base.transform.rotation * vector;
			vector.Scale(base.transform.lossyScale);
			return vector + base.transform.position;
		}

		public Vector3 GetLocalPosition(int nodeId, bool ignoredPivot = false)
		{
			if (this.m_spriteRenderer == null)
			{
				this.m_spriteRenderer = base.GetComponent<SpriteRenderer>();
			}
			if (this.m_spriteRenderer == null || this.m_spriteRenderer.sprite == null)
			{
				return Vector2.zero;
			}
			Vector3 vector = this.GetPositionRaw(nodeId);
			vector.y = -vector.y;
			if (ignoredPivot)
			{
				vector += (Vector3) this.m_spriteRenderer.sprite.rect.size * 0.5f - (Vector3) this.m_spriteRenderer.sprite.pivot;
			}
			vector *= 1f / this.m_spriteRenderer.sprite.pixelsPerUnit;
			if (this.m_spriteRenderer.flipX)
			{
				vector.x = -vector.x;
			}
			if (this.m_spriteRenderer.flipY)
			{
				vector.y = -vector.y;
			}
			return vector;
		}

		public float GetAngle(int nodeId)
		{
			float angleRaw = this.GetAngleRaw(nodeId);
			if (this.m_spriteRenderer == null)
			{
				this.m_spriteRenderer = base.GetComponent<SpriteRenderer>();
			}
			if (this.m_spriteRenderer == null || this.m_spriteRenderer.sprite == null)
			{
				return 0f;
			}
			return angleRaw + base.transform.eulerAngles.z;
		}

		public Vector2 GetPositionRaw(int nodeId)
		{
			switch (nodeId)
			{
			case 0:
				return this.m_node0;
			case 1:
				return this.m_node1;
			case 2:
				return this.m_node2;
			case 3:
				return this.m_node3;
			case 4:
				return this.m_node4;
			case 5:
				return this.m_node5;
			case 6:
				return this.m_node6;
			case 7:
				return this.m_node7;
			case 8:
				return this.m_node8;
			case 9:
				return this.m_node9;
			default:
				return Vector2.zero;
			}
		}

		public float GetAngleRaw(int nodeId)
		{
			switch (nodeId)
			{
			case 0:
				return this.m_ang0;
			case 1:
				return this.m_ang1;
			case 2:
				return this.m_ang2;
			case 3:
				return this.m_ang3;
			case 4:
				return this.m_ang4;
			case 5:
				return this.m_ang5;
			case 6:
				return this.m_ang6;
			case 7:
				return this.m_ang7;
			case 8:
				return this.m_ang8;
			case 9:
				return this.m_ang9;
			default:
				return 0f;
			}
		}

		public void Reset()
		{
			this.m_node0 = Vector2.zero;
			this.m_node1 = Vector2.zero;
			this.m_node2 = Vector2.zero;
			this.m_node3 = Vector2.zero;
			this.m_node4 = Vector2.zero;
			this.m_node5 = Vector2.zero;
			this.m_node6 = Vector2.zero;
			this.m_node7 = Vector2.zero;
			this.m_node8 = Vector2.zero;
			this.m_node9 = Vector2.zero;
			this.m_ang0 = 0f;
			this.m_ang1 = 0f;
			this.m_ang2 = 0f;
			this.m_ang3 = 0f;
			this.m_ang4 = 0f;
			this.m_ang5 = 0f;
			this.m_ang6 = 0f;
			this.m_ang7 = 0f;
			this.m_ang8 = 0f;
			this.m_ang9 = 0f;
		}
	}
}
