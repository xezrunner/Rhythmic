using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using static Logger;

public enum WordDeleteDir { Left = 0, Right = 1 }

public partial class DebugConsole : DebugCom
{
    // Input field: 
    [NonSerialized] public string Input_Text = "";

    public void OnInputChanged()
    {
        // If we aren't open, do not accept new input, as Unity sometimes doesn't properly unfocus elements:
        if (!is_open)
        {
            Input_Field.SetTextWithoutNotify(Input_Text);
            return;
        }

        Input_Text = Input_Field.text;

        // tba
    }
    public void OnInputEditingEnd() { }

    public void InputField_ChangeText(string text, int new_caret = -1) => InputField_ChangeText(text, true, new_caret);
    public void InputField_ChangeText(string text, bool change_caret, int new_caret = -1)
    {
        Input_Field.text = text;
        if (change_caret)
            Input_Field.caretPosition = (new_caret == -1 ? text.Length : new_caret);
    }
    void InputField_WordDelete(WordDeleteDir dir)
    {
        if (Input_Text.IsEmpty()) return;

        int caret_pos = Input_Field.caretPosition;

        // TODO: We should use arrays here, probably. Performance!
        List<string>[] split = new List<string>[2]
        {
            Input_Text.Substring(0, caret_pos).Split(' ').ToList(),
            Input_Text.Substring(caret_pos, Input_Text.Length - caret_pos).Split(' ').ToList()
        };

        if (dir == WordDeleteDir.Left && split[0].Count > 0)
            split[0].RemoveAt(split[0].Count - 1);
        else if (split[1].Count > 0)
            split[1].RemoveAt(0);
        else
        {
            LogW("There weren't enough words in split array!".TM(this));
            return;
        }

        // Reconstruct string:
        string result = string.Join(' ', split[0]) + string.Join(' ', split[1]);
        InputField_ChangeText(result, false);
    }

    // ----- //

    // Input (keys):
    Keyboard Keyboard = Keyboard.current;
    bool keyboard_missing_warned;
    void UPDATE_Input()
    {
        if (Keyboard == null && !keyboard_missing_warned) LogW("No keyboard was found.".T(this));
        if (Keyboard == null) return;

        // Open, close, size:
        if (Keyboard.digit0Key.wasPressedThisFrame || Keyboard.backquoteKey.wasPressedThisFrame)
            _Open();
        if (Keyboard.escapeKey.wasPressedThisFrame)
            _Close();
        if (Keyboard.tabKey.wasPressedThisFrame)
            ChangeSize(!is_compact);

        // Input field extras:
        // Word delete:
        if (Keyboard.ctrlKey.isPressed && Keyboard.backspaceKey.wasPressedThisFrame)
            InputField_WordDelete(WordDeleteDir.Left);
        if (Keyboard.ctrlKey.isPressed && Keyboard.deleteKey.wasPressedThisFrame)
            InputField_WordDelete(WordDeleteDir.Right);
    }
}
