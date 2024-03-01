using System;
using System.Collections.Generic;
using System.Reflection;
using Il2CppInterop.Runtime;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Submerged.Extensions;

public static class ComponentExtensions
{
    private static Dictionary<string, Type> _registeredTypes;

    private static Dictionary<string, Type> RegisteredTypes
    {
        get
        {
            if (_registeredTypes != null) return _registeredTypes;
            _registeredTypes = new Dictionary<string, Type>();

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                RegisterInIl2CppAttribute attribute = type.GetCustomAttribute<RegisterInIl2CppAttribute>();

                if (attribute != null)
                {
                    _registeredTypes[type.Name] = type;
                }
            }

            return _registeredTypes;
        }
    }

    public static T EnsureComponent<T>(this GameObject obj) where T : Component => obj.GetComponent<T>() ?? obj.AddComponent<T>();

    [Obsolete("This should eventualy disappear from the codebase.")]
    public static Component AddInjectedComponentByName(this GameObject obj, string typeName) => obj.AddComponent(Il2CppType.From(RegisteredTypes[typeName]));
}
