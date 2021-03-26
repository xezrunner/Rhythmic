using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DebugMenuEntry
{
    public DebugMenuEntry(string text = "")
    {
        Text = text; IsSelectable = false; Color = Colors.Unimportant;
        ID = 0; Variable = null; Extra = null; HelpText = null; Function = null; IsEnabled = true;
    }
    public DebugMenuEntry(string text = "", object variable = null, string extra = null, string helpText = null, Action function = null, bool isEnabled = true)
    {
        ID = 0; Text = text; IsSelectable = true;

        Variable = (variable == null) ? null : new Ref<object>(() => variable, value => { variable = value; });
        Extra = extra; HelpText = helpText;
        Function = function;

        Color = Colors.Default;
        IsEnabled = isEnabled;
    }

    public int ID; // -1 is invalid
    public string Text;
    public bool IsSelectable;
    public bool IsEnabled;
    public Ref<object> Variable;
    public string Extra;
    public string HelpText;
    public Action Function; // TODO: naming?
    public Color Color;
}

[AttributeUsage(AttributeTargets.Class)]
public class DebugMenuComponentAttribute : Attribute
{
    /// <param name="updateMode">None will never update.<br/>Faster is ~250ms.</param>
    /// <param name="hasMainMenuShortcut">Whether to put a Main menu... button as the -1th top entry.</param>
    public DebugMenuComponentAttribute(float updateMs, bool hasMainMenuShortcut = true)
    {
        UpdateFrequencyInMs = updateMs;
        HasMainMenuShortcut = hasMainMenuShortcut;
    }
    public DebugMenuComponentAttribute(bool hasMainMenuShortcut = true) { HasMainMenuShortcut = hasMainMenuShortcut; }

    public float UpdateFrequencyInMs = -1; // -1: disabled | 0: every frame in Update()
    public bool HasMainMenuShortcut = true;
}

public class DebugMenuComponent
{
    public DebugMenuComponentAttribute Attribute { get { return (DebugMenuComponentAttribute)System.Attribute.GetCustomAttribute(GetType(), typeof(DebugMenuComponentAttribute)); } }

    public string Name;

    Dictionary<int, DebugMenuEntry> Entries;
    int entry_count = 0;

    public static DebugMenuEntry MainMenuShortcut = new DebugMenuEntry("Main menu...", function: DebugMenu.MainMenu);

    public virtual void Init()
    {
        /* This has to be overriden in each component to add lines.*/

        if (Entries == null) Entries = new Dictionary<int, DebugMenuEntry>();

        // Add 'Main menu...' shortcut if required (-1)
        if (Attribute.HasMainMenuShortcut && !Entries.ContainsKey(0))
            AddEntry(MainMenuShortcut, -1);
    }

    public void AddEntry(DebugMenuEntry entry, int? force_id = null)
    {
        int id = (force_id.HasValue) ? force_id.Value : entry_count;

        if (!Entries.ContainsValue(entry))
        {
            entry.ID = id; // Assign ID automatically for each entry.
            Entries.Add(id, entry);
            if (!force_id.HasValue) entry_count++;
        }
    }
    public void AddEntry(string text = "") => AddEntry(new DebugMenuEntry(text)); // Separator
    public void AddEntry(string text, Action function, object variable = null, string extra = null, string helpText = null, bool isEnabled = true) =>
        AddEntry(new DebugMenuEntry(text, variable, extra, helpText, function, isEnabled));

    public string Text; // Main text of the component

    public string Main()
    {
        if (Entries == null || Entries.Count < 1)
        { Logger.LogMethodE($"[{Name}] - Entries is null! This is bad!", this); }

        // Build string:
        string s = "";

        foreach (KeyValuePair<int, DebugMenuEntry> entry in Entries)
            s += entry.Value.Text + (entry.Key < entry_count - 1 ? "\n" : ""); // Add newline after each entry except last

        return s;
    }
}

/// <summary>This helps us hold references to variables.</summary>
public class Ref<T>
{
    Func<T> _get;
    Action<T> _set;

    public Ref(Func<T> get, Action<T> set)
    {
        _get = get;
        _set = set;
    }
    public T Value
    {
        get
        {
            T result = _get();
            if (result == null) Logger.LogMethodE("A Ref<T> had no value! This is bad!");

            return _get();
        }
        set { _set(value); }
    }
}