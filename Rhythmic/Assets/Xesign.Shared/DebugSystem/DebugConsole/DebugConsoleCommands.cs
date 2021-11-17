using System;
using System.Linq;
using System.Collections.Generic;
using static DebugConsoleDefaultCommands;
using static Logger;
using UnityEngine;

public partial class DebugConsole : DebugCom
{
    public void COMMANDS_Start() => cmdsystem = new DebugConsoleCommands();
    public DebugConsoleCommands cmdsystem;

    public bool RegisterCommand(string command, Action action, params string[] aliases) => cmdsystem.RegisterCommand(new ConsoleCommand(command, action, aliases));
    public bool RegisterCommand(string command, Action<string[]> action_with_args, params string[] aliases) => cmdsystem.RegisterCommand(new ConsoleCommand(command, action_with_args, aliases));
    public bool RegisterCommand(Action action, params string[] aliases) => cmdsystem.RegisterCommand(new ConsoleCommand(action.Method.Name, action, aliases));
    public bool RegisterCommand(Action<string[]> action, params string[] aliases) => cmdsystem.RegisterCommand(new ConsoleCommand(action.Method.Name, action, aliases));
    public bool RegisterCommand(ConsoleCommand command) => cmdsystem.RegisterCommand(command);
}

public struct ConsoleCommand
{
    #region Ctors
    public ConsoleCommand(string command, Action action_empty, params string[] aliases)
    {
        this.command = command;
        this.action_empty = action_empty;
        this.action_args = null;
        this.aliases = aliases.ToList();
        this.help_text = "";
    }
    public ConsoleCommand(string command, Action<string[]> action_args, params string[] aliases)
    {
        this.command = command;
        this.action_empty = null;
        this.action_args = action_args;
        this.aliases = aliases.ToList();
        this.help_text = "";
    }
    #endregion

    public string command;
    public List<string> aliases;
    public string help_text;

    public bool is_action_empty { get { return action_empty != null; } }
    public Action action_empty;
    public Action<string[]> action_args;

    public void Invoke(string[] args = null)
    {
        if (is_action_empty) Invoke_Empty();
        else Invoke_Args(args);
    }
    public void Invoke_Empty() => action_empty();
    public void Invoke_Args(string[] args) => action_args(args);
}

public class DebugConsoleCommands
{
    public DebugConsoleCommands()
    {
        if (COMMANDS_RegisterDefaultCommands)
            RegisterDefaultCommands();
    }

    public static bool COMMANDS_AutoTrimCmdPrefix = true; // Whether to automatically trim the 'cmd_' prefix from commands during registration.
    public static bool COMMANDS_RegisterDefaultCommands = true; // Whether to register default commands,
    public static bool COMMANDS_PreventDuplication = true; // Whether duplicate aliases should be prevented. Duplicate aliases will be rejected.
    public static bool COMMANDS_PreventAliasDuplication = true; // Whether duplicate aliases should be prevented. Duplicate aliases will be rejected.

    public List<ConsoleCommand> registered_commands = new List<ConsoleCommand>();
    public int commands_count;

    public bool RegisterCommand(string command, Action action, params string[] aliases) => RegisterCommand(new ConsoleCommand(command, action, aliases));
    public bool RegisterCommand(string command, Action<string[]> action, params string[] aliases) => RegisterCommand(new ConsoleCommand(command, action, aliases));
    public bool RegisterCommand(Action action, params string[] aliases) => RegisterCommand(new ConsoleCommand(action.Method.Name, action, aliases));
    public bool RegisterCommand(Action<string[]> action, params string[] aliases) => RegisterCommand(new ConsoleCommand(action.Method.Name, action, aliases));
    public bool RegisterCommand(ConsoleCommand cmd)
    {
        foreach (ConsoleCommand c in registered_commands)
        {
            if (!COMMANDS_PreventDuplication) break;
            if (c.command == cmd.command && LogE("This command already exists: '%'", c.command)) return false;

            if (!COMMANDS_PreventAliasDuplication) break;
            foreach (string alias in c.aliases)
                if (cmd.aliases.Contains(alias))
                {
                    LogW("Warning: command '%' contains duplicate alias '%' from another command '%'. Rejecting.", cmd.command, alias, c.command);
                    cmd.aliases.Remove(alias);
                }
        }

        // Remove 'cmd_' prefix from commands (default commands are prefixed like that)
        if (COMMANDS_AutoTrimCmdPrefix && cmd.command.StartsWith("cmd_"))
            cmd.command = cmd.command.Remove(0, "cmd_".Length);

        registered_commands.Add(cmd);
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
        RegisterCommand(test);
        RegisterCommand(help);
        RegisterCommand(fps);
        RegisterCommand(timescale, "ts");
    }
}

public static class DebugConsoleDefaultCommands
{
    static DebugConsoleCommands cmdsystem = DebugConsole.Instance.cmdsystem;

    public static void test() => Log("Test command");
    public static void help(string[] args)
    {
        if (args != null && args.Length > 0)
        {
            // Command-specific help goes here...
            LogW("TODO: Command-specific help!");
            return;
        }
        DebugConsole.ConsoleLog("Listing commands...");
        DebugConsole.ConsoleLog("Total commands: %".AddColor(Colors.Unimportant), cmdsystem.commands_count);
        foreach (ConsoleCommand c in cmdsystem.registered_commands)
            DebugConsole.ConsoleLog(c.command.AddColor(Colors.Warning) + (!c.help_text.IsEmpty() ? " - " + c.help_text.AddColor(Colors.Unimportant) : null));
    }
    public static void fps(string[] args)
    {
        if (args == null || args.Length == 0) CoreGameUtils.SetFramerate();
        else
        {
            int fps = args[0].ParseInt();
            int vsync = args[1].ParseInt();
            CoreGameUtils.SetFramerate(fps, vsync);
        }
    }
    public static void timescale(string[] args)
    {
        //if (args.Length == 0 && DebugConsole.ConsoleLog("timescale: argument required!")) return;
        if (args == null || args.Length == 0) CoreGameUtils.SetTimescale();
        else CoreGameUtils.SetTimescale(args[0].ParseFloat());
    }
}