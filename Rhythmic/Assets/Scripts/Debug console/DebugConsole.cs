using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using static Logging;
using static QuickInput;
using static AnimEasing;
using System.Linq;

public partial class DebugConsole : MonoBehaviour {
    static DebugConsole instance;
    public static DebugConsole get_instance() {
        if (instance) return instance;
        log_warn("DebugConsole does not have an instance!");
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
    }
    void Start() {
        // Start closed:
        if (!is_open) {
            is_open = true;
            close(false);
        }
        
        sizing_y = (CONSOLE_Height, CONSOLE_Height);
        ui_lines = new(capacity: CONSOLE_MaxLines);
        history = new(CONSOLE_MaxHistoryEntries);

        register_builtin_commands();

        write_line("[console] initialized");
    }

    RectTransform ui_canvas;
    GameObject self;
    Keyboard keyboard;

    [Header("State")]
    public bool is_open     = false;
    public bool is_expanded = false;

    [Header("UI objects")]
    [SerializeField] RectTransform  ui_panel;
    [SerializeField] Transform      ui_text_container;
    [SerializeField] ScrollRect     ui_scroll_rect;
    [SerializeField] TMP_InputField ui_input_field;

    [Header("Prefabs")]
    [SerializeField] TMP_Text prefab_ui_line;

    
    [Header("Options")]
    [Tooltip("The default height of the console. [Do not change dynamically!]")]
    public float   CONSOLE_DefaultHeight         = 270f;
    [Tooltip("The current height of the console. Used in openness and sizing.")]
    public float   CONSOLE_Height                = 270f;
    [Tooltip("The threshold for amount of lines in the console where if exceeded, oldest entries will start being removed.\n" +
             "Use -1 or below to allow infinite entries.")]
    public int     CONSOLE_MaxLines              = 300;

    [Tooltip("Controls whether the console should write the input into itself upon submission.")]
    public bool    CONSOLE_EchoBack              = true;
    [Tooltip("Controls how many input submissions we want to store in the history buffer." + 
             "Oldest entries will start being removed once exceeded.")]
    public int     CONSOLE_MaxHistoryEntries     = 100;

    [Tooltip("Controls whether you can autocomplete commands in the console using [(Shift +)Tab].")]
    public bool    CONSOLE_EnableAutocomplete    = true;

    [Tooltip("Controls whether the submit keys can be held down to repeatedly submit input to the console.")]
    public bool    CONSOLE_AllowSubmitRepetition = false;
    public float   CONSOLE_RepeatHoldTime        = 380f;
    public float   CONSOLE_RepeatHoldDelay       = 10f;

    [Tooltip("Controls the animation speed for openness and sizing.")]
    public float   CONSOLE_AnimSpeed       = 3f;
    [Tooltip("Controls the animation speed for scrolling (when the scroll animation is requested).")]
    public float   CONSOLE_ScrollAnimSpeed = 3f;
    
    List<TMP_Text> ui_lines                = new();

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

    // Lines:
    TMP_Text add_new_line(string text) {
        TMP_Text com = Instantiate(prefab_ui_line);
        com.transform.SetParent(ui_text_container, false);

        com.SetText(text);
        ui_lines.Add(com);

        if (ui_lines.Count > CONSOLE_MaxLines) {
            Destroy(ui_lines[0].gameObject);
            ui_lines.RemoveAt(0);
        }

        return com;
    }

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

    void input_delete_word(int dir) // -1: left | 1: right
        {
        if (ui_input_field.text == "") return;
        if (ui_input_field.text.Length == 1) return;

        // TODO: Do we still need to fiddle with the caret?
        int caret_position = ui_input_field.caretPosition;

        string s0 = ui_input_field.text[.. caret_position]; // 0 -> *| ...
        string s1 = ui_input_field.text[caret_position ..];

        if (dir == -1) // Delete from caret to the left (one word)
        {
            if (caret_position == 0) return;

            string[] tokens = s0.Split(' ');
            if (tokens.Length > 0) tokens[^1] = "";

            s0 = string.Join(" ", tokens);
            if (!s0.is_empty() && s0[^1] == ' ') s0 = s0[..^1];
            caret_position = s0.Length;
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
    int autocomplete_index = -1;
    List<string> autocomplete_list = new();
    void autocomplete_next(int dir) {
        if (!CONSOLE_EnableAutocomplete)  return;
        if (autocomplete_list == null && log_error("autocomplete_list is null!")) return;
        if (autocomplete_list.Count == 0) return;

        autocomplete_index += dir;
        if      (autocomplete_index >= autocomplete_list.Count) autocomplete_index = 0;
        else if (autocomplete_index < 0) autocomplete_index = autocomplete_list.Count - 1;

        set_input_field_text(autocomplete_list[autocomplete_index], notify: false);
    }

    bool autocomplete_list_debug = true;
    void build_autocomplete_list(string input = null) {
        if (!CONSOLE_EnableAutocomplete) return;

        if (input == null) input = ui_input_field.text;
        if (input.is_empty()) return;

        autocomplete_list.Clear();
        
        // Check whether the whole input itself is contained in the registered commands list:
        if (registered_commands.Keys.Contains(input)) autocomplete_list.Add(input);
        // Find all other entries:
        foreach (string key in registered_commands.Keys) {
            if (key == input) continue;
            if (key.StartsWith(input)) autocomplete_list.Add(key);
        }

        if (autocomplete_list_debug) write_line("Autocomplete: [%]".interp(string.Join("; ", autocomplete_list)));
    }
    void build_autocomplete_ui() {
        if (!CONSOLE_EnableAutocomplete) return;
    }
    public void input_field_value_changed() {
        build_autocomplete_list();
        build_autocomplete_ui();
    }

    // Processing & commands:
    void write_line_internal(string message, LogLevel level) {
        add_new_line(message);
    }

    public static void write_line(string message, LogLevel level = LogLevel.Info) {
        get_instance()?.write_line_internal(message, level);
    }
    
    void submit(string input = null) {
        // Assume we want to submit the input field text when not given a parameter:
        if (input == null) {
            input = ui_input_field.text;
            // Re-focus the input field upon submit:
            focus_input_field();
            // If submit repetition is not allowed, clear the input field upon submit:
            if (!CONSOLE_AllowSubmitRepetition) clear_input_field();
        }
        if (CONSOLE_EchoBack) write_line("> %".interp(input));
        history_add(input);
        scroll_to_bottom();

        if (!registered_commands.ContainsKey(input)) {
            write_line("Could not find command: %".interp(input));
            return;
        }

        ConsoleCommand cmd = registered_commands[input];
        if (cmd.command_type == ConsoleCommandType.Function) {
            ConsoleCommand_Func cmd_func = (ConsoleCommand_Func)cmd;
            cmd_func.invoke();
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
            if (repeated_submits_count > 0)
                log("repeatedly submitted % times.".interp(repeated_submits_count));
            repeated_submits_count = 0;
            submit_hold_timer_ms = 0f;
        }
    }
    
    void Update() {
        UPDATE_Openness();

        // Do not allow toggling with the [0 / backtick] key - it would clash with wanting to input '0':
        if      (!is_open && was_pressed(keyboard?.digit0Key, keyboard?.backquoteKey)) open();
        else if  (is_open && was_pressed(keyboard?.escapeKey)) close();

        if (!is_open) return;
        
        // Input submission:
        if (was_pressed (keyboard?.enterKey, keyboard?.numpadEnterKey)) submit();
        if (CONSOLE_AllowSubmitRepetition && was_released(keyboard?.enterKey, keyboard?.numpadEnterKey))
            clear_input_field();

        UPDATE_HandleSubmitRepetition();

        // History navigation:
        if (was_pressed(keyboard?.upArrowKey))   history_next(-1);
        if (was_pressed(keyboard?.downArrowKey)) history_next(1);
        
        UPDATE_ScrollRequest();

        // Autocomplete:
        int autocomplete_dir = is_held(keyboard?.shiftKey) ? -1 : 1;
        if (ui_input_field.text.Length > 0 && was_pressed(keyboard?.tabKey)) autocomplete_next(autocomplete_dir);

        // Sizing:
        if (ui_input_field.text.Length == 0 && was_pressed(keyboard?.tabKey)) toggle_expanded();
        UPDATE_Sizing();

        // Word deletion:
        if (is_held(keyboard?.ctrlKey) && was_pressed(keyboard?.backspaceKey)) input_delete_word(-1);
        if (is_held(keyboard?.ctrlKey) && was_pressed(keyboard?.deleteKey))    input_delete_word(1);

        // Delete on [Ctrl+C]:
        if (is_held(keyboard?.ctrlKey) && was_pressed(keyboard?.cKey)) set_input_field_text(null);
    }
}
