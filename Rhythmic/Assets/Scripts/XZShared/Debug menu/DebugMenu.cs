using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

using static QuickInput;
using static Logging;

public class DebugMenu : MonoBehaviour
{
    static DebugMenu instance;
    public static DebugMenu get_instance() {
        if (instance) return instance;
        else log_warn("no debugmenu instance!");
        return null;
    }

    void Awake() {
        instance = this;

        keyboard = Keyboard.current;
        if (keyboard == null) log_warn("no keyboard!");

        debugstats_instance = DebugStats.get_instance();
        if (!debugstats_instance) log_warn("no debugstats instance!");

        var page_0 = set_page(typeof(DebugMenu_Pages.MainPage));

        DebugConsole.write_line("[debugmenu] initialized");
    }

    void Start() {
        if (!is_open) close();
    }

    DebugStats debugstats_instance;
    Keyboard keyboard;

    [Header("UI objects:")]
    [Header("  - Self")]
    [SerializeField] RectTransform ui_panel;

    [Header("  - Content")]
    [SerializeField] GameObject    ui_container_gameobject;
    public RectTransform ui_container;

    [SerializeField] TMP_Text ui_temp_page_text; // TODO: TEMP!

    [Header("Prefabs:")]
    [SerializeField] DebugMenu_Line prefab_ui_line;

    // Self:
    public bool is_open;
    public void set_state(bool state) {
        ui_container_gameobject.SetActive(state);
        is_open = state;
    }
    public void open()   => set_state(true);
    public void close()  => set_state(false);
    public void toggle() => set_state(!is_open);

    // Pages:
    public DebugMenu_Page current_page;
    public Dictionary<Type, DebugMenu_Page> cached_pages = new();

    (DebugMenu_Page page, bool was_cached) lookup_or_cache_page(Type type) {
        if (type.BaseType != typeof(DebugMenu_Page)) {
            log_error("the requested page is of base type % - we expect %.".interp(type.BaseType.Name, nameof(DebugMenu_Page)));
            return (null, false);
        }
        DebugMenu_Page result;
        if (cached_pages.ContainsKey(type)) result = cached_pages[type];
        else {
            if (!typeof(IDebugMenu_Page).IsAssignableFrom(type) && log_error("'%' does not implement IDebugMenu_Page. Failing caching!".interp(type.Name))) return (null, false);

            DebugMenu_Page page = (DebugMenu_Page)Activator.CreateInstance(type);
            cached_pages.Add(type, page);
            return (page, false);
        }
        return (result, true);
    }

    (DebugMenu_Page page, bool success) set_page(Type type = null) {
        if (type.BaseType != typeof(DebugMenu_Page)) {
            log_error("the requested page is of base type % - we expect %.".interp(type.BaseType.Name, nameof(DebugMenu_Page)));
            return (null, false);
        }
        var lookup = lookup_or_cache_page(type);
        if (lookup.page == null && log_error("lookup/caching failed!")) return (null, false);
        // if (!lookup.was_cached) log_warn("page '%' wasn't cached".interp(type.Name));
        // else log_warn("page '%' was cached".interp(type.Name)); 

        set_page_internal(lookup.page);

        return (lookup.page, true);
    }
    void set_page_internal(DebugMenu_Page page = null) {
        current_page = page;
        ui_temp_page_text.SetText("page: %".interp(page.GetType().Name)); // TODO: TEMP!

        clear_lines();
        draw_current_page();
        select_line(0);

        log("switched to page '%'".interp(page.GetType().Name), LogLevel.Debug);
    }

    void draw_current_page() {
        IDebugMenu_Page page_interface = (IDebugMenu_Page)current_page;

        page_interface.layout();
        draw_current_page_lines();

        // TODO: BUG: If the amount of lines change, move the selection somewhere.
        if (!select_line(selection_index)) select_line(0);

        // Force a layout update, in case the height of the debug menu has changed.
        UPDATE_Layout(true);
    }

    // Lines:
    List<DebugMenuEntry> entry_queue = new(); // @Naming
    List<DebugMenu_Line> ui_lines = new();

    void draw_current_page_lines() {
        var var_entries = entry_queue.Where(x => x.entry_type == DebugMenuEntryType.Variable);
        int pad_length = var_entries.Count() != 0 ? var_entries.Max(x => x.text.Length) + 2 : 0;

        for (int i = 0; i < entry_queue.Count; ++i) {
            DebugMenuEntry entry = entry_queue[i];
            string text = entry.text;
            string var_text = null;
            if (entry.entry_type == DebugMenuEntryType.Variable) {
                text = text.PadRight(pad_length);
                DebugMenuEntry_Var entry_var = (DebugMenuEntry_Var)entry;
                var_text = "%".interp(entry_var.var_ref.get_value().ToString());
            }
            string final_text = "%%".interp(text, var_text);
            // Re-use existing line:
            if (i < ui_lines.Count) {
                ui_lines[i].entry = entry;;
                ui_lines[i].set_text(final_text);
            }
            else add_new_line(entry, final_text);
        }
        entry_queue.Clear();
    }

    public DebugMenu_Line add_new_line(DebugMenuEntry entry, string text) {
        DebugMenu_Line line = Instantiate(prefab_ui_line);
        line.trans.SetParent(ui_container, false);

        line.entry = entry;
        line.set_text(text);
        line.pointer_up_event   += line_pointer_up_event;
        line.pointer_down_event += line_pointer_down_event;

        ui_lines.Add(line);
        return line;
    }
    public DebugMenuEntry queue_entry(DebugMenuEntry entry) {
        entry_queue.Add(entry);
        return entry;
    }
    void destroy_line(int index) {
        if ((index < 0 || index > ui_lines.Count) && log_error("invalid index! (%)".interp(index))) return;

        ui_lines[index].pointer_up_event   -= line_pointer_up_event;
        ui_lines[index].pointer_down_event -= line_pointer_down_event;

        Destroy(ui_lines[index].self);
        ui_lines.RemoveAt(index);
    }
    void clear_lines() {
        for (int i = ui_lines.Count - 1; i >= 0; --i) destroy_line(i);
    }

    void line_pointer_up_event(object sender, (DebugMenu_Line line, int dir) info) {
        log("up", LogLevel.Debug);
        // Since we now select the item on pointer down, this is useless:
        //select_and_invoke_from_click(info.line, info.dir);
        invoke_selection(info.dir);
        is_holding = false;
    }
    void line_pointer_down_event(object sender, (DebugMenu_Line line, int dir) info) {
        log("down", LogLevel.Debug);
        select_line(info.line);
        is_holding = true;
        hold_dir = info.dir;
        held_ms = 0f;
        held_repeat_ms = -1f;
    }

    bool select_and_invoke_from_click(DebugMenu_Line line, int dir = 0) {
        if (!select_line(line)) return false;
        return invoke_selection(dir);
    }
    
    // Repetition:

    // TODO: allow repetition with the keyboard as well! Tie into invoke_selection()? 

    [Header("Options")]
    [Tooltip("For how long should an entry be held with the mouse before it begins repeating invokation.")]
    public float DEBUGMENU_MouseHoldDelayMs   = 500f;
    [Tooltip("For how long should wait in-between repeating invokations.")]
    public float DEBUGMENU_MouseRepeatDelayMs = 30f;
    
    bool is_holding = false;
    int hold_dir = 0;
    float held_ms;
    float held_repeat_ms = -1; // -1 means allow once at start
    void UPDATE_Holding() {
        if (!is_holding) return;

        held_ms += Time.unscaledDeltaTime * 1000f;
        if (held_ms <= DEBUGMENU_MouseHoldDelayMs) return;

        if (held_repeat_ms != -1 && held_repeat_ms < DEBUGMENU_MouseRepeatDelayMs) {
            held_repeat_ms += Time.unscaledDeltaTime * 1000f;
            return;
        } else held_repeat_ms = 0f;

        invoke_selection(hold_dir);
    }
    
    public DebugMenuEntry write_line(string text)                => queue_entry(new DebugMenuEntry(text));
    public DebugMenuEntry write_line(string text, Action action) => queue_entry(new DebugMenuEntry_Func(text, action));
    public DebugMenuEntry write_line(string text, Ref var_ref)   => queue_entry(new DebugMenuEntry_Var(text, var_ref));

    // Selection:
    int selection_index = 0;

    bool select_line(DebugMenu_Line line) {
        if (!line && log_error("null line!")) return false;
        if (ui_lines.Count == 0 && log_warn("empty page!")) return false;
        if (line.is_separator()) return false;

        for (int i = 0; i < ui_lines.Count; ++i) {
            bool match = ui_lines[i] == line;
            if (match) selection_index = i;
            ui_lines[i].set_selected(match);
        }

        return true;
    }
    bool select_line(int index) {
        if (ui_lines.Count == 0 && log_warn("empty page!")) return false;
        if ((index < 0 || index >= ui_lines.Count) && log_error("invalid index (%)!".interp(index))) return false;

        return select_line(ui_lines[index]);
    }
    (bool success, int index) select_next(int dir) {
        if (ui_lines.Count == 0 && log_warn("empty page!")) return (false, -1);

        int index = selection_index;
        int rollover_count = -1;
        do {
            index += dir;
            if (index >= ui_lines.Count && ++rollover_count < 1) index = 0;
            else if (index < 0 && ++rollover_count < 1)          index = ui_lines.Count - 1;
            else if (rollover_count > 0) {
                log_error("rollover failure!");
                break;
            }
        } while (ui_lines[index].is_separator());

        bool success = select_line(index);
        return (success, index);
    }

    // TODO: could incorporate something like this into Ref generally, for use in DebugConsole also.
    // Try setting the variable for the types we support natively, otherwise, fallback to Convert.ChangeType().
    void invoke_handle_variable(DebugMenuEntry_Var entry, int dir = 1) {
        Ref var_ref = entry.var_ref;

        // TODO: can't use a switch here as types are not compile-time constants.
        // Can we do something about that? Anything better here? Factor out to another file? To Ref?
        if      (var_ref.var_type == typeof(int))   var_ref.set_value(  (int)var_ref.get_value() + dir);
        else if (var_ref.var_type == typeof(float)) var_ref.set_value((float)var_ref.get_value() + dir);
        else if (var_ref.var_type == typeof(bool))  var_ref.set_value(!(bool)var_ref.get_value());
        else if (var_ref.var_type.BaseType == typeof(Enum)) {
            int value_count = Enum.GetValues(var_ref.var_type).Length;
            int target      = (int)var_ref.get_value() + dir; // @Perf
            if      (target >= value_count) target = 0;
            else if (target < 0)            target = value_count - 1;

            object final_enum = Enum.Parse(var_ref.var_type, target.ToString());
            var_ref.set_value(final_enum);
        }
    }

    bool invoke_selection(int dir = 0) {
        if (ui_lines.Count == 0 && log_warn("no lines!")) return false;

        DebugMenu_Line line = ui_lines[selection_index];
        DebugMenuEntry entry = line.entry;
        bool success = false;

        if (entry.entry_type == DebugMenuEntryType.Function) {
            DebugMenuEntry_Func entry_func = (DebugMenuEntry_Func)entry;
            entry_func.invoke();
            success = true;
        } else if (entry.entry_type == DebugMenuEntryType.Variable) {
            invoke_handle_variable((DebugMenuEntry_Var)entry, dir);
            success = true;
        }

        // Re-draw page to show values properly.
        draw_current_page();
        
        return success;
    }

    // Layout:
    bool last_open_state = false;
    void UPDATE_Layout(bool force = false) {
        // Move DebugStats out of the way when we are active:
        if (!debugstats_instance) return;
        if (force || is_open != last_open_state) {
            // We unfortunately have to use a coroutine to update the layout, since UI sizings only update
            // at the end of a frame.
            StartCoroutine(COROUTINE_Layout());
            last_open_state = is_open;
        }
    }
    IEnumerator COROUTINE_Layout() {
        // NOTE: the size of UI elements are zero until they have been visible for one frame.
        // We have to wait for a frame until the debug menu container size becomes non-zero:
        yield return new WaitForEndOfFrame();

        if (is_open) {
            debugstats_instance.set_y(-25 - ui_container.sizeDelta.y);
        } else {
            debugstats_instance.set_y(-10);
        }

        // log("debugstats_instance.get_y(): %  ui_container.sizeDelta.y: %".interp(debugstats_instance.get_y(), ui_container.sizeDelta.y));
    }

    // ----- //

    void Update() {
        if (was_pressed(keyboard?.f1Key)) open();
        if (was_pressed(keyboard?.f2Key)) close();

        UPDATE_Layout();

        if (!is_open) return;

        // TODO: was_pressed_or_held()
        // TODO: cheat keys in internal builds!
        if (was_pressed(keyboard?.oKey, keyboard?.upArrowKey))   select_next(-1);
        if (was_pressed(keyboard?.uKey, keyboard?.downArrowKey)) select_next( 1);
        if (was_pressed(keyboard?.jKey, keyboard?.yKey, keyboard?.zKey, keyboard?.spaceKey, keyboard?.enterKey))
            invoke_selection();
        if (was_pressed(keyboard?.digit1Key, keyboard?.leftArrowKey))  invoke_selection(-1);
        if (was_pressed(keyboard?.digit2Key, keyboard?.rightArrowKey)) invoke_selection( 1);

        UPDATE_Holding();
    }

    // Debugging commands:
    [ConsoleCommand] static void cmd_debugmenu_select(string[] args) => get_instance()?.select_line(args[0].as_int());
    
    [ConsoleCommand] static void cmd_debugmenu_clear() => get_instance()?.clear_lines();
    
    [ConsoleCommand] public static void cmd_debugmenu_test_page() => get_instance()?.set_page(typeof(DebugMenu_Pages.TestPage));
    [ConsoleCommand] public static void cmd_debugmenu_main_page() => get_instance()?.set_page(typeof(DebugMenu_Pages.MainPage));

    [ConsoleCommand("Prints all cached (loaded) pages of the debug menu.")]
    static void cmd_debugmenu_print_page_cache() {
        DebugMenu inst = get_instance();
        if (!inst && log_warn("no debugmenu instance!")) return;
        DebugConsole.write_line("Listing cached pages: (%)".interp(inst.cached_pages.Count), LogLevel._ConsoleInternal);
        foreach (Type t in inst.cached_pages.Keys)
            DebugConsole.write_line("  - %".interp(t.Name), LogLevel._ConsoleInternal);
    }
}
