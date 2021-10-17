using static Logger;

public partial class DebugConsole : DebugCom
{
    public bool DEBUGCONSOLE_Submit(string input)
    {
        InputField_Focus(); // Unity drops focus on submit by default.
        InputField_Clear();
        UI_ScrollConsole();
        ConsoleLog("> %".AddColor(0.8f), input);
        History_Add(input);

        if (input.IsEmpty()) return false;

        string[] split_input = input.Split(' ');

        string[] split_args = null;
        if (split_input.Length > 1)
        {
            // TODO (cleanup): do this in a better way
            split_args = new string[split_input.Length - 1];
            for (int i = 1; i < split_input.Length; ++i)
                split_args[i] = split_input[i];
        }

        for (int i = 0; i < cmdsystem.commands_count; ++i)
        {
            ConsoleCommand cmd = cmdsystem.registered_commands[i];
            if (cmd.command == split_input[0])
            {
                cmd.Invoke(split_args);
                return true;
            }
        }

        LogE("Failed to find command '%'.".T(this), split_input[0]);
        return false;
    }
}
