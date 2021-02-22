using System;
using System.Collections.Generic;

public struct MetaDebugComponent
{
    public MetaDebugComponent(Type type, DebugComponent instance) { Type = type; Instance = instance; ObjectInstance = null; }
    public MetaDebugComponent(Type type, DebugComponent instance, object obj_instance) { Type = type; Instance = instance; ObjectInstance = obj_instance; }

    public DebugComponentAttribute Attribute
    {
        get { return (DebugComponentAttribute)System.Attribute.GetCustomAttribute(Type, typeof(DebugComponentAttribute)); }
    }

    public Type Type;
    public DebugComponent Instance;
    public object ObjectInstance; // This is the literal object hosting the component. In case of Prefab comtypes, it's the object instance itself
}

public static class DebugComponents
{
    // This needs to be a method to always give the latest result.
    /// <summary>Get the list of debug components' metadata that can be used.</summary>
    public static List<MetaDebugComponent> GetMetaComponents()
    {
        return new List<MetaDebugComponent>()
        {
            new MetaDebugComponent(typeof(DebugUI), DebugUI.Instance, DebugUI.ObjectInstance),
            new MetaDebugComponent(typeof(DebugKeys), DebugKeys.Instance),
            new MetaDebugComponent(typeof(DebugStats), DebugStats.Instance)
        };
    }
}