using System;
using System.Collections.Generic;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Submerged.Floors.Objects;

[RegisterInIl2Cpp]
public class ComponentClone(nint ptr) : MonoBehaviour(ptr)
{
    #region Template stuff

    public interface ITemplate<T> : ITemplate where T : Component
    {
        new T AddComponentTo(GameObject target);
        void CopyComponent(T source, T target);

        Component ITemplate.AddComponentTo(GameObject target) => AddComponentTo(target);
        void ITemplate.CopyComponent(Component source, Component target) => CopyComponent(source.Cast<T>(), target.Cast<T>());
    }

    public interface ITemplate
    {
        string Purpose { get; }
        Component AddComponentTo(GameObject target);
        void CopyComponent(Component source, Component target);
    }

    public static void RegisterTemplate<T>(ITemplate<T> template) where T : Component
    {
        if (_templates.TryGetValue((typeof(T), template.Purpose), out ITemplate existing))
        {
            Warning($"A template for {typeof(T).Name} has already been registered with the purpose {existing.Purpose}, overwriting it!!!");
        }
        _templates[(typeof(T), template.Purpose)] = template;
    }

    #endregion

    private static readonly Dictionary<(Type, string), ITemplate> _templates = [];

    public Type componentType;
    public string purpose;
    public Component source;
    public Component target;

    private void Start()
    {
        if (!source)
        {
            Destroy(this);
            throw new NullReferenceException($"{nameof(ComponentClone)}.{nameof(source)} must be set as soon as possible after instantiation. (Object name: {name})");
        }

        componentType = source.GetIl2CppType().ToSystemType();

        if (_templates.TryGetValue((componentType, purpose), out ITemplate template))
        {
            target = template.AddComponentTo(gameObject);
            template.CopyComponent(source, target);
        }
        else
        {
            Destroy(this);
            throw new InvalidOperationException($"No template found for {componentType.Name} with purpose {purpose} (Object name: {name})");
        }
    }

    private void LateUpdate()
    {
        if (!source)
        {
            if (target) Destroy(target);
            Destroy(this);
            return;
        }

        _templates[(componentType, purpose)].CopyComponent(source, target);
    }

    public static void CloneIfPossible(Component source, GameObject targetObject, string purpose)
    {
        if (_templates.ContainsKey((source.GetIl2CppType().ToSystemType(), purpose)))
        {
            ComponentClone clone = targetObject.AddComponent<ComponentClone>();
            clone.source = source;
            clone.purpose = purpose;
        }
    }
}
