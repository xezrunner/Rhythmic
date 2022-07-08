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
    }
    void Start() {
        // Start closed:
        if (!is_open) {
            is_open = true;
            close(false);
        }
        
        ui_lines = new(capacity: CONSOLE_MaxLines);
    }

    GameObject self;
    Keyboard keyboard;

    [Header("State")]
    public bool is_open     = false;
    public bool is_expanded = false;

    [Header("UI objects")]
    [SerializeField] RectTransform  ui_self;
    [SerializeField] Transform      ui_text_container;
    [SerializeField] ScrollRect     ui_scroll_rect;
    [SerializeField] TMP_InputField ui_input_field;

    [Header("Prefabs")]
    [SerializeField] TMP_Text prefab_ui_line;

    [Header("Options")]
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
        set_openness_anim((ui_self.localPosition.y, 0), default, anim ? 0 : 1);
        is_open = true;
        ui_input_field.interactable = true;
        focus_input_field();
        // ...
    }

    public void close(bool anim = true) {
        if (!is_open) return;
        set_openness_anim((ui_self.localPosition.y, CONSOLE_Height), default, anim ? 0 : 1);
        is_open = false;
        ui_input_field.interactable = false;
        // ...
    }

    public bool toggle() {
        if (!is_open) open();
        else close();
        return is_open;
    }

    void set_openness_anim((float, float) y, (float, float) w = default, float t = 0) {
        openness_y = y;
        openness_w = w;
        openness_t = t;
    }

    float openness_t;
    (float from, float to) openness_y;
    (float from, float to) openness_w;
    public void UPDATE_Animation() {
        if (openness_t > 1f) return;

        float y = ease_out_quadratic(openness_y.from, openness_y.to, openness_t);
        ui_self.localPosition = new(ui_self.localPosition.x, y);
        // TODO: Width animation!
        //float w = Mathf.Lerp(anim_w.from, anim_w.to, anim_t);
        //ui_self.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        openness_t += CONSOLE_AnimSpeed * Time.unscaledDeltaTime;
    }

    // Scrolling:
    public const float SCROLL_TOP = 1f;
    public const float SCROLL_BOTTOM = 0f;

    bool is_scrolling = false;
    float scroll_target;
    float scroll_t;
    void UPDATE_ScrollRequest() {
        if (!is_scrolling) return;

        ui_scroll_rect.verticalNormalizedPosition = 
            ease_out_quadratic(ui_scroll_rect.verticalNormalizedPosition, scroll_target, scroll_t);
        scroll_t += Time.unscaledDeltaTime * CONSOLE_ScrollAnimSpeed;

        if (scroll_t > 1f) is_scrolling = false;
    }
    public void scroll_console(float value) {
        scroll_t = 0;
        scroll_target = value;
        is_scrolling = true;
    }
    public void scroll_to_top()    => scroll_console(SCROLL_TOP);
    public void scroll_to_bottom() => scroll_console(SCROLL_BOTTOM);

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
        UPDATE_Animation();

        // Do not allow toggling with the [0 / backtick] key - it would clash with wanting to input '0':
        if      (!is_open && was_pressed(keyboard?.digit0Key, keyboard?.backquoteKey)) open();
        else if  (is_open && was_pressed(keyboard?.escapeKey)) close();

        if (!is_open) return;
        
        if (was_pressed (keyboard?.enterKey, keyboard?.numpadEnterKey)) submit();
        if (CONSOLE_AllowSubmitRepetition && was_released(keyboard?.enterKey, keyboard?.numpadEnterKey))
            clear_input_field();

        UPDATE_HandleSubmitRepetition();
        // Cancel scrolling animations when scrolling with mouse wheel
        if (Mouse.current != null && Mouse.current.scroll.y.ReadValue() != 0) {
            // TODO: We probably don't actually want to cancel the previous scroll request.
            // is_scrolling = false;
        }
        else UPDATE_ScrollRequest();
    }
}
