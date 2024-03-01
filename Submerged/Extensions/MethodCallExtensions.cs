using System;
using UnityEngine.Events;

namespace Submerged.Extensions;

public static class MethodCallExtensions
{
    public static void AddListener(this UnityEvent self, Action listener) => self.AddListener(listener);
}
