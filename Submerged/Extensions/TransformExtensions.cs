using UnityEngine;

namespace Submerged.Extensions;

public static class TransformExtensions
{
    public static void SetZPos(this Transform transform, float z)
    {
        Vector3 vector = transform.position;
        vector.z = z;
        transform.position = vector;
    }

    public static void SetZLocalPos(this Transform transform, float z)
    {
        Vector3 vector = transform.localPosition;
        vector.z = z;
        transform.localPosition = vector;
    }

    public static void SetXScale(this Transform transform, float x)
    {
        Vector3 vector = transform.localScale;
        vector.x = x;
        transform.localScale = vector;
    }

    public static Transform[] GetChildren(this Transform transform)
    {
        int childCount = transform.childCount;
        Transform[] children = new Transform[childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            children[i] = transform.GetChild(i);
        }

        return children;
    }
}
