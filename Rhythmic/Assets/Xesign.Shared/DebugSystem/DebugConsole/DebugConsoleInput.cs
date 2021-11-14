using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.InputSystem;
using static Logger;

public enum WordDeleteDir { Left = 0, Right = 1 }

public partial class DebugConsole : DebugCom
{
    Keyboard Keyboard = Keyboard.current;
    void INPUT_Start()
    {
        if (Keyboard == null)
            LogW("No keyboard was found.".T(this));
    }

    // Input field: 
    public TMP_InputField Input_Field;
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

    public void InputField_ChangeText(string text, int new_caret = -1) => InputField_ChangeText(text, true, new_caret);
    public void InputField_ChangeText(string text, bool change_caret, int new_caret = -1)
    {
        Input_Field.text = text;
        if (change_caret)
            Input_Field.caretPosition = (new_caret == -1 ? text.Length : new_caret);
    }

    void InputField_Focus() => Input_Field.ActivateInputField();
    void InputField_Unfocus() => Input_Field.DeactivateInputField();
    void InputField_Clear() => InputField_ChangeText("");

    void HandleWordDelete(WordDeleteDir dir)
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

    // History navigation:
    List<string> History = new List<string>();
    [NonSerialized] public int History_Max = 50;

    public void History_Add(string s)
    {
        if (History == null) return;
        if (History.Count > 0 && History[0] == s) return;

        History.Insert(0, s);
        if (History.Count > History_Max) History.RemoveAt(History.Count - 1);
    }

    int history_index = -1;
    public void History_Walk(int dir)
    {
        if (History == null || History.Count == 0) return;
        history_index += dir;

        if (history_index >= History.Count) history_index = 0;
        else if (history_index < 0) history_index = History.Count - 1;

        InputField_ChangeText(History[history_index]);
    }


    // ----- //

    // Input (keys):
    void UPDATE_Input()
    {
        if (Keyboard == null) return;

        // Open, close, size:
        if (Keyboard.digit0Key.wasPressedThisFrame || Keyboard.backquoteKey.wasPressedThisFrame)
            _Open();
        else if (Keyboard.escapeKey.wasPressedThisFrame)
            _Close();

        if (!is_open) return;

        if (Keyboard.tabKey.wasPressedThisFrame)
            ChangeSize(!is_compact);

        // Submit:
        if (Keyboard.enterKey.wasPressedThisFrame || Keyboard.numpadEnterKey.wasPressedThisFrame)
            SubmitInput(Input_Text);

        // History navigation:
        if (Keyboard.upArrowKey.wasPressedThisFrame) History_Walk(1);
        else if (Keyboard.downArrowKey.wasPressedThisFrame)
        {
            if (history_index == 0) InputField_ChangeText("");
            else History_Walk(-1);
        }

        // Input field extras:
        if (Keyboard.ctrlKey.isPressed)
        {
            // Word delete:
            if (Keyboard.backspaceKey.wasPressedThisFrame) HandleWordDelete(WordDeleteDir.Left);
            else if (Keyboard.deleteKey.wasPressedThisFrame) HandleWordDelete(WordDeleteDir.Right);

            if (Keyboard.homeKey.wasPressedThisFrame) UI_ScrollConsole(SCROLL_TOP);
            else if (Keyboard.endKey.wasPressedThisFrame || Keyboard.tabKey.wasPressedThisFrame) UI_ScrollConsole(SCROLL_BOTTOM);

        }

    }
}
