using System;
using System.Collections.Generic;
using UnityEngine;

public enum DebugComponentType { Component = 0, Prefab = 1 }

[AttributeUsage(AttributeTargets.Class)]
public class DebugComponentAttribute : Attribute
{
    public DebugComponentAttribute(DebugComponentFlag debugFlag, DebugComponentType comType, DebugUI_SelectionUpdateMode selectionUpdateMode, string prefabPath = "")
    { DebugFlag = debugFlag; ComponentType = comType; DebugUI_SelectionUpdateMode = selectionUpdateMode; PrefabPath = prefabPath; }
    public DebugComponentAttribute(DebugComponentFlag debugFlag, DebugComponentType comType, string prefabPath = "")
    { DebugFlag = debugFlag; ComponentType = comType; DebugUI_SelectionUpdateMode = DebugUI_SelectionUpdateMode.Once; PrefabPath = prefabPath; }

    public DebugComponentFlag DebugFlag;
    public DebugComponentType ComponentType;
    public DebugUI_SelectionUpdateMode DebugUI_SelectionUpdateMode;
    public string PrefabPath;
}

public class DebugComponent : MonoBehaviour
{
    DebugUI DebugUI { get { return DebugUI.Instance; } }

    public DebugComponentAttribute Attribute { get { return (DebugComponentAttribute)System.Attribute.GetCustomAttribute(GetType(), typeof(DebugComponentAttribute)); } }

    [Header("Container")]
    public GameObject SelfParent;
    public void RemoveComponent()
    {
        if (Attribute.ComponentType == DebugComponentType.Component)
        {
            // Destroy SelfParent container if exists. Otherwise, only destroy this component.
            if (SelfParent) Destroy(SelfParent);
            else Destroy(this);
        }
        else
            Destroy(gameObject); // Destory the entire gameObject in case we're a prefab.
    }

    DebugUI_SelectionUpdateMode? _selectionUpdateMode;
    public DebugUI_SelectionUpdateMode SelectionUpdateMode
    {
        get
        {
            if (!_selectionUpdateMode.HasValue)
                _selectionUpdateMode = Attribute.DebugUI_SelectionUpdateMode;

            return _selectionUpdateMode.Value;
        }
    }

    // Selection
    // TODO: Add selection logic!
    bool selectionUpdated = false;
    public List<DebugUI_SelectionLine> Lines = new List<DebugUI_SelectionLine>();

    string AddLine(string line = "", bool isSelectable = false) => AddLine(line, 1, isSelectable);
    string AddLine(string line = "", int linesToAdd = 1, bool isSelectable = false, object selectionTag = null)
    {
        DebugUI_Text += $"{line}";

        // Add newlines:
        for (int i = 0; i < linesToAdd; i++)
            DebugUI_Text += '\n';

        // Add selection line:
        if (isSelectable)
        {
            if (SelectionUpdateMode == DebugUI_SelectionUpdateMode.Always || (SelectionUpdateMode < DebugUI_SelectionUpdateMode.Always && !selectionUpdated))
            {
                DebugUI_SelectionLine selectionLine = new DebugUI_SelectionLine(DebugUI_Text.Length - line.Length - linesToAdd, DebugUI_Text.Length, selectionTag);
                Lines.Add(selectionLine);
            }
        }

        return DebugUI_Text;
    }

    // TODO: components should emit thier own individual texts.
    // This means that in case we want to have multiple components writing at once, DebugUI should be able to grab all of them.
    string _debugUI_Text;
    public string DebugUI_Text
    {
        get { return _debugUI_Text; }
        set
        {
            _debugUI_Text = value;
            DebugUI.Text = value;
        }
    }
}