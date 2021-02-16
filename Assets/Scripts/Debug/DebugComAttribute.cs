using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[AttributeUsage(AttributeTargets.Class)]
public class DebugComAttribute : Attribute
{
    public DebugComAttribute(DebugControllerState state, DebugComponentType comType = DebugComponentType.Prefab) { State = state; ComponentType = comType;  }
    public DebugComAttribute(DebugControllerState state, Type type, DebugComponentType comType = DebugComponentType.Component) { State = state; Type = type; ComponentType = comType; }

    public DebugControllerState State;
    public DebugComponentType ComponentType;
    public Type Type;
}