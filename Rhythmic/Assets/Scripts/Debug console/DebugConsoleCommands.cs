using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Logging;

public enum ConsoleCommandType { Function, Variable }
public abstract class ConsoleCommand {
    public ConsoleCommandType command_type;
    public string[] aliases;
    public string   help_text;
    public bool     is_cheat_command = false;
}

// <!> CLEANUP REQUIRED:
public class ConsoleCommand_Func : ConsoleCommand {
    public ConsoleCommand_Func(Action action, string help_text = null, string[] aliases = null) {
        command_type = ConsoleCommandType.Function;
        is_params = false;
        action_empty = action;
        this.help_text = help_text;
        this.aliases = aliases;
    }
    public ConsoleCommand_Func(Action<string[]> action, string help_text = null, string[] aliases = null) {
        command_type = ConsoleCommandType.Function;
        is_params = true;
        action_params = action;
        this.help_text = help_text;
        this.aliases = aliases;
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

    int registered_command_count = 0;
    public bool register_command_internal(ConsoleCommand command, string help_text, string[] aliases) {
        if (command == null && log_warn("attempting to register null command!")) return false;
        if ((aliases == null || aliases.Length == 0) && log_warn("no aliases!")) return false;
        command.help_text = help_text;
        foreach (string alias in aliases) {
            if (alias.is_empty()) continue;
            
            if (registered_commands.ContainsKey(alias)) {
                log_warn("The alias '%' is already registered. Ignoring!".interp(alias));
                continue;
            }
            registered_commands.Add(alias, command);
        }
        ++registered_command_count;
        return true;
    }

    public static bool register_command(ConsoleCommand command, string help_text = null, params string[] aliases) {
        return get_instance().register_command_internal(command, help_text, aliases);
    }

    public static bool COMMANDS_AlwaysAddFuncNames = true;
    public static string[] register_command_func_handle_aliases(MethodInfo info, string[] aliases) {
        if (COMMANDS_AlwaysAddFuncNames || aliases.Length == 0) {
            // Allocate new space for the function name:
            string[] new_aliases = new string[aliases.Length + 1];
            // Add the function name as an alias:
            new_aliases[^1] = info.Name;
            // Remove the "cmd_" prefix if exists:
            if (new_aliases[^1].StartsWith("cmd_")) new_aliases[^1] = new_aliases[^1][4 ..];
            // Copy the remaining aliases:
            aliases.CopyTo(new_aliases, 0);
            return new_aliases;
        }
        return aliases;
    }
    public static bool register_command(Action action, string help_text = null, params string[] aliases) {
        aliases = register_command_func_handle_aliases(action.Method, aliases);
        ConsoleCommand_Func command = new(action, help_text, aliases);
        return get_instance().register_command_internal(command, help_text, command.aliases);
    }
    public static bool register_command(Action<string[]> action, string help_text = null, params string[] aliases) {
        aliases = register_command_func_handle_aliases(action.Method, aliases);
        ConsoleCommand_Func command = new(action, help_text, aliases);
        return get_instance().register_command_internal(command, help_text, command.aliases);
    }

    // ----- //

    void register_builtin_commands() {
        register_command(cmd_help, "Lists all commands.");
        register_command(cmd_clear, "Deletes all of the text from the console.");
        register_command(cmd_toggle_line_categories, "Toggles the category button visibility next to console lines.");
        register_command(cmd_filter, "Filter by a log level category.");
    }

    void cmd_clear() {
        for (int i = ui_lines.Count - 1; i >= 0; --i) destroy_line(i);
    }
    void cmd_help(string[] args) {
        bool is_help      = false;
        bool is_cmd_help  = false;
        bool show_hashes  = false;
        bool show_aliases = false;
        if (args != null) {
            foreach (string s in args) {
                if      (s.Contains("?") || s.Contains("help")) is_help = true;
                else if (s.Contains("hash"))  show_hashes = true;
                else if (s.Contains("alias")) show_aliases = true;
                // Clashes with command help:
                //else     write_line_internal("invalid option: %".interp(s));
                else is_cmd_help = true;
            }
        }

        if (is_help) {
            write_line_internal("--------------------------------------------------------------------");
            write_line_internal("[help ?]             :: print help for the help command");
            write_line_internal("[help command]       :: print the help text for a specific command");
            write_line_internal("[help]               :: lists out all of the registered commands");
            write_line_internal("[help opt1 opt2 ...] :: same as above, but prints additional details");
            write_line_internal("options: ");
            write_line_internal("  - alias: prints out possible aliases for commands");
            write_line_internal("  - hash:  prints out the hash for each registered command entry");
            return;
        }

        if (is_cmd_help && args.Length >= 1) {
            // write_line_internal("attempting to invoke command '%' with the parameter '?'...".interp(args[0]));
            submit("% %".interp(args[0], "?"));
            return;
        }
        
        write_line_internal("Listing all commands: (%)".interp(registered_command_count));
        int prev_hash = -1;
        foreach (var kv in registered_commands) {
            ConsoleCommand cmd = kv.Value;
            int cmd_hash = cmd.GetHashCode();

            // We already print each alias ourselves. Ignore "duplicates":
            if (cmd_hash == prev_hash) continue;

            string s_help_text = null;
            if (!cmd.help_text.is_empty()) s_help_text = $" :: {cmd.help_text}";

            int longest_key_length = registered_commands.Keys.Max(k => k.Length);
            string s_aliases = null;
            string s_alias   = cmd.aliases[0].PadRight(longest_key_length);
            if (show_aliases && cmd.aliases.Length > 1) {
                s_aliases = $" :: aliases: [{string.Join("; ", cmd.aliases, 1, cmd.aliases.Length - 1)}]";
            }

            string s_hash = show_hashes ? $" [{cmd_hash:X8}]" : null;

            write_line_internal("  - %%%".interp(s_alias, s_hash, !show_aliases ? s_help_text : null, s_aliases));

            prev_hash = cmd_hash;
        }
    }
    void cmd_toggle_line_categories() => get_instance().CONSOLE_ShowLineCategories = !get_instance().CONSOLE_ShowLineCategories;
    void cmd_filter(string[] args) {
        if (args.Length == 0) {
            write_line("current filter: %".interp(current_filter), LogLevel._ConsoleInternal);
            return;
        }
        if (args[0] == "?") {
            write_line_internal("------------------------------------------------------------------------------------");
            write_line_internal("[filter ?]     :: print help for the filter command");
            write_line_internal("[filter level] :: filter the console entries by a specific log level");
            write_line_internal("                  (case-insensitive, accepts partial input - ex. \"warn\", \"err\")");
            write_line_internal("valid levels: [%]".interp(string.Join(", ", Enum.GetNames(typeof(LogLevel)))));
            return;
        }

        string s_level = args[0].ToLower();
        string s_match = null;
        foreach (string s in Enum.GetNames(typeof(LogLevel))) {
            if (s.ToLower().Contains(s_level)) s_match = s;
        }
        if (s_match.is_empty() && log_error("invalid log level: %".interp(s_level))) return;

        LogLevel level = (LogLevel)Enum.Parse(typeof(LogLevel), s_match);
        filter(level);
    }

}