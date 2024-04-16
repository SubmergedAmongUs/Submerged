using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

public static class TransformSetZ {
    [MenuItem("CONTEXT/Transform/Set Z Position Based on Y Position")]
    public static void SetZPos(MenuCommand command)
    {
        Transform transform = (Transform) command.context;
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y / 1000f);
    }
}