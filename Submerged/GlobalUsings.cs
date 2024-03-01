// ReSharper disable all
#pragma warning disable CS0618

extern alias JetBrains;

global using static Reactor.Utilities.Logger<Submerged.SubmergedPlugin>;

global using IEnumerator = System.Collections.IEnumerator;
global using CppIEnumerator = Il2CppSystem.Collections.IEnumerator;

global using SCG = System.Collections.Generic;
global using ICG = Il2CppSystem.Collections.Generic;

global using IntPtr = System.IntPtr;

global using Object = Submerged.Object; // hot take but i don't want to see this anywhere in the codebase
global using CppObject = Il2CppSystem.Object;
global using UnityObject = UnityEngine.Object;

global using SystemRandom = System.Random;
global using UnityRandom = UnityEngine.Random;

global using UsedImplicitly = JetBrains::JetBrains.Annotations.UsedImplicitlyAttribute;

global using nint = System.IntPtr; // nint is already a keyword for IntPtr but it was introduced very recently and some IDEs don't support it

namespace Submerged
{
    [System.Obsolete("Use 'object', 'CppObject' or 'UnityObject' instead")]
    internal sealed class Object { }
}
