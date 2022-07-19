using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static Logging;

public enum DebugMenuEntryType { None = 0, Function = 1, Variable = 2, Separator = 3 }
public class DebugMenuEntry {
    public DebugMenuEntry() { }
    public DebugMenuEntry(string text, string help_text = null, bool is_cheat_entry = false) {
        this.text = text;
        this.help_text = help_text;
        this.is_cheat_entry = is_cheat_entry;
    }

    public DebugMenuEntryType entry_type = DebugMenuEntryType.None;
    public string text;
    public string help_text;
    public bool   is_cheat_entry;
}
public class DebugMenuEntry_Func : DebugMenuEntry {
    public DebugMenuEntry_Func() {
        entry_type = DebugMenuEntryType.Function;
    }
    public DebugMenuEntry_Func(string text, Action action) : this() {
        this.text = text;
        this.action = action;
    }
    public Action action;
    public void invoke() {
        if (action == null && log_warn("no action for entry with text '%'.".interp(text))) return;
        action.Invoke();
    }
}
public class DebugMenuEntry_Var : DebugMenuEntry {
    public DebugMenuEntry_Var() {
        entry_type = DebugMenuEntryType.Variable;
    }
    public DebugMenuEntry_Var(string text, Ref var_ref) : this() {
        this.text = text;
        this.var_ref = var_ref;
    }
    public Ref var_ref;
    public object get_value()             => var_ref.get_value();
    public void   set_value(object value) => var_ref.set_value(value);
}

public class DebugMenu_Line : MonoBehaviour, IPointerUpHandler
{
    public Transform  trans;
    public GameObject self;
    
    [SerializeField] TMP_Text ui_text;

    public Selectable ui_selectable;

    public bool is_separator() {
        return entry.entry_type == DebugMenuEntryType.Separator;
    }

    public bool is_hovering = false;
    public bool is_selected = false;

    public DebugMenuEntry entry;

    public void set_text(string text) => ui_text.SetText(text);
    public void set_selected(bool state) {
        if (state) ui_text.color = Color.red;
        else       ui_text.color = Color.white;
        is_selected = state;
    }

    public event EventHandler<(DebugMenu_Line line, int dir)> clicked;

    // int click_counter = 0;
    public void OnPointerUp(PointerEventData eventData) {
        // if (click_counter < 10) set_text("I've been clicked % times.".interp(++click_counter));
        // else set_text("...what is this, Cookie Clicker? % times...".interp(++click_counter));
        int dir = 0;
        if      (eventData.button == PointerEventData.InputButton.Left)  dir =  1;
        else if (eventData.button == PointerEventData.InputButton.Right) dir = -1;
        clicked?.Invoke(null, (this, dir));
    }
}