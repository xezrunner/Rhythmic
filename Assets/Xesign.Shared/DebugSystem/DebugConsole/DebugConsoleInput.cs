using System;
using UnityEngine.InputSystem;

public partial class DebugConsole : DebugCom
{
    // Input field: 
    [NonSerialized] public string Input_Text = "";

    public void OnInputChanged()
    {

    }
    public void OnInputEditingEnd()
    {

    }

    // Input (keys):
    Keyboard Keyboard = Keyboard.current;
    void UPDATE_Input()
    {
        if (Keyboard.digit0Key.wasPressedThisFrame || Keyboard.backquoteKey.wasPressedThisFrame) _Open();
        if (Keyboard.escapeKey.wasPressedThisFrame) _Close();
        if (Keyboard.tabKey.wasPressedThisFrame) ChangeSize(!is_compact);
    }
}
