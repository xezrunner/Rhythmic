using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using static QuickInput;
using static Logging;
using TMPro;

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

        DebugConsole.write_line("[debugmenu] initialized");
    }

    void Start() {
        if (!is_open) close();
        
        var page_0 = set_page(typeof(DebugMenu_Pages.MainPage));
    }

    Keyboard keyboard;

    [Header("UI objects:")]
    [Header("  - Self")]
    [SerializeField] RectTransform ui_panel;

    [Header("  - Content")]
    [SerializeField] GameObject    ui_container_gameobject;
    [SerializeField] RectTransform ui_container;

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
        IDebugMenu_Page page_interface = (IDebugMenu_Page)page;
        current_page = page;

        clear_lines();
        ui_temp_page_text.SetText("page: %".interp(page.GetType().Name)); // TODO: TEMP!
        page_interface.draw_page();
        select_line(0);
        
        log("switched to page '%'".interp(page.GetType().Name));
    }

    // Lines:
    List<DebugMenu_Line> ui_lines = new();

    public DebugMenu_Line add_new_line(DebugMenuEntry entry) {
        DebugMenu_Line line = Instantiate(prefab_ui_line);
        line.trans.SetParent(ui_container, false);

        line.set_from_entry(entry);
        line.clicked += line_clicked_event;

        ui_lines.Add(line);
        return line;
    }
    void destroy_line(int index) {
        if ((index < 0 || index > ui_lines.Count) && log_error("invalid index! (%)".interp(index))) return;

        ui_lines[index].clicked -= line_clicked_event;

        Destroy(ui_lines[index].self);
        ui_lines.RemoveAt(index);
    }
    void clear_lines() {
        for (int i = ui_lines.Count - 1; i >= 0; --i) destroy_line(i);
    }

    void line_clicked_event(object sender, DebugMenu_Line line) => select_and_invoke_from_click(line);
    bool select_and_invoke_from_click(DebugMenu_Line line) {
        select_line(line);
        return invoke_selection();
    }
    
    public DebugMenu_Line write_line(string text)                => add_new_line(new DebugMenuEntry(text));
    public DebugMenu_Line write_line(string text, Action action) => add_new_line(new DebugMenuEntry_Func(text, action));
    public DebugMenu_Line write_line(string text, Ref var_ref)   => add_new_line(new DebugMenuEntry_Var(text, var_ref));

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

    bool invoke_selection(int dir = 0) {
        DebugMenu_Line line = ui_lines[selection_index];
        DebugMenuEntry entry = line.entry;
        if (entry.entry_type == DebugMenuEntryType.Function) {
            DebugMenuEntry_Func entry_func = (DebugMenuEntry_Func)entry;
            entry_func.invoke();
            return true;
        } else if (entry.entry_type == DebugMenuEntryType.Variable) {
            log_warn("TODO: debug menu variable-type entries!");
            return true;
        }
        else {
            // log("empty entry invoked!");
            return false;
        }
    }

    // ----- //

    void Update() {
        if (was_pressed(keyboard?.f1Key)) open();
        if (was_pressed(keyboard?.f2Key)) close();

        if (!is_open) return;

        if (was_pressed(keyboard?.jKey)) select_next(-1);
        if (was_pressed(keyboard?.kKey)) select_next( 1);
        if (was_pressed(keyboard?.lKey)) invoke_selection();
    }

    // Debugging commands:
    [ConsoleCommand] static void cmd_debugmenu_select(string[] args) => get_instance()?.select_line(args[0].to_int());
    
    [ConsoleCommand] static void cmd_debugmenu_clear() => get_instance()?.clear_lines();
    
    [ConsoleCommand] public static void cmd_debugmenu_test_page() => get_instance()?.set_page(typeof(DebugMenu_Pages.TestPage));
    [ConsoleCommand] public static void cmd_debugmenu_main_page() => get_instance()?.set_page(typeof(DebugMenu_Pages.MainPage));

    [ConsoleCommand("Prints all cached (loaded) pages of the debug menu.")]
    static void cmd_print_debugmenu_page_cache() {
        DebugMenu inst = get_instance();
        if (!inst && log_warn("no debugmenu instance!")) return;
        DebugConsole.write_line("Listing cached pages: (%)".interp(inst.cached_pages.Count), LogLevel._ConsoleInternal);
        foreach (Type t in inst.cached_pages.Keys)
            DebugConsole.write_line("  - %".interp(t.Name), LogLevel._ConsoleInternal);
    }
}
