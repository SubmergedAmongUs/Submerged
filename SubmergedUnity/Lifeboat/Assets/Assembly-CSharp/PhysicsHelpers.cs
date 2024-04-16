using System;
using UnityEngine;

public static class PhysicsHelpers
{
	private static Collider2D[] colliderHits = new Collider2D[20];

	private static RaycastHit2D[] castHits = new RaycastHit2D[20];

	private static Vector2 temp = default(Vector2);

	private static ContactFilter2D filter;

	public static bool CircleContains(Vector2 source, float radius, int layerMask)
	{
		return Physics2D.OverlapCircleNonAlloc(source, radius, PhysicsHelpers.colliderHits, layerMask) > 0;
	}

	public static bool AnyEdgeTriggerBetween(Vector2 source, Vector2 target, int layerMask)
	{
		PhysicsHelpers.filter.layerMask = layerMask;
		PhysicsHelpers.filter.useTriggers = true;
		PhysicsHelpers.temp.x = target.x - source.x;
		PhysicsHelpers.temp.y = target.y - source.y;
		int num = Physics2D.Raycast(source, PhysicsHelpers.temp, PhysicsHelpers.filter, PhysicsHelpers.castHits, PhysicsHelpers.temp.magnitude);
		for (int i = 0; i < num; i++)
		{
			if (PhysicsHelpers.castHits[i].collider.isTrigger)
			{
				return true;
			}
		}
		return false;
	}

	public static bool AnythingBetween(Vector2 source, Vector2 target, int layerMask, bool useTriggers)
	{
		PhysicsHelpers.filter.layerMask = layerMask;
		PhysicsHelpers.filter.useTriggers = useTriggers;
		PhysicsHelpers.temp.x = target.x - source.x;
		PhysicsHelpers.temp.y = target.y - source.y;
		return Physics2D.Raycast(source, PhysicsHelpers.temp, PhysicsHelpers.filter, PhysicsHelpers.castHits, PhysicsHelpers.temp.magnitude) > 0;
	}

	public static bool AnythingBetween(Collider2D castObject, Vector2 source, Vector2 target, int layerMask, bool useTriggers)
	{
		PhysicsHelpers.filter.layerMask = layerMask;
		PhysicsHelpers.filter.useTriggers = useTriggers;
		PhysicsHelpers.temp.x = target.x - source.x;
		PhysicsHelpers.temp.y = target.y - source.y;
		return castObject.Cast(PhysicsHelpers.temp, PhysicsHelpers.filter, PhysicsHelpers.castHits, PhysicsHelpers.temp.magnitude) > 0;
	}

	public static bool AnyNonTriggersBetween(Vector2 source, Vector2 dirNorm, float mag, int layerMask)
	{
		int num = Physics2D.RaycastNonAlloc(source, dirNorm, PhysicsHelpers.castHits, mag, layerMask);
		bool result = false;
		for (int i = 0; i < num; i++)
		{
			if (!PhysicsHelpers.castHits[i].collider.isTrigger)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public static bool AnythingBetween(Vector2 source, Vector2 target, int layerMask, bool useTriggers, Collider2D itemToIgnore, Transform objectToIgnore)
	{
		PhysicsHelpers.filter.layerMask = layerMask;
		PhysicsHelpers.filter.useTriggers = useTriggers;
		PhysicsHelpers.temp.x = target.x - source.x;
		PhysicsHelpers.temp.y = target.y - source.y;
		return Physics2D.Raycast(source, PhysicsHelpers.temp, PhysicsHelpers.filter, PhysicsHelpers.castHits, PhysicsHelpers.temp.magnitude) > 0 && PhysicsHelpers.castHits[0].collider != itemToIgnore && PhysicsHelpers.castHits[0].collider.transform != objectToIgnore;
	}

	// Note: this type is marked as 'beforefieldinit'.
	static PhysicsHelpers()
	{
		ContactFilter2D contactFilter2D = default(ContactFilter2D);
		contactFilter2D.useLayerMask = true;
		PhysicsHelpers.filter = contactFilter2D;
	}
}
