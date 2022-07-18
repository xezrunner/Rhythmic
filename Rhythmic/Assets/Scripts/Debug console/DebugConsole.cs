using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using static Logging;
using static QuickInput;
using static AnimEasing;
using System.Linq;
using System.Text;
using System;

public partial class DebugConsole : MonoBehaviour {
    static DebugConsole instance;
    public static DebugConsole get_instance() {
        if (instance) return instance;
        Debug.LogWarning("DebugConsole does not have an instance!");
        return null;
    }

    void Awake() {
        instance = this;
        self = gameObject;

        keyboard = Keyboard.current;
        if (keyboard == null) log_warn("no keyboard!");

        ui_canvas = FindObjectOfType<Canvas>()?.GetComponent<RectTransform>();
        if (!ui_canvas) log_error("no ui_canvas!");

        // Disable Unity's SRP Debug canvas:
        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false; // @SRPDebugCanvas @WordDeleteClash

        ui_lines = new(capacity: CONSOLE_MaxLines);
        history = new(CONSOLE_MaxHistoryEntries);
        register_commands_from_assembly();
    }
    void Start() {
        // Start closed:
        if (!is_open) {
            is_open = true;
            close(false);
        }
        
        sizing_y = (CONSOLE_Height, CONSOLE_Height);
        write_line("[console] initialized");
    }

    RectTransform ui_canvas;
    GameObject self;
    Keyboard keyboard;

    [Header("State")]
    public bool is_open     = false;
    public bool is_expanded = false;

    [Header("UI objects:")]
    [Header("  - Self")]
    [SerializeField] RectTransform  ui_panel;

    [Header("  - Content")]
    [SerializeField] RectTransform  ui_text_container;
    [SerializeField] RectTransform  ui_scroll_rect_trans;
    [SerializeField] ScrollRect     ui_scroll_rect;

    [Header("  - Filter warning panel")]
    [SerializeField] GameObject     ui_filter_warning_panel_gameobject;
    [SerializeField] RectTransform  ui_filter_warning_panel_rect;
    [SerializeField] Image          ui_filter_warning_panel_image;
    [SerializeField] TMP_Text       ui_filter_warning_panel_text;

    [Header("  - Input field")]
    [SerializeField] TMP_InputField ui_input_field;
    [SerializeField] TMP_Text       ui_input_field_text;
    [SerializeField] TMP_Text       ui_autocomplete_text;
    [SerializeField] RectTransform  ui_autocomplete_text_rect;

    [Header("Prefabs:")]
    [SerializeField] DebugConsole_Line prefab_ui_line;
    
    [Header("Options:")]
    [Tooltip("The default height of the console. [Do not change dynamically!]")]
    public float   CONSOLE_DefaultHeight         = 270f;
    [Tooltip("The current height of the console. Used in openness and sizing.")]
    public float   CONSOLE_Height                = 270f;
    [Tooltip("The threshold for amount of lines in the console where if exceeded, oldest entries will start being removed.\n" +
             "Use -1 or below to allow infinite entries.")]
    public int     CONSOLE_MaxLines              = 300;

    [Tooltip("Controls whether to repeat the last submitted entry when submitting an empty input.")]
    public bool    CONSOLE_RepeatOnEmptySubmit   = true;
    [Tooltip("Controls whether the console should write the input into itself upon submission.")]
    public bool    CONSOLE_EchoBack              = true;
    [Tooltip("Controls how many input submissions we want to store in the history buffer." + 
             "Oldest entries will start being removed once exceeded.")]
    public int     CONSOLE_MaxHistoryEntries     = 100;

    [Tooltip("Controls whether you can autocomplete commands in the console using [(Shift +)Tab].")]
    public bool    CONSOLE_EnableAutocomplete    = true;
    [Tooltip("The padding between the input field text and the autocomplete text.")]
    public float   CONSOLE_AutocompletePaddingX  = 18f;

    [Tooltip("Controls whether the submit keys can be held down to repeatedly submit input to the console.")]
    public bool    CONSOLE_AllowSubmitRepetition = false;
    public float   CONSOLE_RepeatHoldTime        = 380f;
    public float   CONSOLE_RepeatHoldDelay       = 10f;

    [Tooltip("Controls the animation speed for openness and sizing.")]
    public float   CONSOLE_AnimSpeed       = 3f;
    [Tooltip("Controls the animation speed for scrolling (when the scroll animation is requested).")]
    public float   CONSOLE_ScrollAnimSpeed = 3f;

    [Tooltip("Controls the visibility of the category filter button on each line.")]
    public bool    CONSOLE_ShowLineCategories = true;
    
    public void open(bool anim = true) {
        if (is_open) {
            focus_input_field();
            return;
        }

        set_openness_anim((ui_panel.anchoredPosition.y, 0), t: anim ? 0 : 1);
        is_open = true;

        ui_input_field.interactable = true;
        focus_input_field();
        // ...
    }

    public void close(bool anim = true) {
        if (!is_open) return;

        set_openness_anim((ui_panel.anchoredPosition.y, !is_expanded ? CONSOLE_Height : ui_canvas.rect.height), t: anim ? 0 : 1);
        is_open = false;
        
        ui_input_field.interactable = false;
        // ...
    }

    public bool toggle() {
        if (!is_open) open();
        else          close();
        return is_open;
    }

    void set_openness_anim((float, float) y_pos, float t = 0) {
        openness_y_pos = y_pos;
        openness_t = t;
    }

    float openness_t = 1f;
    (float from, float to) openness_y_pos;
    public void UPDATE_Openness() {
        if (openness_t > 1f) return;

        float y_pos = ease_out_quadratic(openness_y_pos.from, openness_y_pos.to, openness_t);
        ui_panel.anchoredPosition = new(ui_panel.anchoredPosition.x, y_pos);

        openness_t += CONSOLE_AnimSpeed * Time.unscaledDeltaTime;
    }

    // Sizing:
    public void change_size(bool expanded, float height = -1f) {
        if (!expanded && height > 30f) CONSOLE_Height = height;
        sizing_y = (ui_panel.sizeDelta.y, !expanded ? CONSOLE_Height : ui_canvas.rect.height);

        is_expanded = expanded;
        sizing_t = 0;
    }
    public void toggle_expanded() => change_size(!is_expanded);

    float sizing_t = 1f;
    (float from, float to) sizing_y;
    (float from, float to) sizing_w;
    public void UPDATE_Sizing() {
        if (sizing_t > 1f) return;

        float y_pos = ease_out_quadratic(sizing_y.from, sizing_y.to, sizing_t);
        ui_panel.sizeDelta = new(ui_panel.sizeDelta.x, y_pos);
        // TODO: animate W!
        // float w_pos = ease_out_quadratic(sizing_w.from, sizing_w.to, sizing_t);
        // ui_panel.sizeDelta = new(w_pos, ui_panel.sizeDelta.y);

        sizing_t += CONSOLE_AnimSpeed * Time.unscaledDeltaTime;
        // Keep scrolling to the bottom throughout the size change, without animating the scroll.
        scroll_to_bottom(false);
    }

    // Scrolling:
    public const float SCROLL_TOP = 1f;
    public const float SCROLL_BOTTOM = 0f;

    bool  is_scrolling = false;
    float scroll_target;
    float scroll_t = 1f;
    void UPDATE_ScrollRequest() {
        if (!is_scrolling) return;
        // Cancel automated scrolling when scrolling with mouse wheel:
        // TODO: could have a delay here, so that we don't interfere with the user for
        // at least 1s or something.
        if (Mouse.current != null && Mouse.current.scroll.y.ReadValue() != 0) return;

        ui_scroll_rect.verticalNormalizedPosition = 
            ease_out_quadratic(ui_scroll_rect.verticalNormalizedPosition, scroll_target, scroll_t);
        scroll_t += Time.unscaledDeltaTime * CONSOLE_ScrollAnimSpeed;

        if (scroll_t > 1f) is_scrolling = false;
    }
    public void scroll_console(float value, bool anim = true) {
        scroll_t = anim ? 0 : 1;
        scroll_target = value;
        is_scrolling = true;
    }
    public void scroll_to_top(bool anim = true)    => scroll_console(SCROLL_TOP, anim);
    public void scroll_to_bottom(bool anim = true) => scroll_console(SCROLL_BOTTOM, anim);

    // Input field:
    public void focus_input_field() {
        ui_input_field.ActivateInputField();
    }
    public void defocus_input_field() {
        ui_input_field.DeactivateInputField();
    }
    void clear_input_field() {
        // TODO: Is this correct / any faster?
        set_input_field_text(null); // @Optimization
    }

    void input_delete_word(int dir) { // -1: left | 1: right
        if (ui_input_field.text == "") return;
        if (ui_input_field.text.Length == 1) return;

        int caret_position = ui_input_field.caretPosition;
        string s0 = ui_input_field.text[.. caret_position]; // 0 -> *| ...
        string s1 = ui_input_field.text[caret_position ..];

        if (dir == -1) // Delete from caret to the left (one word)
        {
            if (caret_position == 0) return;

            string[] tokens = s0.Split(' ');
            if (tokens.Length > 0) tokens[^1] = "";

            s0 = string.Join(" ", tokens);
            if (!s0.is_empty() && s0[^1] == ' ') s0 = s0[.. ^1];
        } else if (dir == 1) {
            if (caret_position == ui_input_field.text.Length - 1) return;

            string[] tokens = s1.Split(' ');
            if (tokens.Length > 0) tokens[0] = "";

            s1 = string.Join(" ", tokens);
            if (s1.Length > 1) s1 = s1[1 ..]; // Eat first space
        }

        string s = s0 + s1;
        set_input_field_text(s);
        //InputField_ChangeText(s, caret_position);
    }

    void set_input_field_text(string text, bool notify = true, int caret_pos = -1) {
        if (notify) ui_input_field.text = text;
        else        ui_input_field.SetTextWithoutNotify(text);
        ui_input_field.caretPosition = (caret_pos == -1) ? ui_input_field.text.Length : caret_pos;
    }

    public void input_field_value_changed() {
        if (!is_autocompleting) {
            build_autocomplete_list();
            autocomplete_index = -1;
        }
        build_autocomplete_ui();

        is_autocompleting = false;
    }

    // History:
    List<string> history;

    void history_add(string input) {
        if (input.is_empty()) return;
        if (CONSOLE_MaxHistoryEntries == 0) return;

        history.Add(input);
        if (history.Count > CONSOLE_MaxHistoryEntries) history.RemoveAt(0);
    }

    int history_index = -1;
    void history_next(int dir) {
        if (history == null && log_error("input_history is null!")) return;
        if (history.Count == 0) return;
        if (CONSOLE_MaxHistoryEntries == 0) return;
        
        history_index += dir;
        if      (history_index >= history.Count) history_index = 0;
        else if (history_index < 0)              history_index = history.Count - 1;
        
        set_input_field_text(history[history_index]);
    }

    // Autocomplete:
    bool is_autocompleting = false;
    int autocomplete_index = -1;
    List<string> autocomplete_list = new();
    void autocomplete_next(int dir) {
        if (!CONSOLE_EnableAutocomplete)  return;
        if (autocomplete_list == null && log_error("autocomplete_list is null!")) return;
        if (autocomplete_list.Count == 0) return;

        autocomplete_index += dir;
        if      (autocomplete_index >= autocomplete_list.Count) autocomplete_index = 0;
        else if (autocomplete_index < 0) autocomplete_index = autocomplete_list.Count - 1;

        is_autocompleting = true;

        set_input_field_text(autocomplete_list[autocomplete_index]);
    }

    bool autocomplete_list_debug = false;
    List<string> build_autocomplete_list(string input = null) {
        if (!CONSOLE_EnableAutocomplete) return null;

        if (input == null) input = ui_input_field.text;
        if (input.is_empty()) return null;

        autocomplete_list.Clear();
        
        // Check whether the whole input itself is contained in the registered commands list:
        if (registered_commands.Keys.Contains(input)) autocomplete_list.Add(input);
        // Find all other entries:
        foreach (string key in registered_commands.Keys) {
            if (key == input) continue;
            if (key.StartsWith(input)) autocomplete_list.Add(key);
        }

        if (autocomplete_list_debug) write_line("Autocomplete: [%]".interp(string.Join("; ", autocomplete_list)));
        return autocomplete_list;
    }
    void build_autocomplete_ui() {
        ui_autocomplete_text.SetText("");
        
        if (!CONSOLE_EnableAutocomplete)    return;
        if (ui_input_field.text.is_empty()) return;
        if (autocomplete_list.Count == 0)   return;

        int     last_c_index = ui_input_field_text.textInfo.lineInfo[0].lastVisibleCharacterIndex;
        var     last_c_info  = ui_input_field_text.textInfo.characterInfo[last_c_index];
        Vector2 last_c_pos   = ui_input_field_text.rectTransform.TransformPoint(last_c_info.bottomRight);
        ui_autocomplete_text_rect.position = new Vector2(last_c_pos.x + CONSOLE_AutocompletePaddingX, ui_autocomplete_text_rect.position.y);

        StringBuilder builder = new(); // TODO: Is a StringBuilder beneficial here?
        for (int i = 0; i < autocomplete_list.Count; ++i) {
            string to_add = autocomplete_list[i];
            if (i == autocomplete_index) {
                to_add = to_add.color("#03A9F4");
                to_add = to_add.bold();
                to_add = to_add.underline();
            }
            if (i != autocomplete_list.Count - 1) to_add += "; ";
            builder.Append(to_add);
        }
        ui_autocomplete_text.SetText($":: {builder}");
    }

    // Lines:
    List<DebugConsole_Line> ui_lines = new();

    DebugConsole_Line add_new_line(string text, LogLevel level = LogLevel.Info) {
        DebugConsole_Line com = Instantiate(prefab_ui_line);
        com.trans.SetParent(ui_text_container, false);

        com.set_text(text);
        com.category = level;
        com.category_button_clicked_event += category_button_clicked;

        ui_lines.Add(com);
        if (ui_lines.Count > CONSOLE_MaxLines) destroy_line(0);

        return com;
    }
    void destroy_line(int index) {
        if ((index < 0 || index >= ui_lines.Count) && log_error("invalid index!")) return;
        ui_lines[0].category_button_clicked_event -= category_button_clicked;
        Destroy(ui_lines[0].gameobject);
        ui_lines.RemoveAt(0);
    }
    void category_button_clicked(object sender, LogLevel category) {
        // write_line("pressed!  cat: %".interp(category));
        if (current_filter == category) category = LogLevel.None;
        filter(category);
    }

    // Processing & commands:
    void write_line_internal(string message) {
        add_new_line(message, LogLevel._ConsoleInternal);
    }
    public static void write_line(string message, LogLevel level = LogLevel.Info) {
        get_instance()?.add_new_line(message, level);
    }
    
    static bool submit_debug = false;
    void submit(string input = null) {
        // Assume we want to submit the input field text when not given a parameter:
        if (input == null) {
            input = ui_input_field.text;
            // Re-focus the input field upon submit:
            focus_input_field();
            // If submit repetition is not allowed, clear the input field upon submit:
            if (!CONSOLE_AllowSubmitRepetition) clear_input_field();
        }

        if (input.is_empty() && CONSOLE_RepeatOnEmptySubmit && history.Count != 0) input = history.Last();
        if (CONSOLE_EchoBack) write_line("> %".interp(input), LogLevel._ConsoleInternal);
        history_add(input);
        history_index = -1;
        scroll_to_bottom();

        string[] split  = input.Split(' ');
        string   s_cmd  = split[0];
        string[] s_args = split[1 ..];

        if (submit_debug) write_line("[submit] s_cmd: % :: s_args: [%]".interp(s_cmd, s_args.Length == 0 ? "none" : string.Join("; ", s_args)));
        
        if (!registered_commands.ContainsKey(s_cmd)) {
            write_line("Could not find command: %".interp(input), LogLevel._ConsoleInternal);
            return;
        }

        ConsoleCommand cmd = registered_commands[s_cmd];
        if (cmd.command_type == ConsoleCommandType.Function) {
            ConsoleCommand_Func cmd_func = (ConsoleCommand_Func)cmd;
            cmd_func.invoke(s_args);
        }
        else if (cmd.command_type == ConsoleCommandType.Variable) {
            ConsoleCommand_Variable cmd_var = (ConsoleCommand_Variable)cmd;
            Ref var_ref = cmd_var.var_ref;
            if (s_args.Length == 0) write_line_internal("% (%): %".interp(s_cmd, var_ref.var_type.Name, var_ref.get_value()));
            else {
                object value_to_set;
                try {
                    value_to_set = Convert.ChangeType(s_args[0], var_ref.var_type);
                } catch (Exception ex) { write_line("Could not set value: %".interp(ex.Message), LogLevel.Error); return; }
                
                var_ref.set_value(value_to_set);
            }
        }
    }

    int repeated_submits_count = 0;
    float submit_hold_timer_ms = 0f;
    void UPDATE_HandleSubmitRepetition() {
        if (!CONSOLE_AllowSubmitRepetition) return;

        // If we keep holding a submit key, repeatedly submit after a delay:
        if (is_held(keyboard?.enterKey, keyboard?.numpadEnterKey)) {
            if (submit_hold_timer_ms < CONSOLE_RepeatHoldTime) submit_hold_timer_ms += Time.unscaledDeltaTime * 1000f;
            else {
                submit();
                ++repeated_submits_count;
                submit_hold_timer_ms = CONSOLE_RepeatHoldTime - CONSOLE_RepeatHoldDelay;
            }
        }
        if (was_released(keyboard?.enterKey, keyboard?.numpadEnterKey)) {
            if (repeated_submits_count > 0) log("repeatedly submitted % times.".interp(repeated_submits_count));
            repeated_submits_count = 0;
            submit_hold_timer_ms = 0f;

            clear_input_field();
            focus_input_field();
        }
    }
    
    // Filtering (categories):
    public LogLevel current_filter = LogLevel.None;
    float  filter_last_scroll_location = -1f;
    void filter(LogLevel category = LogLevel.None) {
        if (current_filter == category) return;
        // log("Filtering console by %".interp(category), LogLevel._IgnoreFiltering);
        current_filter = category;
        set_ui_filter_warning_panel(category);
        
        if (filter_last_scroll_location != -1f && category == LogLevel.None) {
            scroll_console(filter_last_scroll_location);
            filter_last_scroll_location = -1f;
        }
        filter_last_scroll_location = ui_scroll_rect.verticalNormalizedPosition;
    }
    void UPDATE_Filtering() {
        foreach (DebugConsole_Line line in ui_lines) {
            bool new_state =  current_filter == LogLevel.None || line.category.HasFlag(LogLevel._IgnoreFiltering) ||
                              line.category == current_filter;
            line.set_state(new_state);
        }
    }
    // TODO: Unify this into some helper class:
    public Color copy_color_add(Color color, float add) => new(color.r + add, color.g + add, color.b + add, color.a);
    void set_ui_filter_warning_panel(LogLevel category) {
        if (category == LogLevel.None /*|| category == LogLevel.Info*/) {
            ui_filter_warning_panel_gameobject.SetActive(false);
            ui_scroll_rect_trans.offsetMax = new(ui_scroll_rect_trans.offsetMax.x, 0);
            return;
        }
        ui_filter_warning_panel_image.color = copy_color_add(XZ_GetColorForLogLevel(category), -0.2f);
        ui_filter_warning_panel_text.SetText("Filter active: %".interp(category.ToString()));
        ui_scroll_rect_trans.offsetMax = new(ui_scroll_rect_trans.offsetMax.x, -ui_filter_warning_panel_rect.sizeDelta.y);
        ui_filter_warning_panel_gameobject.SetActive(true);
    }

    void Update() {
        UPDATE_Openness();

        // Do not allow toggling with the [0 / backtick] key - it would clash with wanting to input '0':
        if      (!is_open && was_pressed(keyboard?.digit0Key, keyboard?.backquoteKey)) open();
        else if  (is_open && was_pressed(keyboard?.escapeKey)) close();

        if (!is_open) return;
        
        // Input submission:
        if (was_pressed (keyboard?.enterKey, keyboard?.numpadEnterKey)) submit();
        UPDATE_HandleSubmitRepetition();

        // Sizing:
        if (ui_input_field.text.Length == 0 && was_pressed(keyboard?.tabKey)) toggle_expanded();
        UPDATE_Sizing();

        // Scrolling animations:
        UPDATE_ScrollRequest();

        // History navigation:
        if (was_pressed(keyboard?.upArrowKey))   history_next(-1);
        if (was_pressed(keyboard?.downArrowKey)) history_next(1);

        // Autocomplete:
        int autocomplete_dir = is_held(keyboard?.shiftKey) ? -1 : 1;
        if (ui_input_field.text.Length > 0 && was_pressed(keyboard?.tabKey)) autocomplete_next(autocomplete_dir);

        // Word deletion:
        if (is_held(keyboard?.ctrlKey) && was_pressed(keyboard?.backspaceKey)) input_delete_word(-1);
        if (is_held(keyboard?.ctrlKey) && was_pressed(keyboard?.deleteKey))    input_delete_word(1);

        // Delete on [Ctrl+C]:
        if (is_held(keyboard?.ctrlKey) && was_pressed(keyboard?.cKey)) set_input_field_text(null);

        // Filtering:
        UPDATE_Filtering();
    }
}
