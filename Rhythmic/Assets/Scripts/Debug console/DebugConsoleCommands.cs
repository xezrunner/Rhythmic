using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

using static Logging;

public enum ConsoleCommandType { Function, Variable }
public abstract class ConsoleCommand {
    public ConsoleCommand(ConsoleCommandAttribute attrib = null) {
        if (attrib == null) attrib = new();
        help_text        = attrib.help_text;
        is_cheat_command = attrib.is_cheat_command;
    }
    public ConsoleCommandType command_type;
    public string[] aliases;
    public string   help_text;
    public bool     is_cheat_command = false;
}

public class ConsoleCommand_Func : ConsoleCommand {
    ConsoleCommand_Func(ConsoleCommandAttribute attrib = null) : base(attrib) {
        command_type = ConsoleCommandType.Function;
    }
    public ConsoleCommand_Func(Action action, ConsoleCommandAttribute attrib = null) : this(attrib) {
        is_params = false;
        action_empty = action;
    }
    public ConsoleCommand_Func(Action<string[]> action, ConsoleCommandAttribute attrib = null) : this(attrib) {
        is_params = true;
        action_params = action;
    }

    public bool is_params = false; // @Hack  This controls whether the function accepts parameters.
    public Action action_empty;
    public Action<string[]> action_params;

    public void invoke(string[] args = null) {
        if (!is_params) action_empty.Invoke();
        else action_params.Invoke(args);
    }
}

public class ConsoleCommand_Var : ConsoleCommand {
    public ConsoleCommand_Var(Ref var_ref, ConsoleCommandAttribute attrib = null) : base(attrib) {
        command_type = ConsoleCommandType.Variable;
        this.var_ref = var_ref;
        
    }
    public Ref var_ref;
    public object get_value()             => var_ref.get_value();
    public void   set_value(object value) => var_ref.set_value(value);
}

public class Ref {
    public Ref() { }
    public Ref(Func<object> getter, Action<object> setter) {
        this.getter = getter;
        this.setter = setter;
        var_type = get_value().GetType();
    }
    public Type  var_type;
    public Func  <object> getter;
    public Action<object> setter;
    public object get_value()             => getter.Invoke();
    public void   set_value(object value) => setter.Invoke(value);
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class ConsoleCommandAttribute : Attribute {
    public ConsoleCommandAttribute(string help_text = null, bool is_cheat_command = false, params string[] aliases) {
        this.is_cheat_command = is_cheat_command;
        this.help_text = help_text;
        this.aliases = aliases;
    }
    public bool     is_cheat_command;
    public string   help_text;
    public string[] aliases;
}

public partial class DebugConsole {
    public Dictionary<string, ConsoleCommand> registered_commands = new();

    int registered_command_count = 0;
    bool register_command_internal(ConsoleCommand command, params string[] aliases) {
        if (command == null     && log_error("null command!")) return false;

        // TODO: Should we just use a List<string> within ConsoleCommand as well?
        // What are the performance implications?

        List<string> temp_aliases = new();
        if (aliases != null)          foreach (string s in aliases)          temp_aliases.Add(s);
        if (command?.aliases != null) foreach (string s in command?.aliases) temp_aliases.Add(s);
        if (temp_aliases.Count == 0 && log_error("no aliases!")) return false;

        foreach (string key in registered_commands.Keys) {
            if (temp_aliases.Contains(key)) {
                log_warn("the alias '%' is already registered. Ignoring!".interp(key));
                temp_aliases.Remove(key);
            }
        }
        command.aliases = temp_aliases.ToArray();

        foreach (string alias in command.aliases) registered_commands.Add(alias, command);
        ++registered_command_count;

        return true;
    }

    public static bool register_command(ConsoleCommand command, params string[] aliases) => get_instance().register_command_internal(command, aliases);
    public static bool register_command(Ref var_ref, params string[] aliases) => get_instance().register_command_internal(new ConsoleCommand_Var(var_ref), aliases);
    public static bool register_command(Action action, params string[] aliases) => get_instance().register_command_internal(new ConsoleCommand_Func(action), aliases);
    public static bool register_command(Action<string[]> action, params string[] aliases) => get_instance().register_command_internal(new ConsoleCommand_Func(action), aliases);

    public static bool COMMANDS_AlwaysAddFuncNames = true;
    static string[] register_command_func_handle_aliases(MethodInfo info, string[] aliases) {
        if (COMMANDS_AlwaysAddFuncNames || aliases.Length == 0) {
            // Allocate new space for the function name:
            string[] new_aliases = new string[aliases.Length + 1];
            // Add the function name as an alias:
            new_aliases[0] = info.Name;
            // Remove the "cmd_" prefix if exists:
            if (new_aliases[0].StartsWith("cmd_")) new_aliases[0] = new_aliases[0][4 ..];
            // Copy the remaining aliases:
            aliases.CopyTo(new_aliases, 1);
            return new_aliases;
        }
        return aliases;
    }

    // ----- //

    void register_commands_from_assembly() {
        Assembly assembly = Assembly.GetExecutingAssembly();
        Type[] types = assembly.GetTypes();

        // NOTE: only static methods and fields can be used as console commands!
        
        // Methods:
        foreach (Type type in types) {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (MethodInfo info in methods) {
                if (!info.IsDefined(typeof(ConsoleCommandAttribute))) continue;
                
                ConsoleCommandAttribute attrib = (ConsoleCommandAttribute)info.GetCustomAttribute(typeof(ConsoleCommandAttribute));

                bool is_params = false;
                ParameterInfo[] parameters = info.GetParameters();
                if (parameters.Length > 0 && parameters[0].ParameterType == typeof(string[])) is_params = true;

                if (!is_params) {
                    Action action = (Action)info.CreateDelegate(typeof(Action));
                    string[] aliases = register_command_func_handle_aliases(action.Method, attrib.aliases);
                    ConsoleCommand_Func cmd = new(action, attrib);
                    register_command(cmd, aliases);
                } else {
                    Action<string[]> action = (Action<string[]>)info.CreateDelegate(typeof(Action<string[]>));
                    string[] aliases = register_command_func_handle_aliases(action.Method, attrib.aliases);
                    ConsoleCommand_Func cmd = new(action, attrib);
                    register_command(cmd, aliases);
                }
            }
        }

        // Fields:
        foreach (Type type in types) {
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (FieldInfo info in fields) {
                if (!info.IsDefined(typeof(ConsoleCommandAttribute))) continue;
                
                ConsoleCommandAttribute attrib = (ConsoleCommandAttribute)info.GetCustomAttribute(typeof(ConsoleCommandAttribute));
                string[] aliases = attrib.aliases.Length > 0 ? attrib.aliases : new string[1] { info.Name };
                Ref var_ref = new Ref(() => info.GetValue(null), (v) => info.SetValue(null, v));
                ConsoleCommand_Var cmd = new(var_ref, attrib);
                register_command(cmd, aliases);
            }
        }
    }

    [ConsoleCommand("Deletes all of the text from the console.")]
    static void cmd_clear() {
        DebugConsole console = get_instance();
        if (!console && log_error("no console!")) return;
        for (int i = console.ui_lines.Count - 1; i >= 0; --i) console.destroy_line(i);
    }
    [ConsoleCommand("Changes the compact mode size of the console.")]
    static void cmd_change_console_size(string[] args) {
        DebugConsole console = get_instance();
        if (!console && log_error("no console!")) return;

        float y = args[0].as_float();
        // TODO: console width!
        if (y <= 0) y = console.CONSOLE_DefaultHeight;
        console.change_size(false, y);
    }
    [ConsoleCommand("Toggles the category button visibility next to console lines.")]
    static void cmd_toggle_line_categories()   => get_instance().CONSOLE_ShowLineCategories = !get_instance().CONSOLE_ShowLineCategories;
    [ConsoleCommand("Toggles the ability to submit a console command continuously by holding down the submit button.")]
    static void cmd_toggle_submit_repetition() {
        DebugConsole console = get_instance();
        if (!console && log_error("no console!")) return;

        console.CONSOLE_AllowSubmitRepetition = !console.CONSOLE_AllowSubmitRepetition;
        write_line_internal("new state: %".interp(get_instance().CONSOLE_AllowSubmitRepetition));

        // We clear the input field in submit() when submit repetition is off.
        // Since submit() invoked this function while submit repetition was off, it has not cleared the input field, 
        // so let's do it ourselves:
        console.clear_input_field();
        console.focus_input_field();
    }
    [ConsoleCommand("Lists all commands.")]
    static void cmd_help(string[] args) {
        DebugConsole console = get_instance();
        if (!console && log_error("no console!")) return;

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
            console.submit("% %".interp(args[0], "?"));
            return;
        }
        
        write_line_internal("Listing all commands: (%)".interp(console.registered_command_count));
        int prev_hash = -1;
        foreach (var kv in console.registered_commands) {
            ConsoleCommand cmd = kv.Value;
            int cmd_hash = cmd.GetHashCode();

            // We already print each alias ourselves. Ignore "duplicates":
            if (cmd_hash == prev_hash) continue;

            string s_help_text = null;
            if (!cmd.help_text.is_empty()) s_help_text = $" :: {cmd.help_text}";

            int longest_key_length = console.registered_commands.Keys.Max(k => k.Length);
            string s_aliases = null;
            string s_alias   = cmd.aliases?[0].PadRight(longest_key_length);
            if (show_aliases && cmd.aliases?.Length > 1) {
                s_aliases = $" :: aliases: [{string.Join("; ", cmd.aliases, 1, cmd.aliases.Length - 1)}]";
            }

            string s_hash = show_hashes ? $" [{cmd_hash:X8}]" : null;

            write_line_internal("  - %%%".interp(s_alias, s_hash, !show_aliases ? s_help_text : null, s_aliases));

            prev_hash = cmd_hash;
        }
    }
   [ConsoleCommand("Filter by a log level category.")]
    static void cmd_filter(string[] args) {
        DebugConsole console = get_instance();
        if (!console && log_error("no console!")) return;
        
        if (args.Length == 0) {
            write_line("current filter: %".interp(console.current_filter), LogLevel._ConsoleInternal);
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
        console.filter(level);
    }
    [ConsoleCommand("Print debug information about a registered command.")]
    static void cmd_print_cmd_info(string[] args) {
        DebugConsole console = get_instance();
        if (!console && log_error("no console!")) return;

        if (args.Length == 0) write_line_internal("missing command arg!");
        var cmd_kv = console.registered_commands.Where(kv => kv.Key == args[0]).FirstOrDefault();
        ConsoleCommand cmd = cmd_kv.Value;
        if (cmd == null) write_line_internal("commad not found!");

        write_line_internal("type:             %".interp(cmd.command_type));
        write_line_internal("help_text:        %".interp(cmd.help_text));
        write_line_internal("is_cheat_command: %".interp(cmd.is_cheat_command));
        write_line_internal("aliases: [%]".interp(cmd.aliases != null ? string.Join("; ", cmd.aliases) : null));
        if (cmd.command_type == ConsoleCommandType.Function) {
            write_line_internal("TODO: print function info!");
        }
        else if (cmd.command_type == ConsoleCommandType.Variable) {
            ConsoleCommand_Var cmd_var = (ConsoleCommand_Var)cmd;
            write_line_internal("var_type: % (base: %)".interp(cmd_var.var_ref.var_type.Name, cmd_var.var_ref.var_type.BaseType.Name));
        }
    }
#if false
    [ConsoleCommand("Controls whether log messages that come in for Unity should be redirected to the XZShared console.")]
    static void cmd_unity_logging_redirection(string[] args) {
        if (args.Length == 0 && write_line_internal("Current value: %".interp(CONSOLE_RedirectUnityLogging))) return;
        CONSOLE_RedirectUnityLogging = args[0].as_bool();
    }
#endif

    [ConsoleCommand("Quits the game, or stops play mode in the editor.", aliases: "q")]
    static void cmd_quit() {
        log("quitting...");
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}