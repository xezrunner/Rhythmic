using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum DebugMenuHistoryDir { Backwards, Forwards, MainMenu = -1 }
public enum DebugMenuEntryDir { Up, Home, Down, End }
public enum DebugMenuVarDir { Decrease, Increase, Action }

[DebugComponent(DebugComponentFlag.DebugMenu, DebugComponentType.Component, true, -1)]
public partial class DebugMenu : DebugComponent
{
    DebugUI DebugUI;

    TextMeshProUGUI debugmenuText;
    public Image debugmenuArrow;

    public static DebugMenu Instance;
    public static RefDebugComInstance Instances;

    Dictionary<string, DebugMenuComponent> ComponentInstances = new Dictionary<string, DebugMenuComponent>();
    DebugMenuComponent ActiveComponent = null;
    DebugMenuEntry SelectedEntry;

    public bool IsActive;
    public static void SetActive(bool value) // TODO: static is hacky!
    {
        DebugMenu debugMenu = (DebugMenu)Instance;
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

        // Disable player input during debug menu
        AmpPlayerInputHandler.IsActive = !value;

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
        Instance = this;
        Instances = new RefDebugComInstance(this, gameObject);

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
        DebugMenu debugMenu = (DebugMenu)Instance;
        if (!debugMenu) Logger.LogMethodW("Debug menu has no instance!", "DebugMenu");
        debugMenu.SwitchToComponent(typeof(DebugMenus.MainMenu));

        // TODO: Navigation history - 20 limit!
        //debugMenu.navigation_history.Clear(); // Clear navigation history
    }

    // Help text:
    void LogHelp()
    {
        Logger.Log("[Help] " + "Debug menu: \n".AddColor(Colors.Application) +
                   "F1 - F2: ".AddColor(Colors.Network) + "Open / close the debug menu\n" +
                   "[Shift] + F1: ".AddColor(Colors.Network) + "Go back to the main menu\n" +
                   "U - O: ".AddColor(Colors.Network) + "Navigate up / down between entries\n" +
                   "1 - 2: ".AddColor(Colors.Network) + "Decrease / increase / toggle entry value\n" +
                   "Space - Y/Z: ".AddColor(Colors.Network) + "Activate entry function\n" +
                   "Page up/down: ".AddColor(Colors.Network) + "Navigate backwards / forwards (up / down) in the page history");
    }

    // Component switching & handling:
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

            if (!ComponentInstances.ContainsKey(type.Name)) ComponentInstances.Add(type.Name, com);
            instance = com;

            Logger.LogMethodW($"Component {type.Name} did not exist, so we instanced one. (total: {ComponentInstances.Count})", this);
        }
        //{ Logger.LogMethodW($"Cannot switch to component {type.Name} - not available!", this); return; }

        SwitchToComponent(instance);
    }
    public void SwitchToComponent(DebugMenuComponent com = null, bool addToHistory = true)
    {
        if (com == null) return;

        ActiveComponent = com;
        entry_index = com.entry_index; // Set this up early so that we keep previous positions through restarts
        if (addToHistory) AddToNavigationHistory(com);
        Handle_ActiveComponent(entry_index);

        // Check whether we have entries
        if (ActiveComponent.Entries == null || ActiveComponent.Entries.Count == 0)
        {
            Logger.LogMethodE($"{com.GetType().Name.AddColor(Colors.Application)} has no entries! This is bad and should not happen!", this);
            return;
        }

        if (IsActive) Entry_Move(entry_index);
    }

    // Function calls, variable increment / decrement:
    public void PerformSelectedAction(DebugMenuVarDir dir = DebugMenuVarDir.Action)
    {
        if ((int)dir < 3 && SelectedEntry.Variable != null)
        {
            float addition = (Keyboard.current.altKey.isPressed ? 0.1f : 1) * (dir == DebugMenuVarDir.Decrease ? -1 : 1);
            switch (SelectedEntry.Variable.Value)
            {
                case bool b: SelectedEntry.Variable.Value = !b; break;
                case int i: SelectedEntry.Variable.Value = (int)(i + addition); break;
                case float f: SelectedEntry.Variable.Value = (float)(f + addition); break;
                case double d: SelectedEntry.Variable.Value = (double)(d + addition); break;
                default: Logger.LogMethodE("Invalid object type " + $"{SelectedEntry.Variable.Value.GetType().Name}".AddColor(Colors.Network) + "!"); break;
            }
        }

        if (SelectedEntry.Page != null) { SwitchToComponent(SelectedEntry.Page); return; }
        SelectedEntry.DoFunction(); // Functions, if assigned, will always be executed! | TODO: revise this?
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
    int navigation_index = -1; // Navigation history index
    List<DebugMenuComponent> navigation_history = new List<DebugMenuComponent>();

    public void AddToNavigationHistory(DebugMenuComponent com)
    {
        navigation_history.Add(ActiveComponent);
        navigation_index = Mathf.Clamp(navigation_index + 1, 0, navigation_history_max); // Add 1, limit: <=20
        if (navigation_history.Count > navigation_history_max)
            navigation_history.RemoveAt(0);
    }
    public void NavigateHistory(int index)
    {
        if (index >= navigation_history.Count || index < 0)
        {
            Logger.LogMethodW($"Boundary reached! | index: {index} - count: {navigation_history.Count}");
            return; // Return on boundaries
        }

        DebugMenuComponent com = navigation_history[index];
        if (com != null)
        {
            SwitchToComponent(com, addToHistory: false);
            navigation_index = index;
        }
    }
    public void NavigateHistory(DebugMenuHistoryDir dir)
    {
        int index = navigation_index;
        int prev_index = navigation_index;
        switch (dir)
        {
            case DebugMenuHistoryDir.Forwards:
                NavigateHistory(++index);
                break;
            case DebugMenuHistoryDir.Backwards:
                NavigateHistory(--index);
                break;
            case DebugMenuHistoryDir.MainMenu:
                MainMenu();
                break;
        }
        bool fail = (navigation_index == prev_index);
        //Logger.LogMethod($"Navigated {dir} to index {navigation_index}" + "(from {prev_index})".AddColor(0.45f) + (fail ? " FAIL!".AddColor(Colors.Error) : ""), this);
    }

    // Loop:
    float update_ElapsedMs;
    void Update()
    {
        ProcessKeys();
        if (!IsActive) return;

        if (ActiveComponent != null && ActiveComponent.Attribute.UpdateFrequencyInMs != -1)
        {
            update_ElapsedMs += Time.unscaledDeltaTime * 1000;
            if (ActiveComponent.Attribute.UpdateFrequencyInMs == 0 || update_ElapsedMs >= ActiveComponent.Attribute.UpdateFrequencyInMs)
                Handle_ActiveComponent(entry_index);
        }

    }
}