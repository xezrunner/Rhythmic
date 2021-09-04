using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Logger;

public partial class DebugConsole : DebugCom
{
    public bool ProcessInput(string input)
    {
        FocusInputField(); // Unity drops focus on submit by default.

        string[] split_input = input.Split(' ');

        for (int i = 0; i < cmdsystem.commands_count; ++i)
        {
            ConsoleCommand cmd = cmdsystem.registered_commands[i];
            if (cmd.command == split_input[0] && Log("Found: [%] %", i, cmd.command)) return true;
        }

        LogE("Failed to find command '%'.".T(this), split_input[0]);
        return false;
    }
}
