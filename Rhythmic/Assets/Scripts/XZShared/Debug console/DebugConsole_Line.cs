using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XZShared;
using static Logging;

public class DebugConsole_Line : MonoBehaviour
{
    void Awake() {
        console = DebugConsole.get_instance();
        if (!console) log_error("no console instance!");
        set_category_button_state(false);
    }
    void Start() {
        set_category(info.level);

        // HACK: position the category button onto the first line, in case of multi-line text:
        // TODO: factor out into a function?
        var     c_info  = ui_text.textInfo.characterInfo[0];
        Vector2 c_pos   = ui_text.rectTransform.TransformPoint(c_info.topLeft);
        ui_category_button_rect.position = new Vector2(ui_category_button_rect.position.x, c_pos.y);
    }
    
    DebugConsole console;

    // Cache:
    public Transform  trans;
    public GameObject self;

    public TMP_Text   ui_text;

    public Button        ui_category_button;
    public GameObject    ui_category_button_gameobject;
    public RectTransform ui_category_button_rect;
    public TMP_Text      ui_category_button_text;
    public Image         ui_category_button_image;

    public Logging_Info info;

    public void set_state(bool state) => self.SetActive(state);

    // TODO: remove getters!

    public string get_text()                   => ui_text.text;
    public void set_text(string text = null) {
        // TODO: control!
        if (info.caller_info.has_info) text = text.add_caller_debug_info(logging_options.caller_format_flags, info.caller_info);
        ui_text.SetText(text);
    }

    public LogLevel get_category() => info.level;
    public void     set_category(LogLevel level) {
        set_category_button_text(info.level.ToString()[0].ToString()); // TODO: refine this!
        set_category_button_colors(XZ_GetColorForLogLevel(level));
        set_category_button_state(true);
    }

    public string get_category_button_text()                   => ui_category_button_text.text;
    public void   set_category_button_text(string text = null) => ui_category_button_text.SetText(text);

    public Color copy_color_alpha(Color color, float a) => new(color.r, color.g, color.b, a);
    public Color copy_color_add(Color color, float add) => new(color.r + add, color.g + add, color.b + add, color.a);

    public void set_category_button_colors(Color color, bool is_text_white = true) {
        ColorBlock new_colors = ui_category_button.colors;
        new_colors.normalColor = color;
        new_colors.selectedColor = color;
        new_colors.highlightedColor = copy_color_add(color, 0.1f);
        new_colors.pressedColor = copy_color_add(color, -0.1f);
        new_colors.disabledColor = copy_color_add(color, -0.8f);
        ui_category_button.colors = new_colors;

        ui_category_button_text.color = is_text_white ? Color.white : "#272727".hex_to_unity_color();
    }

    // NOTE: This does not represent the current state of the button.
    // This is the "local" state of the button, without taking into the account whether these buttons are
    // globally disabled within the application.
    // This can be true, while the button will still not be visible. See update_category_button_state().
    public bool self_is_active = true;
    public bool set_category_button_state(bool new_state) {
        self_is_active = new_state;
        update_category_button_state();
        return new_state;
    }

    public static Enum[] category_buttons_hide_flags = { LogLevel._ConsoleInternal, LogLevel._IgnoreFiltering };
    void update_category_button_state() {
        bool new_state = self_is_active;
        if (!console.CONSOLE_ShowLineCategories)   new_state = false;
        if (info.level == LogLevel.None)             new_state = false;
        // TODO: Preferably, we'll want to have a list/panel (UI) where we can select _ConsoleInternal,
        // while not having the line button visible for this specific and other internal categories.
        if (info.level.HasFlag_Any(category_buttons_hide_flags)) new_state = false;

        ui_category_button_gameobject.SetActive(new_state);
        ui_text.margin = !new_state ? Vector4.zero : new(ui_category_button_rect.sizeDelta.x + 6f,0f,0f,0f);
    }

    public event EventHandler<LogLevel> category_button_clicked_event;
    public void category_button_clicked() {
        category_button_clicked_event?.Invoke(null, info.level);
    }

    void LateUpdate() {
        update_category_button_state();
    }
}
