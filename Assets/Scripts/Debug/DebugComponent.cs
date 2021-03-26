using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct RefDebugComInstance
{
    public RefDebugComInstance(DebugComponent instance) { Component = instance; ObjectInstance = null; }
    public RefDebugComInstance(DebugComponent instance, object objInstance) { Component = instance; ObjectInstance = objInstance; }

    public DebugComponent Component;
    public object ObjectInstance;
}

public enum DebugComponentType { Component = 0, Prefab = 1 }
public enum DebugComTextMode { Clear = 0, Additive = 1 }

[AttributeUsage(AttributeTargets.Class)]
public class DebugComponentAttribute : Attribute
{
    public DebugComponentAttribute(DebugComponentFlag debugFlag, DebugComponentType comType, bool _internal_nohandling)
    { DebugFlag = debugFlag; ComponentType = comType; _Internal_NoHandleComponent = _internal_nohandling; }
    public DebugComponentAttribute(DebugComponentFlag debugFlag, DebugComponentType comType, string prefabPath, float updateMs = -1)
    { DebugFlag = debugFlag; ComponentType = comType; TextMode = DebugComTextMode.Clear; UpdateFrequencyInMs = updateMs; PrefabPath = prefabPath; }
    public DebugComponentAttribute(DebugComponentFlag debugFlag, DebugComponentType comType, float updateMs = -1, string prefabPath = "")
    { DebugFlag = debugFlag; ComponentType = comType; TextMode = DebugComTextMode.Clear; UpdateFrequencyInMs = updateMs; PrefabPath = prefabPath; }
    public DebugComponentAttribute(DebugComponentFlag debugFlag, DebugComponentType comType, float updateMs, DebugComTextMode textMode, string prefabPath = "")
    { DebugFlag = debugFlag; ComponentType = comType; TextMode = textMode; UpdateFrequencyInMs = updateMs; PrefabPath = prefabPath; }
    /// <param name="additiveMaxLines">We'll assume you want Additive text mode.</param>
    public DebugComponentAttribute(DebugComponentFlag debugFlag, DebugComponentType comType, float updateMs, int additiveMaxLines, string prefabPath = "")
    { DebugFlag = debugFlag; ComponentType = comType; TextMode = DebugComTextMode.Additive; AdditiveMaxLines = additiveMaxLines; UpdateFrequencyInMs = updateMs; PrefabPath = prefabPath; }

    public bool _Internal_NoHandleComponent;
    public DebugComponentFlag DebugFlag;
    public DebugComponentType ComponentType;
    public DebugComTextMode TextMode;

    public int AdditiveMaxLines = -1;
    public float UpdateFrequencyInMs = -1; // -1: disabled | 0: every frame in Update()
    public string PrefabPath = null;
}

public class DebugComponent : MonoBehaviour
{
    public string Name { get { return GetType().Name; } }
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

    // Determines whether the component is an UI component based on whether it has called AddLine() at least once.
    public bool IsUIComponent = false;

    public virtual void UI_Main() { }
    // TODO: components should emit thier own individual texts.
    // This means that in case we want to have multiple components writing at once, DebugUI should be able to grab all of them.
    // For now, only one component can change the main DebugUI text.
    public string Text;
    public void ClearText() => Text = "";

    public string AddLine(string line = "", int linesToAdd = 1, bool isSelectable = false, object selectionTag = null)
    {
        if (!IsUIComponent) IsUIComponent = true;

        Text += $"{line}";

        // Add newlines:
        // TODO: We may want to change the linesToAdd param to behave as extra lines rather than the total amount of lines to add.
        // This would reduce the confusion of having to type '2' as a param when we want to separate entries.
        // TODO: Possibly even add both? (linesToAddAfter, linesToAddBefore?)

        // TODO: we may want a Separator() / AddSeparator() function to add extra space? (although AddLine() would be just fine... hmm!)
        for (int i = 0; i < Mathf.Abs(linesToAdd); i++)
        {
            if (linesToAdd > 0) Text += '\n';
            else
            {
                Text = Text.Insert(Text.Length - line.Length, "\n");
                Text += '\n'; // Add one newline at the end of the string regardless of going negative.
            }
        }

        // Max line count:
        if (Attribute.AdditiveMaxLines > 0)
            Text = Text.MaxLines(Attribute.AdditiveMaxLines);

        return Text;
    }
    public string AddLine(string line = "", bool isSelectable = false) => AddLine(line, 1, isSelectable);
    public string AddLine(string line = "") => AddLine(line, false);
    public string AddLine() => AddLine("");
}