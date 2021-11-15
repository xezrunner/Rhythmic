using static Logger;

public partial class DebugConsole : DebugCom
{
    public bool SubmitInput(string input)
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
                split_args[i - 1] = split_input[i];
        }

        return _ExecuteCommand(split_input[0], split_args);
    }

    public bool _ExecuteCommand(string command, string[] args)
    {
        for (int i = 0; i < cmdsystem.commands_count; ++i)
        {
            ConsoleCommand cmd = cmdsystem.registered_commands[i];
            if (cmd.command == command || cmd.aliases.Contains(command))
            {
                cmd.Invoke(args);
                return true;
            }
        }

        LogE("Failed to find command '%'.".T(this), command);
        return false;
    }
    public static bool ExecuteCommand(string command, string[] args)
    {
        if (Instance) return Instance._ExecuteCommand(command, args);
        return false;
    }
}
