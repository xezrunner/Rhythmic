using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum DebugMenuHistoryDir { Backwards, Forwards, MainMenu = -1 }
public enum DebugMenuEntryDir { Up, Home, Down, End }
public enum DebugMenuVarDir { Decrease, Increase, Action }

[DebugComponent(DebugComponentFlag.DebugMenu, DebugComponentType.Component, true)]
public class DebugMenu : DebugComponent
{
    DebugUI DebugUI;

    TextMeshProUGUI debugmenuText;
    public Image debugmenuArrow;

    public static RefDebugComInstance Instance;

    Dictionary<string, DebugMenuComponent> Instances = new Dictionary<string, DebugMenuComponent>();
    DebugMenuComponent ActiveComponent = null;
    DebugMenuEntry SelectedEntry;

    public bool IsActive;
    public static void SetActive(bool value) // TODO: static is hacky!
    {
        DebugMenu debugMenu = (DebugMenu)Instance.Component;
        if (!debugMenu) Logger.LogMethodW("Debug menu has no instance!", "DebugMenu");

        debugMenu._SetActive(value);
    }
    public void _SetActive(bool value)
    {
        // TODO: disable player input during active!
        // Also invincibility, pausing (controllable)

        IsActive = value;
        debugmenuArrow.gameObject.SetActive(value);
        debugmenuText.gameObject.SetActive(value);

        if (IsActive) StartCoroutine(_Startup_EntryMoveCoroutine());
    }

    // TODO: This hack is required as TMP apparently has no line count when the component has just started.
    // This also fixes the wrong cursor position bug when starting at index 0.
    IEnumerator _Startup_EntryMoveCoroutine()
    {
        // Wait for the line count to populate before moving to the last known position.
        while (debugmenuText.textInfo.lineCount == 0)
            yield return null;

        Entry_Move(ActiveComponent.entry_index);
    }

    void Awake()
    {
        Instance = new RefDebugComInstance(this);

        DebugUI = DebugUI.Instance;
        debugmenuText = DebugUI.debugmenuText;
        debugmenuArrow = DebugUI.debugmenuArrow;
    }
    void Start()
    {
        if (!DebugUI)
        { Logger.LogMethodW("DebugUI instance doesn't seem to exist!", this); return; }

        SetActive(false); // Start disabled.
        MainMenu();
    }

    // There's always one component that's considered as the Main menu.
    // It can be switched to a different one, but there has to be one.
    public static void MainMenu()
    {
        DebugMenu debugMenu = (DebugMenu)Instance.Component;
        if (!debugMenu) Logger.LogMethodW("Debug menu has no instance!", "DebugMenu");
        debugMenu.SwitchToComponent(typeof(DebugMenus.MainMenu));

        // TODO: Navigation history - 20 limit!
        //debugMenu.navigation_history.Clear(); // Clear navigation history
    }

    // Component switching & handling:
    // TODO: Components need to have their roaming entry indexes for navigation purposes!
    void Handle_ActiveComponent(int index = 0) // Prints the active menu's text
    {
        if (ActiveComponent == null)
        { debugmenuText.text = ""; return; }

        //if (_isActive)
        debugmenuText.text = ActiveComponent.Main(index);
    }
    public void SwitchToComponent(Type type)
    {
        DebugMenuComponent instance = null;
        FieldInfo field = type.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
        if (field != null)
            instance = (DebugMenuComponent)field.GetValue(null);

        if (instance == null)
        {
            DebugMenuComponent com = (DebugMenuComponent)Activator.CreateInstance(type);
            com.Init();

            // Add missing instance field warning:
            if (field == null)
            {
                com.Main();
                string info = "* ".AddColor(Colors.Error) + $"Component {type.Name.AddColor(Colors.Application)} does not have an {"Instance".AddColor(Colors.Network)} variable" +
                $" - navigation history data will be lost!\n" +
                $"Add a {"public static".AddColor(Colors.Error)} {"Instance".AddColor(Colors.Network)} field " +
                $"and set it to {"'this'".AddColor(Colors.Error)} inside {"Init()".AddColor(Colors.Error)}!";
                DebugMenuEntry entry = new DebugMenuEntry(info, false) { Color = Colors.Default };
                com.AddEntry(entry);
            }

            if (!Instances.ContainsKey(type.Name)) Instances.Add(type.Name, com);
            instance = com;

            Logger.LogMethodW($"Component {type.Name} did not exist, so we instanced one. (total: {Instances.Count})", this);
        }
        //{ Logger.LogMethodW($"Cannot switch to component {type.Name} - not available!", this); return; }

        SwitchToComponent(instance);
    }
    public void SwitchToComponent(DebugMenuComponent com = null)
    {
        if (com == null) return;

        ActiveComponent = com;
        entry_index = com.entry_index; // Set this up early so that we keep previous positions through restarts
        Handle_ActiveComponent(entry_index);

        // Check whether we have entries
        if (ActiveComponent.Entries == null || ActiveComponent.Entries.Count == 0)
        {
            Logger.LogMethodE($"{com.GetType().Name.AddColor(Colors.Application)} has no entries! This is bad and should not happen!", this);
            return;
        }

        if (IsActive) Entry_Move(entry_index);
    }

    // Input processing | TODO TODO TODO: Wrapper for keyboard key down / pressed checking!
    void ProcessKeys()
    {
        // Enable & disable | F1: ON ; F2: OFF
        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            if (Keyboard.current.ctrlKey.isPressed || Keyboard.current.altKey.isPressed) return;
            if (Keyboard.current.shiftKey.isPressed) { MainMenu(); return; }

            if (IsActive) Logger.Log("[Help] " + $"{SelectedEntry.Text}: ".AddColor(Colors.Application) +
                          $"{(SelectedEntry.HelpText != null ? SelectedEntry.HelpText : "No help text for this entry.".AddColor(Colors.Unimportant))}");
            else SetActive(true);
        }
        else if (Keyboard.current.f2Key.wasPressedThisFrame) SetActive(false);

        if (!IsActive) return;

        // Move through entries:
        // U: Move down ; O: Move up
        if (Keyboard.current.uKey.wasPressedThisFrame)
            Entry_Move(DebugMenuEntryDir.Down);
        else if (Keyboard.current.oKey.wasPressedThisFrame) Entry_Move(DebugMenuEntryDir.Up);
        else if (Keyboard.current.homeKey.wasPressedThisFrame) Entry_Move(DebugMenuEntryDir.Home);
        else if (Keyboard.current.endKey.wasPressedThisFrame) Entry_Move(DebugMenuEntryDir.End);

        // Activate & manipulate entries:
        // Space: enter ; 1-2: Change value of variable entries
        if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.yKey.wasPressedThisFrame) // TODO: y / z? Perhaps both?
            PerformSelectedAction(DebugMenuVarDir.Action);
        else if (Keyboard.current.digit1Key.wasPressedThisFrame) PerformSelectedAction(DebugMenuVarDir.Decrease); // conflict with song changing in DebugKeys!
        else if (Keyboard.current.digit2Key.wasPressedThisFrame) PerformSelectedAction(DebugMenuVarDir.Increase); // conflict with song changing in DebugKeys!

        // History navigation | PgUp: backwards ; PgDn: forwards
        if (Keyboard.current.pageUpKey.wasPressedThisFrame) NavigateHistory(DebugMenuHistoryDir.Backwards);
        else if (Keyboard.current.pageDownKey.wasPressedThisFrame) NavigateHistory(DebugMenuHistoryDir.Forwards);
    }

    // Functions, if assigned, will always be executed!
    // TODO: revise this?
    void PerformSelectedAction(DebugMenuVarDir dir = DebugMenuVarDir.Action)
    {
        if ((int)dir < 3 && SelectedEntry.Variable != null)
        {
            float addition = (Keyboard.current.altKey.isPressed ? 0.1f : 1f) * (dir == DebugMenuVarDir.Decrease ? -1 : 1);
            switch (SelectedEntry.Variable.Value)
            {
                case bool b: SelectedEntry.Variable.Value = !b; break;
                case int i: SelectedEntry.Variable.Value = (i + addition); break;
                case float f: f++; SelectedEntry.Variable.Value = (f + addition); break;
                case double d: d++; SelectedEntry.Variable.Value = (d + addition); break;
            }
        }

        if (SelectedEntry.Page != null) { SwitchToComponent(SelectedEntry.Page); return; }
        SelectedEntry.DoFunction();
        if (ActiveComponent._GlobalEntryAction != null)
            ActiveComponent._GlobalEntryAction(SelectedEntry);
    }

    // ***** Navigation: *****
    void UpdateEntryColors(ref Dictionary<int, DebugMenuEntry> Entries, int target_index) => // TODO: is ref needed?
        Handle_ActiveComponent(target_index);
    // TODO: multiple targets?

    // Entry navigation:
    public int entry_index = 0; // Current index of entry (line)
    public bool Entry_Move(int index, int prev_index = -1)
    {
        bool success = true;

        Dictionary<int, DebugMenuEntry> Entries = ActiveComponent.Entries;
        if (Entries == null || Entries.Count == 0)
        {
            Logger.LogMethodW($"Entries are null or empty!", this);
            success = false; return false;
        }

        bool boundary = (index < 0 || index >= Entries.Count);

        DebugMenuEntry? entry = null;
        if (!boundary) entry = Entries.ElementAt(index).Value;

        if ((boundary || !entry.HasValue || !entry.Value.IsSelectable) && prev_index != -1)
        {
            success = false;

            // Find next selectable item
            int entry_count = Entries.Count;
            bool rollover = false; int addition = (index < prev_index ? -1 : 1);
            if (index < 0) index = entry_count - 1;

            for (int i = index; i <= entry_count; i += addition)
            {
                if (i >= entry_count)
                    if (!rollover) { i = 0; rollover = true; }
                    else break;

                DebugMenuEntry e = Entries.ElementAt(i).Value;
                if (e.IsSelectable)
                {
                    entry = e; index = i;
                    success = true; break;
                }
            }
        }

        if (!success) return false;

        // *** Move Debug menu arrow: ***
        // Get the left-top position of the line
        //int line_index = debugmenuText.textInfo.lineInfo[index].firstCharacterIndex;
        TMP_LineInfo line = new TMP_LineInfo();
        line = debugmenuText.textInfo.lineInfo[index];
        float line_height = (line.lineHeight * index + (line.lineHeight / 2));
        Vector3 line_worldPos = debugmenuText.transform.TransformPoint(new Vector3(0, -line_height, 0));

        Vector3 arrow_pos = line_worldPos + (Vector3.left * 25f); /* Left padding */
        debugmenuArrow.transform.position = arrow_pos;

        UpdateEntryColors(ref Entries, index);

        SelectedEntry = entry.Value;
        entry_index = ActiveComponent.entry_index = index; // Set roaming entry index as well!

        return true;
    }
    public void Entry_Move(DebugMenuEntryDir dir)
    {
        Dictionary<int, DebugMenuEntry> Entries = ActiveComponent.Entries;
        int index = entry_index;
        int prev_index = entry_index;
        bool result = false;
        switch (dir)
        {
            case DebugMenuEntryDir.Down:
                result = Entry_Move(++index, prev_index);
                break;
            case DebugMenuEntryDir.Up:
                result = Entry_Move(--index, prev_index);
                break;
            case DebugMenuEntryDir.Home:
                result = Entry_Move(0, prev_index);
                break;
            case DebugMenuEntryDir.End:
                result = Entry_Move(ActiveComponent.Entries.Count - 1, prev_index);
                break;
        }

        if (!result) { entry_index = prev_index; /* log !*/ return; } // Restore previous index if unsuccessful!
    }

    // Navigation history:
    int navigation_history_max = 20; // Hold a maximum of 20 components in the history
    int navigation_index = 0; // Navigation history index
    List<DebugMenuComponent> navigation_history = new List<DebugMenuComponent>();

    public void AddToNavigationHistory(DebugMenuComponent com)
    {
        navigation_history.Add(ActiveComponent);
        if (navigation_history.Count > 20)
            navigation_history.RemoveRange(19, navigation_history.Count - 19);
    }
    public void NavigateHistory(int index)
    {
        if (index >= navigation_history.Count || index < 0)
        {
            Logger.LogMethodW($"Boundary reached! | index: {index} - count: {navigation_history.Count}", this);
            return; // Return on boundaries
        }

        ActiveComponent = navigation_history[index];
    }
    public void NavigateHistory(DebugMenuHistoryDir dir)
    {
        int prev_index = navigation_index;
        switch (dir)
        {
            case DebugMenuHistoryDir.Forwards:
                NavigateHistory(++navigation_index);
                break;
            case DebugMenuHistoryDir.Backwards:
                NavigateHistory(--navigation_index);
                break;
            case DebugMenuHistoryDir.MainMenu:
                MainMenu();
                break;
        }
        bool fail = (navigation_index == prev_index);
        Logger.LogMethod($"Navigated {dir} to index {navigation_index}" + "(from {prev_index})".AddColor(0.45f) + (fail ? " FAIL!".AddColor(Colors.Error) : ""), this);
    }

    // *****

    float update_ElapsedMs;
    void Update()
    {
        if (ActiveComponent != null && ActiveComponent.Attribute.UpdateFrequencyInMs != -1)
        {
            update_ElapsedMs += Time.unscaledDeltaTime * 1000;
            if (ActiveComponent.Attribute.UpdateFrequencyInMs == 0 || update_ElapsedMs >= ActiveComponent.Attribute.UpdateFrequencyInMs)
                Handle_ActiveComponent(entry_index);
        }

        ProcessKeys();
    }
}