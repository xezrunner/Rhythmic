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

public struct DebugComSelectionLine // Ambiguity with DebugLine
{
    public DebugComSelectionLine(int start, int end, object tag) { startIndex = start; endIndex = end; Tag = tag; Color = Color.white; SelectedColor = Colors.Application; }
    public DebugComSelectionLine(int start, int end, object tag, Color color) { startIndex = start; endIndex = end; Tag = tag; Color = color; SelectedColor = Colors.Application; }
    public DebugComSelectionLine(int start, int end, object tag, Color color, Color selectedColor) { startIndex = start; endIndex = end; Tag = tag; Color = color; SelectedColor = selectedColor; }

    public int startIndex;
    public int endIndex;
    public object Tag;

    public Color Color;
    public Color SelectedColor;
}

public enum DebugComponentType { Component = 0, Prefab = 1 }
public enum DebugComSelectionUpdateMode { Once = 0, ManualFlagging = 1, Always = 2 }
public enum DebugComTextMode { Clear = 0, Additive = 1 }

[AttributeUsage(AttributeTargets.Class)]
public class DebugComponentAttribute : Attribute
{
    public DebugComponentAttribute(DebugComponentFlag debugFlag, DebugComponentType comType, string prefabPath, float updateMs = -1, DebugComSelectionUpdateMode selectionUpdateMode = DebugComSelectionUpdateMode.ManualFlagging)
    { DebugFlag = debugFlag; ComponentType = comType; SelectionUpdateMode = selectionUpdateMode; TextMode = DebugComTextMode.Clear; UpdateFrequencyInMs = updateMs; PrefabPath = prefabPath; }
    public DebugComponentAttribute(DebugComponentFlag debugFlag, DebugComponentType comType, float updateMs = -1, DebugComSelectionUpdateMode selectionUpdateMode = DebugComSelectionUpdateMode.ManualFlagging, string prefabPath = "")
    { DebugFlag = debugFlag; ComponentType = comType; SelectionUpdateMode = selectionUpdateMode; TextMode = DebugComTextMode.Clear; UpdateFrequencyInMs = updateMs; PrefabPath = prefabPath; }
    public DebugComponentAttribute(DebugComponentFlag debugFlag, DebugComponentType comType, float updateMs, DebugComTextMode textMode, DebugComSelectionUpdateMode selectionUpdateMode = DebugComSelectionUpdateMode.ManualFlagging, string prefabPath = "")
    { DebugFlag = debugFlag; ComponentType = comType; SelectionUpdateMode = selectionUpdateMode; TextMode = textMode; UpdateFrequencyInMs = updateMs; PrefabPath = prefabPath; }
    /// <param name="additiveMaxLines">We'll assume you want Additive text mode.</param>
    public DebugComponentAttribute(DebugComponentFlag debugFlag, DebugComponentType comType, float updateMs, int additiveMaxLines, string prefabPath = "")
    { DebugFlag = debugFlag; ComponentType = comType; SelectionUpdateMode = DebugComSelectionUpdateMode.ManualFlagging; TextMode = DebugComTextMode.Additive; AdditiveMaxLines = additiveMaxLines; UpdateFrequencyInMs = updateMs; PrefabPath = prefabPath; }

    public DebugComponentFlag DebugFlag;
    public DebugComponentType ComponentType;
    public DebugComSelectionUpdateMode SelectionUpdateMode;
    public DebugComTextMode TextMode;

    public int AdditiveMaxLines = -1;
    public float UpdateFrequencyInMs = -1; // -1: disabled | 0: every frame in Update()
    public string PrefabPath = null;
}

public class DebugComponent : MonoBehaviour
{
    //public RefDebugComInstance Instances;
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
    public string Text { get; set; }
    public void ClearText() => Text = "";

    public string AddLine(string line = "", int linesToAdd = 1, bool isSelectable = false, object selectionTag = null)
    {
        if (!IsUIComponent) IsUIComponent = true;

        Text += $"{line}";

        // Add newlines:
        for (int i = 0; i < linesToAdd; i++)
            Text += '\n';

        // Max line count:
        if (Attribute.AdditiveMaxLines > 0)
        {
            string[] lines = Text.Split('\n');
            int lineCount = lines.Length - 1;
            if (lineCount > Attribute.AdditiveMaxLines)
            {
                int lineDiff = Mathf.Abs(Attribute.AdditiveMaxLines - lineCount);
                string[] newLines = new string[Attribute.AdditiveMaxLines + 1]; // newline at end!
                for (int i = lineDiff; i < lineCount; i++) // Remove lines from start to keep max line count
                    newLines[i - lineDiff] = lines[i];

                Text = string.Join("\n", newLines);
            }
        }

        // Add selection line:
        if (isSelectable && SelectionLines.Where(i => i.Tag == selectionTag) != null) // TODO: performance!
        {
            if (SelectionUpdateMode == DebugComSelectionUpdateMode.Always || (SelectionUpdateMode < DebugComSelectionUpdateMode.Always && !selectionUpdated))
            {
                DebugComSelectionLine selectionLine = new DebugComSelectionLine(Text.Length - line.Length - linesToAdd, Text.Length, selectionTag);
                SelectionLines.Add(selectionLine);
            }
        }

        return Text;
    }
    public string AddLine(string line = "", bool isSelectable = false) => AddLine(line, 1, isSelectable);
    public string AddLine(string line = "") => AddLine(line, false);
    public string AddLine() => AddLine("");

    // Selection:
    DebugComSelectionUpdateMode? _selectionUpdateMode;
    public DebugComSelectionUpdateMode SelectionUpdateMode
    {
        get
        {
            if (!_selectionUpdateMode.HasValue)
                _selectionUpdateMode = Attribute.SelectionUpdateMode;

            return _selectionUpdateMode.Value;
        }
    }

    bool selectionUpdated = false;
    public List<DebugComSelectionLine> SelectionLines = new List<DebugComSelectionLine>();
}