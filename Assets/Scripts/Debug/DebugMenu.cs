using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum DebugMenuHistoryDir { Backwards, Forwards, MainMenu = -1 }
public enum DebugMenuEntryDir { Up, Home, Down, End }

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

    bool _isActive;
    public bool IsActive { get { return _isActive; } }
    public static void SetActive(bool value) // TODO: static is hacky!
    {
        DebugMenu debugMenu = (DebugMenu)Instance.Component;
        if (!debugMenu) Logger.LogMethodW("Debug menu has no instance!", "DebugMenu");

        // TODO: disable player input during active!
        // Also invincibility, pausing (controllable)

        debugMenu._isActive = value;
        debugMenu.debugmenuText.gameObject.SetActive(value);
        debugMenu.debugmenuArrow.gameObject.SetActive(value);
        if (value) MainMenu();
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
    }

    // There's always one component that's considered as the Main menu.
    // It can be switched to a different one, but there has to be one.
    public static void MainMenu()
    {
        DebugMenu debugMenu = (DebugMenu)Instance.Component;
        if (!debugMenu) Logger.LogMethodW("Debug menu has no instance!", "DebugMenu");
        debugMenu.SwitchToComponent(typeof(DebugMenus.MainMenu));

        debugMenu.navigation_history.Clear(); // Clear navigation history
    }

    // Component switching & handling:
    // TODO: Components need to have their roaming entry indexes for navigation purposes!
    public void SwitchToComponent(Type type)
    {
        DebugMenuComponent instance = (DebugMenuComponent)type.GetField("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).GetValue(null);

        if (instance == null)
        {
            DebugMenuComponent com = (DebugMenuComponent)Activator.CreateInstance(type);
            com.Init();
            if (!Instances.ContainsKey(type.Name)) Instances.Add(type.Name, com);
            instance = com;

            Logger.LogMethodW($"Component {type.Name} did not exist, so we instanced one. (total: {Instances.Count})");
        }
        //{ Logger.LogMethodW($"Cannot switch to component {type.Name} - not available!", this); return; }

        SwitchToComponent(instance);
    }
    public void SwitchToComponent(DebugMenuComponent com = null)
    {
        if (com != null)
        {
            navigation_history.Add(new KeyValuePair<int, DebugMenuComponent>(entry_index, ActiveComponent));
            ActiveComponent = com;
        }
        Handle_ActiveComponent(entry_index);
    }
    void Handle_ActiveComponent(int index = 0)
    {
        if (ActiveComponent == null)
        { debugmenuText.text = ""; return; }

        if (_isActive)
            debugmenuText.text = ActiveComponent.Main(index);
    }

    // Input processing:
    void ProcessKeys()
    {
        // Enable & disable | F1: ON ; F2: OFF
        if (Keyboard.current.f1Key.wasPressedThisFrame) SetActive(true);
        else if (Keyboard.current.f2Key.wasPressedThisFrame) SetActive(false);

        if (!_isActive) return;

        // Move through entries:
        // U: Move down ; O: Move up
        if (Keyboard.current.uKey.wasPressedThisFrame) Entry_Move(DebugMenuEntryDir.Down);
        else if (Keyboard.current.oKey.wasPressedThisFrame) Entry_Move(DebugMenuEntryDir.Up);
        else if (Keyboard.current.homeKey.wasPressedThisFrame) Entry_Move(DebugMenuEntryDir.Home);
        else if (Keyboard.current.endKey.wasPressedThisFrame) Entry_Move(DebugMenuEntryDir.End);

        // Activate & manipulate entries:
        // Space: enter ; 1-2: Change value of variable entries
        if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.zKey.wasPressedThisFrame)
            if (SelectedEntry.Function != null) SelectedEntry.DoFunction();
        else if (Keyboard.current.digit1Key.wasPressedThisFrame) { } // conflict with song changing in DebugKeys!
        else if (Keyboard.current.digit2Key.wasPressedThisFrame) { } // conflict with song changing in DebugKeys!

        // History navigation | PgUp: backwards ; PgDn: forwards
        if (Keyboard.current.pageUpKey.wasPressedThisFrame) NavigateHistory(DebugMenuHistoryDir.Backwards);
        else if (Keyboard.current.pageDownKey.wasPressedThisFrame) NavigateHistory(DebugMenuHistoryDir.Forwards);
    }

    // ***** Navigation: *****

    void UpdateEntryColors(ref Dictionary<int, DebugMenuEntry> Entries, int target_index) => // TODO: is ref needed?
        Handle_ActiveComponent(target_index);
    // TODO: multiple targets?

    // Entry navigation:
    public int entry_index = 0; // Current index of entry (line)
    public void Entry_Move(int index)
    {
        Dictionary<int, DebugMenuEntry> Entries = ActiveComponent.Entries;
        if (Entries == null || Entries.Count == 0)
        {
            Logger.LogMethodW($"Entries are null or empty!", this);
            return;
        }
        if (index < 0 || index >= Entries.Count)
        {
            //Logger.LogMethodW($"Boundary reached! | index: {index} - count: {Entries.Count}", this);
            return;
        }

        DebugMenuEntry entry = Entries[index];

        if (!entry.IsSelectable)
        {
            UpdateEntryColors(ref Entries, -1);
            return;
        }

        // Debug menu arrow:
        // Get the left-top position of the line
        int line_index = debugmenuText.textInfo.lineInfo[index].firstCharacterIndex;
        TMP_CharacterInfo line = debugmenuText.textInfo.characterInfo[line_index];
        // The debug menu arrow has a pivot point of (0.5, 1), meaning that top-left of the line
        // will automatically center the arrow vertically, to the line.
        Vector3 line_topLeft = line.topLeft;
        Vector3 line_worldPos = debugmenuText.transform.TransformPoint(line_topLeft);

        Vector3 arrow_pos = line_worldPos + (Vector3.left * 25f); /* Left padding */
        debugmenuArrow.transform.position = arrow_pos;

        UpdateEntryColors(ref Entries, index);

        SelectedEntry = entry;
        entry_index = ActiveComponent.entry_index = index; // Set roaming entry index as well!
    }
    public void Entry_Move(DebugMenuEntryDir dir)
    {
        Dictionary<int, DebugMenuEntry> Entries = ActiveComponent.Entries;
        int prev_index = entry_index;
        switch (dir)
        {
            case DebugMenuEntryDir.Down:
                Entry_Move(++entry_index);
                break;
            case DebugMenuEntryDir.Up:
                Entry_Move(--entry_index);
                break;
            case DebugMenuEntryDir.Home:
                Entry_Move(0);
                break;
            case DebugMenuEntryDir.End:
                Entry_Move(ActiveComponent.Entries.Count - 1);
                break;
        }

        // TODO: not-selectable items should be skipped!
        // Handle situation where there's not a single selectable item!
        // Might not be handled here!
        //DebugMenuEntry result = Entries[entry_index];

        bool boundary_check = (entry_index < 0 || entry_index >= ActiveComponent.Entries.Count);
        //Logger.LogMethod($"Moved {dir} to index {entry_index}" + $"(from {prev_index})".AddColor(0.45f) + (boundary_check ? " BOUNDARY!".AddColor(Colors.Error) : ""), this);

        if (!boundary_check) return;
        // Rollover on boundary:
        if (entry_index < 0) Entry_Move(ActiveComponent.Entries.Count - 1); // Restore previous index on boundary
        else Entry_Move(0);
    }

    // History navigation:
    // Key: last entry index of com | Value: the com itself
    int navigation_index = 0; // Navigation history index
    List<KeyValuePair<int, DebugMenuComponent>> navigation_history = new List<KeyValuePair<int, DebugMenuComponent>>();

    public void NavigateHistory(int index)
    {
        if (index >= navigation_history.Count || index < 0)
        {
            Logger.LogMethodW($"Boundary reached! | index: {index} - count: {navigation_history.Count}", this);
            return; // Return on boundaries
        }

        ActiveComponent = navigation_history[index].Value;
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

    float update_ElapsedMs;
    void Update()
    {
        if (ActiveComponent != null && ActiveComponent.Attribute.UpdateFrequencyInMs != -1)
        {
            update_ElapsedMs += Time.unscaledDeltaTime * 1000;
            if (ActiveComponent.Attribute.UpdateFrequencyInMs == 0 || update_ElapsedMs >= ActiveComponent.Attribute.UpdateFrequencyInMs)
                Handle_ActiveComponent();
        }

        ProcessKeys();
    }
}