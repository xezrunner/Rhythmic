using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using static InputHandler;

public partial class DebugMenu
{
    static Keyboard Keyboard = Keyboard.current;
    static Gamepad Gamepad = Gamepad.current;

    #region Keys & buttons
    ButtonControl Open_Key = Keyboard.f1Key;
    ButtonControl Close_Key = Keyboard.f2Key;
    ButtonControl[] OpenClose_Gamepad = { Gamepad.leftTrigger, Gamepad.leftShoulder };

    ButtonControl[] Submit_Buttons = { Keyboard.spaceKey, Keyboard.yKey, Gamepad.buttonWest, Gamepad.buttonSouth};
    ButtonControl[] Decrease_Buttons = { Keyboard.digit1Key, Gamepad.dpad.left };
    ButtonControl[] Increase_Buttons = { Keyboard.digit2Key, Gamepad.dpad.right };

    // TODO: Analog sticks?
    ButtonControl[] NavigateUp_Buttons = { Keyboard.oKey, Gamepad.dpad.up };
    ButtonControl[] NavigateDown_Buttons = { Keyboard.uKey, Gamepad.dpad.down };

    ButtonControl[] HistoryBack_Buttons = { Keyboard.pageUpKey, Gamepad.rightTrigger };
    ButtonControl[] HistoryForwards_Buttons = { Keyboard.pageDownKey, Gamepad.rightShoulder };
    #endregion

    // Processing & logic:

    float f1_held_ms = 0.0f;
    float f1_held_threshold = 1000f; // 1 second
    bool f1_held = false;

    bool gamepad_ts_held = false;

    void ProcessKeys()
    {
        bool gamepad_ts_held_now = ArePressed(OpenClose_Gamepad);

        // Holding down the Open key  @Hold
        // Show help message for debug menu when key is held
        {
            // Check whether we're holding the Open keys
            if (!f1_held && (IsPressed(Open_Key) || gamepad_ts_held_now))
                f1_held_ms += Time.unscaledDeltaTime * 1000;

            // We held the keys successfully!  @Held
            if (f1_held_ms >= f1_held_threshold)
            {
                if (!f1_held) // If we are coming from just being held:
                {
                    f1_held = true; // Consider held, block rest of the input processing
                    f1_held_ms = 0;

                    LogHelp(); return;
                }
            }

            // Reset held state once released
            if ((WasReleased(Keyboard.f1Key) || !gamepad_ts_held_now) && f1_held) { f1_held = false; return; }
        }

        // Gamepad  - Enable & disable
        if (!gamepad_ts_held && gamepad_ts_held_now)
        {
            SetActive(!IsActive);
            gamepad_ts_held = true;
        }
        else if (gamepad_ts_held && !gamepad_ts_held_now)
            gamepad_ts_held = false;

        // Keyboard - Enable & disable | F1: ON ; F2: OFF
        if (WasReleased(Keyboard.f1Key))
        {
            if (IsPressed(Keyboard.ctrlKey) || IsPressed(Keyboard.altKey)) return;
            if (IsPressed(Keyboard.shiftKey)) { MainMenu(); return; }

            if (IsActive) Logger.Log("[Help] " + $"{SelectedEntry.Text}: ".AddColor(Colors.Application) +
                          $"{(SelectedEntry.HelpText != null ? SelectedEntry.HelpText : "No help text for this entry.".AddColor(Colors.Unimportant))}");
            else SetActive(true);
        }
        else if (WasPressed(Keyboard.f2Key)) SetActive(false);

        if (!IsActive) return;
        // Debug menu controls:

        // Move through entries:
        // U: Move down ; O: Move up
        if (WasPressed(NavigateUp_Buttons)) Entry_Move(DebugMenuEntryDir.Up);
        else if (WasPressed(NavigateDown_Buttons)) Entry_Move(DebugMenuEntryDir.Down);
        else if (WasPressed(Keyboard.homeKey)) Entry_Move(DebugMenuEntryDir.Home);
        else if (WasPressed(Keyboard.endKey)) Entry_Move(DebugMenuEntryDir.End);

        // Activate & manipulate entries:
        // Space: enter ; 1-2: Change value of variable entries
        if (WasPressed(Submit_Buttons)) PerformSelectedAction(DebugMenuVarDir.Action);
        else if (WasPressed(Decrease_Buttons)) PerformSelectedAction(DebugMenuVarDir.Decrease);
        else if (WasPressed(Increase_Buttons)) PerformSelectedAction(DebugMenuVarDir.Increase);

        // History navigation | PgUp: backwards ; PgDn: forwards
        if (WasPressed(HistoryBack_Buttons)) NavigateHistory(DebugMenuHistoryDir.Backwards);
        else if (WasPressed(HistoryForwards_Buttons)) NavigateHistory(DebugMenuHistoryDir.Forwards);
    }
}