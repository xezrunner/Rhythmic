using System;
using System.Collections.Generic;
using UnityEngine;

public enum DebugComponentType { Component = 0, Prefab = 1 }

[AttributeUsage(AttributeTargets.Class)]
public class DebugComponentAttribute : Attribute
{
    public DebugComponentAttribute(DebugControllerState state, DebugComponentType comType = DebugComponentType.Prefab) { State = state; ComponentType = comType; }
    public DebugComponentAttribute(DebugControllerState state, Type type, DebugComponentType comType = DebugComponentType.Component) { State = state; Type = type; ComponentType = comType; }

    public DebugControllerState State;
    public DebugComponentType ComponentType;
    public Type Type;
}

public class DebugComponent : MonoBehaviour
{
    public DebugComponent _Instance; // TODO: might not need

    //DebugController DebugController { get { return DebugController.Instance; } }

    public void RemoveDebugComponent(DebugComponentType comType)
    {
        // Remove the component's GameObject if it's a Prefab
        if (comType == DebugComponentType.Prefab)
            Destroy(gameObject); // Destroy the entire GameObject
        else
            Destroy(this); // Destroy the component only
    }

    public static void HandleState(DebugControllerState State, Type t)
    {
        DebugComponentAttribute attr = (DebugComponentAttribute)Attribute.GetCustomAttribute(t, typeof(DebugComponentAttribute));
        Logger.LogMethod(attr.State.ToString(), "DebugComponent");
    }
}