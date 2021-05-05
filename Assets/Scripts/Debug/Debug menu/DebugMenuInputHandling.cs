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
    ButtonControl[] OpenClose_Gamepad;

    ButtonControl[] Submit_Buttons;
    ButtonControl[] Decrease_Buttons;
    ButtonControl[] Increase_Buttons;

    // TODO: Analog sticks?                
    ButtonControl[] NavigateUp_Buttons;
    ButtonControl[] NavigateDown_Buttons;

    ButtonControl[] HistoryBack_Buttons;
    ButtonControl[] HistoryForwards_Buttons;

    // HACK HACK HACK!!!
    void InitInput()
    {
        if (Gamepad != null)
        {
            OpenClose_Gamepad = new ButtonControl[] { Gamepad.leftTrigger, Gamepad.leftShoulder }; // TODO: Fix gamepad not being connected

            Submit_Buttons = new ButtonControl[] { Keyboard.spaceKey, Keyboard.yKey, Gamepad.buttonWest, Gamepad.buttonSouth };
            Decrease_Buttons = new ButtonControl[] { Keyboard.digit1Key, Gamepad.dpad.left };
            Increase_Buttons = new ButtonControl[] { Keyboard.digit2Key, Gamepad.dpad.right };

            NavigateUp_Buttons = new ButtonControl[] { Keyboard.oKey, Gamepad.dpad.up, Gamepad.leftStick.up };
            NavigateDown_Buttons = new ButtonControl[] { Keyboard.uKey, Gamepad.dpad.down, Gamepad.leftStick.down };

            HistoryBack_Buttons = new ButtonControl[] { Keyboard.pageUpKey, Gamepad.buttonEast };
            HistoryForwards_Buttons = new ButtonControl[] { Keyboard.pageDownKey, Gamepad.rightShoulder };
        }
        else
        {
            Submit_Buttons = new ButtonControl[] { Keyboard.spaceKey, Keyboard.yKey };
            Decrease_Buttons = new ButtonControl[] { Keyboard.digit1Key };
            Increase_Buttons = new ButtonControl[] { Keyboard.digit2Key };

            NavigateUp_Buttons = new ButtonControl[] { Keyboard.oKey };
            NavigateDown_Buttons = new ButtonControl[] { Keyboard.uKey };

            HistoryBack_Buttons = new ButtonControl[] { Keyboard.pageUpKey };
            HistoryForwards_Buttons = new ButtonControl[] { Keyboard.pageDownKey };
        }
    }

    #endregion

    // Processing & logic:

    float f1_held_ms = 0.0f;
    float f1_held_threshold = 1000f; // 1 second
    bool f1_held = false;

    bool gamepad_ts_held = false;

    void ProcessKeys()
    {
        // TODO: The game should handle Gamepads not being connected!
        bool gamepad_ts_held_now = ArePressed(OpenClose_Gamepad);

        // Holding down the Open key  @Hold
        // Show help message for debug menu when key is held
        {
            // Check whether we're holding the Open keys
            if (!f1_held && (IsPressed(Open_Key)/* || gamepad_ts_held_now)*/))
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
            if ((WasReleased(Open_Key) || !gamepad_ts_held_now) && f1_held) { f1_held = false; return; }
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
        if (WasReleased(Open_Key))
        {
            if (IsPressed(Keyboard.ctrlKey) || IsPressed(Keyboard.altKey)) return;
            if (IsPressed(Keyboard.shiftKey)) { MainMenu(); return; }

            if (IsActive) Logger.Log("[Help] " + $"{SelectedEntry.Text}: ".AddColor(Colors.Application) +
                          $"{(SelectedEntry.HelpText != null ? SelectedEntry.HelpText : "No help text for this entry.".AddColor(Colors.Unimportant))}");
            else SetActive(true);
        }
        else if (WasPressed(Close_Key)) SetActive(false);

        if (!IsActive) return;
        // Debug menu controls:

        // Move through entries:
        // U: Move down ; O: Move up
        if (WasPressed(NavigateUp_Buttons)) Entry_Move(DebugMenuEntryDir.Up);
        else if (WasPressed(NavigateDown_Buttons)) Entry_Move(DebugMenuEntryDir.Down);
        else if (WasPressed(Keyboard.homeKey)) Entry_Move(DebugMenuEntryDir.Home); //  @Hardcode
        else if (WasPressed(Keyboard.endKey)) Entry_Move(DebugMenuEntryDir.End);   //  @Hardcode

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