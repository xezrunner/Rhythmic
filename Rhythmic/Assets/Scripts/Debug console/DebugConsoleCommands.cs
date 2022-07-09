using System;
using System.Collections.Generic;

using static Logging;

public enum ConsoleCommandType { Function, Variable }
public abstract class ConsoleCommand {
    public ConsoleCommandType command_type;
    public string[] aliases;
    public string   help_text;
    public bool     is_cheat_command = false;
}
public class ConsoleCommand_Func : ConsoleCommand {
    public ConsoleCommand_Func(Action action, string help_text = null, string[] aliases = null) {
        command_type = ConsoleCommandType.Function;
        is_params = false;
        action_empty = action;
        this.help_text = help_text;
        if (aliases == null || aliases.Length == 0)
            this.aliases = new string[1] { action.Method.Name };
        else this.aliases = aliases;
    }
    public ConsoleCommand_Func(Action<string[]> action, string help_text = null, string[] aliases = null) {
        command_type = ConsoleCommandType.Function;
        is_params = true;
        action_params = action;
        this.help_text = help_text;
        if (aliases == null || aliases.Length == 0)
            this.aliases = new string[1] { action.Method.Name };
        else this.aliases = aliases;
    }

    public bool is_params = false; // @Hack  This controls whether the function accepts parameters.
    public Action action_empty;
    public Action<string[]> action_params;

    public void invoke(string[] args = null) {
        if (!is_params) action_empty.Invoke();
        else action_params.Invoke(args);
    }
}
public class ConsoleCommand_Value<T> : ConsoleCommand {
    public ConsoleCommand_Value(T value) {
        command_type = ConsoleCommandType.Variable;
        this.value = value;
        type = value.GetType();
    }
    // TODO: REF!!!
    public T value;
    public Type type;
}

public partial class DebugConsole {
    public Dictionary<string, ConsoleCommand> registered_commands = new();

    public bool register_command_internal(ConsoleCommand command, string help_text, string[] aliases) {
        if (command == null && log_warn("attempting to register null command!")) return false;
        if ((aliases == null || aliases.Length == 0) && log_warn("no aliases!")) return false;
        command.help_text = help_text;
        foreach (string alias in aliases) {
            registered_commands.Add(alias, command);
        }
        return true;
    }

    public static bool register_command(ConsoleCommand command, string help_text = null, params string[] aliases) {
        return get_instance().register_command_internal(command, help_text, aliases);
    }
    public static bool register_command(Action action, string help_text = null, params string[] aliases) {
        ConsoleCommand_Func command = new(action, help_text, aliases);
        return get_instance().register_command_internal(command, help_text, command.aliases);
    }
    public static bool register_command(Action<string[]> action, string help_text = null, params string[] aliases) {
        ConsoleCommand_Func command = new(action, help_text, aliases);
        return get_instance().register_command_internal(command, help_text, command.aliases);
    }
}