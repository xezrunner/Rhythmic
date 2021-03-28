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

public partial class DebugController
{
    public List<MetaDebugComponent> MetaComponents
    {
        get
        {
            return new List<MetaDebugComponent>()
            {
                new MetaDebugComponent(typeof(DebugUI), DebugUI.Instance),
                new MetaDebugComponent(typeof(DebugKeys), DebugKeys.Instance.Component),
                new MetaDebugComponent(typeof(DebugMenu), DebugMenu.Instance.Component),
                new MetaDebugComponent(typeof(DebugStats), DebugStats.Instance.Component),
                new MetaDebugComponent(typeof(SelectionComponentTest), SelectionComponentTest.Instance.Component)
            };
        }
    }
}