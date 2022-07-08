using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using static Logging;
using static QuickInput;
using static AnimEasing;

public class DebugConsole : MonoBehaviour {
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
        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false; // @SRPDebugCanvas WordDeleteClash
    }
    void Start() {
        // Start closed:
        if (!is_open) {
            is_open = true;
            close(false);
        }
        
        sizing_y = (CONSOLE_Height, CONSOLE_Height);
        ui_lines = new(capacity: CONSOLE_MaxLines);

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
    public static float CONSOLE_DefaultHeight    = 270f;
    public float   CONSOLE_Height                = 270f;
    public int     CONSOLE_MaxLines              = 300;

    public bool    CONSOLE_AllowSubmitRepetition = false;
    public float   CONSOLE_RepeatHoldTime        = 380f;
    public float   CONSOLE_RepeatHoldDelay       = 10f;

    public float   CONSOLE_AnimSpeed       = 3f;
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
        else close();
        return is_open;
    }

    void set_openness_anim((float, float) y_pos, float t = 0) {
        openness_y_pos    = y_pos;
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
        if (!expanded) {
            if (height > 30f) CONSOLE_Height = height;
            sizing_y = (ui_panel.sizeDelta.y, CONSOLE_Height);
        } else 
            sizing_y = (ui_panel.sizeDelta.y, ui_canvas.rect.height);

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
        // float w_pos = ease_out_quadratic(sizing_w.from, sizing_w.to, sizing_t);
        // ui_panel.sizeDelta = new(w_pos, ui_panel.sizeDelta.y);

        sizing_t += CONSOLE_AnimSpeed * Time.unscaledDeltaTime;

        scroll_to_bottom(false);
    }

    // Scrolling:
    public const float SCROLL_TOP = 1f;
    public const float SCROLL_BOTTOM = 0f;

    bool is_scrolling = false;
    float scroll_target;
    float scroll_t = 1f;
    void UPDATE_ScrollRequest() {
        if (!is_scrolling) return;
        // Cancel scrolling animations when scrolling with mouse wheel:
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
        ui_input_field.SetTextWithoutNotify(null);
    }

    void write_line_internal(string message, LogLevel level) {
        add_new_line(message);
    }

    public static void write_line(string message, LogLevel level = LogLevel.Info) {
        get_instance()?.write_line_internal(message, level);
    }

    // Processing & commands:
    void submit(string input = null) {
        // Assume we want to submit the input field text when not given a parameter:
        if (input == null) {
            input = ui_input_field.text;
            focus_input_field();
            if (!CONSOLE_AllowSubmitRepetition) clear_input_field();
        }
        log("input: '%'".interp(input));
        scroll_to_bottom();
    }

    int repeated_submits_count = 0;
    float submit_hold_timer_ms = 0f;
    void UPDATE_HandleSubmitRepetition() {
        if (!CONSOLE_AllowSubmitRepetition) return;
        // If we keep holding a submit key, repeatedly submit after a delay:
        if (is_pressed(keyboard?.enterKey, keyboard?.numpadEnterKey)) {
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
        
        if (was_pressed (keyboard?.enterKey, keyboard?.numpadEnterKey)) submit();
        if (CONSOLE_AllowSubmitRepetition && was_released(keyboard?.enterKey, keyboard?.numpadEnterKey))
            clear_input_field();

        UPDATE_HandleSubmitRepetition();
        UPDATE_ScrollRequest();

        // TODO: Clash with autocomplete!
        if (was_pressed(keyboard?.tabKey)) toggle_expanded();
        UPDATE_Sizing();
    }
}
