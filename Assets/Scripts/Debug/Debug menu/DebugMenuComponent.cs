using DebugMenus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct DebugMenuEntry
{
    // TODO: Easier separator(s)!
    // Perhaps even by AddLine(), such as AddSeparator(SeparatorType)
    public DebugMenuEntry(string text = "", bool isSelectable = true)
    {
        Text = text; IsSelectable = (isSelectable ? (text != "") : false); Color = (isSelectable) ? Colors.Info : Colors.Unimportant;
        ID = 0; Variable = null; Page = null; Extra = null; HelpText = null; Function = null; IsEnabled = true; CloseMenuOnFunction = false;
    }
    public DebugMenuEntry(string text, Type page)
    {
        ID = 0; Text = text; IsSelectable = true;

        Variable = null;
        Page = page;
        Extra = null; HelpText = null;
        Function = null;
        CloseMenuOnFunction = false;
        Color = Colors.Default;
        IsEnabled = true;
    }
    public DebugMenuEntry(string text, Ref variable, string extra = null, string helpText = null, bool closeMenuOnFunction = false, bool isEnabled = true)
    {
        ID = 0; Text = text; IsSelectable = true;

        Variable = variable;
        Page = null;
        Extra = extra; HelpText = helpText;
        Function = null;
        CloseMenuOnFunction = closeMenuOnFunction;

        Color = Colors.Default;
        IsEnabled = isEnabled;
    }
    public DebugMenuEntry(string text, Func<object> variable, string extra = null, string helpText = null, bool closeMenuOnFunction = false, bool isEnabled = true)
    {
        ID = 0; Text = text; IsSelectable = true;

        Variable = new Ref(() => variable(), (v) => { }); // Empty setter!
        Page = null;
        Extra = extra; HelpText = helpText;
        Function = null;
        CloseMenuOnFunction = closeMenuOnFunction;

        Color = Colors.Default;
        IsEnabled = isEnabled;
    }
    public DebugMenuEntry(string text, Action function, Ref variable, string extra = null, string helpText = null, bool closeMenuOnFunction = false, bool isEnabled = true)
    {
        ID = 0; Text = text; IsSelectable = true;

        Variable = variable;
        Page = null;
        Extra = extra; HelpText = helpText;
        Function = function;
        CloseMenuOnFunction = closeMenuOnFunction;

        Color = Colors.Default;
        IsEnabled = isEnabled;
    }
    public DebugMenuEntry(string text, Action function, Func<object> variable, string extra = null, string helpText = null, bool closeMenuOnFunction = false, bool isEnabled = true)
    {
        ID = 0; Text = text; IsSelectable = true;

        Variable = new Ref(() => variable(), (v) => { });
        Page = null;
        Extra = extra; HelpText = helpText;
        Function = function;
        CloseMenuOnFunction = closeMenuOnFunction;

        Color = Colors.Default;
        IsEnabled = isEnabled;
    }
    public DebugMenuEntry(string text, Action function, string extra = null, string helpText = null, bool closeMenuOnFunction = false, bool isEnabled = true)
    {
        ID = 0; Text = text; IsSelectable = true;

        Variable = null;
        Page = null;
        Extra = extra; HelpText = helpText;
        Function = function;
        CloseMenuOnFunction = closeMenuOnFunction;

        Color = Colors.Default;
        IsEnabled = isEnabled;
    }

    public int ID; // -1 is invalid
    public string Text;
    public bool IsSelectable;
    public bool IsEnabled;

    public Type Page;
    public Ref Variable;
    public string Extra;
    public string HelpText;
    public Action Function; // TODO: naming?
    public bool CloseMenuOnFunction;
    public Color Color;

    public void DoFunction() // TODO: naming!
    {
        if (CloseMenuOnFunction) DebugMenu.SetActive(false);
        if (Function != null) Function();
    }
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
    public DebugMenuComponentAttribute(bool hasMainMenuShortcut = true, float updateMs = 250) { HasMainMenuShortcut = hasMainMenuShortcut; UpdateFrequencyInMs = updateMs; }

    public float UpdateFrequencyInMs = 250; // -1: disabled | 0: every frame in Update()
    public bool HasMainMenuShortcut = true;
}

public class DebugMenuComponent
{
    DebugMenu DebugMenu { get { return (DebugMenu)DebugMenu.Instance; } }

    public DebugMenuComponentAttribute Attribute;

    public string Name { get { return GetType().Name; } }

    public Dictionary<int, DebugMenuEntry> Entries;
    int entry_count = 0;
    public int entry_index = 0; // Current entry index - set by DebugMenu

    public static DebugMenuEntry MainMenuShortcut = new DebugMenuEntry("Main menu...", DebugMenu.MainMenu);

    public virtual void Init()
    {
        /* This has to be overriden in each component to add lines.
         * You have to keep the base.Init() function when overriding! */

        // In case you did not need an Attribute, we'll add a default for you.
        var attr = (DebugMenuComponentAttribute)System.Attribute.GetCustomAttribute(GetType(), typeof(DebugMenuComponentAttribute));
        if (attr == null) attr = new DebugMenuComponentAttribute();
        Attribute = attr;

        if (Entries == null) Entries = new Dictionary<int, DebugMenuEntry>();

        // Add 'Main menu...' shortcut if required (-1 is Main menu)
        if (Attribute.HasMainMenuShortcut && (Entries.Count == 0 /*|| Entries.ElementAt(0).Value.ID != -1 <- probably not be needed*/))
            AddEntry(MainMenuShortcut, -1);

    }
    public void _Base_Init() => Init();

    public void DebugMenu_Close() => DebugMenu._SetActive(false);

    public Action<DebugMenuEntry> _GlobalEntryAction; // TEST: If this is set, all options will use this function instead.

    public static int var_spaces = 4;
    public static int var_text_length = 8;
    public static int extra_spaces = 2;
    public string Main(int selectionIndex = -1)
    {
        if (Entries == null || Entries.Count < 1) // No entries! Add a Main menu... shortcut and an info text
        {
            Logger.LogMethodE($"[{Name}] - Entries is null! This is bad!", this);
            AddEntry(MainMenuShortcut);
            string infoText = "* ".AddColor(Colors.Error) + $"Error: there are no entries on this page! Check the {"Init()".AddColor(Colors.Network)} function and add some entries!";
            var info = new DebugMenuEntry($"Component name: {GetType().Name}\n\n".AddColor(Colors.Application) + infoText, false) { Color = Colors.Error };
            AddEntry(info);
        }

        // Find alignment values (right>):
        int var_align_max = Entries.Max(kv => kv.Value.Text.Length) + var_spaces;
        int extra_align_max = (var_align_max + var_text_length) + extra_spaces;

        // Build string:
        string s = "";

        //foreach (KeyValuePair<int, DebugMenuEntry> kv in Entries)
        for (int i = 0; i < entry_count; i++)
        {
            KeyValuePair<int, DebugMenuEntry> kv = Entries.ElementAt(i);
            DebugMenuEntry entry = kv.Value;
            object variable = (entry.Variable != null) ? entry.Variable.Value : null;

            string s_entry = entry.Text; // Text

            string var = "";
            if (variable != null && variable.GetType() == typeof(bool))
                var = ((bool)variable ? "ON " : "OFF").AlignSpaces(s_entry.Length, var_align_max);
            else if (variable != null)
            {
                string var_string = variable.ToString();
                var = var_string.Substring(0, Mathf.Clamp(var_text_length, 0, var_string.Length)).AlignSpaces(s_entry.Length, var_align_max);
            }
            s_entry += var;

            string extra = "";
            if (entry.Extra != null && entry.Extra != "")
                extra = entry.Extra.AlignSpaces(s_entry.Length, extra_align_max).AddColor((selectionIndex == i && entry.IsSelectable) ? Colors.DebugMenuSelection : new Color(1, 1, 1, 0.45f));
            s_entry += extra;

            // Add selection color if index matches and selectable
            s_entry = s_entry.AddColor((selectionIndex == i && entry.IsSelectable) ? Colors.DebugMenuSelection : entry.Color);
            s_entry += (i < entry_count - 1 ? "\n" : ""); // Add newline after each entry except last
            s += s_entry;
        }

        return s;
    }

    public void AddEntry(DebugMenuEntry entry, int? force_id = null)
    {
        int id = (force_id.HasValue) ? force_id.Value : entry_count;

        // TODO: Performance?!
        //if (!Entries.ContainsValue(entry))
        //{
        entry.ID = id; // Assign ID automatically for each entry.
        Entries.Add(id, entry);
        entry_count++;
        //}
    }
    public void AddEntry(string text = "", bool isSelectable = true) => AddEntry(new DebugMenuEntry(text, isSelectable)); // Separator
    public void AddEntry(string text, Type page) => AddEntry(new DebugMenuEntry(text, page)); // Pages
    public void AddEntry(string text, Ref variable, string extra = null, string helpText = null, bool closeMenuOnFunction = false, bool isEnabled = true) =>
        AddEntry(new DebugMenuEntry(text, variable, extra, helpText, closeMenuOnFunction, isEnabled));
    public void AddEntry(string text, Func<object> variable, string extra = null, string helpText = null, bool closeMenuOnFunction = false, bool isEnabled = true) =>
        AddEntry(new DebugMenuEntry(text, variable, extra, helpText, closeMenuOnFunction, isEnabled));
    public void AddEntry(string text, Action function, string extra = null, string helpText = null, bool closeMenuOnFunction = false, bool isEnabled = true) =>
        AddEntry(new DebugMenuEntry(text, function, extra, helpText, closeMenuOnFunction, isEnabled));
    public void AddEntry(string text, Action function, Func<object> variable, string extra = null, string helpText = null, bool closeMenuOnFunction = false, bool isEnabled = true) =>
        AddEntry(new DebugMenuEntry(text, function, variable, extra, helpText, closeMenuOnFunction, isEnabled));

    public string Text; // Main text of the component
}

/// <summary>This helps us hold references to variables.</summary>
public class Ref
{
    Func<object> _get;
    Action<object> _set;

    public Ref(Func<object> get, Action<object> set)
    {
        _get = get;
        _set = set;
    }
    public object Value
    {
        get
        {
            object result = _get();
            if (result == null) Logger.LogMethodE("A Ref had no value! This is bad!", this);

            return _get();
        }
        set { _set(value); }
    }
}