using System;
using System.Linq;
using System.Collections.Generic;
using static DebugConsoleDefaultCommands;
using static Logger;

public struct ConsoleCommand
{
    #region Ctors
    public ConsoleCommand(string command, Action action, params string[] aliases)
    {
        this.command = command;
        this.action = action;
        this.action_with_args = null;
        this.aliases = aliases.ToList();
        this.help_text = "";
    }
    public ConsoleCommand(string command, Action<string[]> action_with_args, params string[] aliases)
    {
        this.command = command;
        this.action = null;
        this.action_with_args = action_with_args;
        this.aliases = aliases.ToList();
        this.help_text = "";
    }
    #endregion

    public string command;
    public List<string> aliases;
    public string help_text;

    public bool is_action_empty { get { return action == null; } }
    public Action action;
    public Action<string[]> action_with_args;
}

public class DebugConsoleCommands
{
    public DebugConsoleCommands()
    {
        if (COMMANDS_RegisterDefaultCommands)
            RegisterDefaultCommands();
    }

    public static bool COMMANDS_RegisterDefaultCommands = true; // Whether to register default commands
    public static bool COMMANDS_PreventDuplication = true; // Whether duplicate aliases should be prevented. Duplicate aliases will be rejected.
    public static bool COMMANDS_PreventAliasDuplication = true; // Whether duplicate aliases should be prevented. Duplicate aliases will be rejected.

    public List<ConsoleCommand> registered_commands = new List<ConsoleCommand>();
    public int commands_count;

    public bool RegisterCommand(string command, Action action, params string[] aliases) => RegisterCommand(new ConsoleCommand(command, action, aliases));
    public bool RegisterCommand(string command, Action<string[]> action_with_args, params string[] aliases) => RegisterCommand(new ConsoleCommand(command, action_with_args, aliases));
    public bool RegisterCommand(ConsoleCommand command)
    {
        foreach (ConsoleCommand c in registered_commands)
        {
            if (!COMMANDS_PreventDuplication) break;
            if (c.command == command.command && LogE("This command already exists: '%'", c.command)) return false;

            if (!COMMANDS_PreventAliasDuplication) break;
            foreach (string alias in c.aliases)
                if (command.aliases.Contains(alias))
                {
                    LogW("Warning: command '%' contains duplicate alias '%' from another command '%'. Rejecting.", command.command, alias, c.command);
                    command.aliases.Remove(alias);
                }
        }

        registered_commands.Add(command);
        ++commands_count;
        return true;
    }

    public void DumpRegisteredCommands()
    {
        for (int i = 0; i < registered_commands.Count; ++i)
        {
            ConsoleCommand c = registered_commands[i];
            Log("[%] %", i, c.command);
        }
    }

    // ---------- //

    public void RegisterDefaultCommands()
    {
        RegisterCommand("test", cmd_test);
    }
}

public partial class DebugConsole : DebugCom
{
    public void COMMANDS_Start() => cmdsystem = new DebugConsoleCommands();
    public DebugConsoleCommands cmdsystem;

    public bool RegisterCommand(string command, Action action, params string[] aliases) => cmdsystem.RegisterCommand(new ConsoleCommand(command, action, aliases));
    public bool RegisterCommand(string command, Action<string[]> action_with_args, params string[] aliases) => cmdsystem.RegisterCommand(new ConsoleCommand(command, action_with_args, aliases));
    public bool RegisterCommand(ConsoleCommand command) => cmdsystem.RegisterCommand(command);
}