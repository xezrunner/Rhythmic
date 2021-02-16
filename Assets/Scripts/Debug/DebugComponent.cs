using System;
using System.Collections.Generic;
using UnityEngine;

public enum DebugComponentType { Component = 0, Prefab = 1 }

public class DebugComponent : MonoBehaviour
{
    public DebugComponent _Instance; // This is a generic instance to the DebugComponent type | TODO: might not need

    //DebugController DebugController { get { return DebugController.Instance; } }

    public void RemoveDebugComponent(DebugComponentType comType)
    {
        // Remove the component's GameObject if it's a Prefab
        if (comType == DebugComponentType.Prefab)
            Destroy(gameObject); // Destroy the entire GameObject
        else
            Destroy(this); // Destroy the component only
    }

    public static List<KeyValuePair<DebugComAttribute, object>> Components
    {
        get
        {
            return new List<KeyValuePair<DebugComAttribute, object>>()
            {
                new KeyValuePair<DebugComAttribute, object>(DebugUI.Attribute, new object[] { DebugUI.Instance, DebugUI.Prefab })
            };
        }
    }

    public static void HandleState(DebugControllerState State, Type t)
    {
        DebugComAttribute attr = (DebugComAttribute)Attribute.GetCustomAttribute(t, typeof(DebugComAttribute));
        Logger.LogMethod(attr.State.ToString(), "DebugComponent");
    }
}