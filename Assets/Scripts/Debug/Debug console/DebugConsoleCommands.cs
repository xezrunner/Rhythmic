using DebugMenus;
using System;
using System.Collections.Generic;

// These take in strings as parameters.
// It was initially developed with it taking in objects, but it shouldn't really matter.
// Your console commands should be written with it taking in a string array that'll represent the
// parameters given in the console (separated by spaces).
public struct ConsoleCommand
{
    public ConsoleCommand(string command, Action action) { Command = command; Action_Empty = action; Action_Param = null; is_action_empty = true; }
    public ConsoleCommand(string command, Action<string[]> action) { Command = command; Action_Param = action; Action_Empty = null; is_action_empty = false; }

    public string Command;

    public bool is_action_empty; // HACK!
    public Action Action_Empty;
    public Action<string[]> Action_Param;

    public void Invoke(params string[] args)
    {
        if (is_action_empty) Action_Empty();
        else Action_Param(args);
    }
}

public partial class DebugConsole
{
    // This is the main procedure for registering common console commands.
    // Other classes are free to register console commands at any point by using RegisterCommand().
    void RegisterCommonCommands()
    {
        _RegisterCommand("test", test); // temp!
        _RegisterCommand("song", LoadSong);
        _RegisterCommand("world", LoadWorld);
        _RegisterCommand("close", close);
    }

    public List<ConsoleCommand> Commands = new List<ConsoleCommand>();
    public int Commands_Count = 0;

    // NOTE: If you don't check for existing commands, depending on Process_ReturnOnFoundCommand, you may run multiple commands at once!
    public static bool Register_CheckForExistingCommands = true;
    public static void RegisterCommand(string command, Action<string[]> action) => Instance?._RegisterCommand(command, action);
    void _RegisterCommand(string command, Action<string[]> action)
    {
        if (Register_CheckForExistingCommands) // TODO: Performance, especially in non-debug builds (?)
            for (int i = 0; i < Commands_Count; ++i)
                if (Commands[i].Command == command) { Logger.LogMethodW($"Command {command.AddColor(Colors.Application)} was already registered! Ignoring current attempt..."); return; }

        ConsoleCommand c = new ConsoleCommand(command, action);
        Commands.Add(c); ++Commands_Count;
    }
    void _RegisterCommand(string command, Action action)
    {
        if (Register_CheckForExistingCommands) // TODO: Performance, especially in non-debug builds (?)
            for (int i = 0; i < Commands_Count; ++i)
                if (Commands[i].Command == command) { Logger.LogMethodW($"Command {command.AddColor(Colors.Application)} was already registered! Ignoring current attempt..."); return; }

        ConsoleCommand c = new ConsoleCommand(command, action);
        Commands.Add(c); ++Commands_Count;
    }

    // ----- Common commands ----- //
    /// You should add non-common commands from a different class.

    public void test(string[] a)
    {
        if (a.Length == 0)
        { _Log("We got no arguments.".M()); return; }

        string s = "";
        for (int i = 0; i < a.Length; ++i) s += a[i] + ' ';
        _Log("got the following args: %".TM(this), s);
    }

    public void close()
    {
        UnfocusInputField();
    }

    /// Songs & worlds:
    void LoadSong(string[] args)
    {
        if (args.Length == 0) DebugConsole.Log("usage: ".TM() + "song ".AddColor(Colors.Application) + "<song name>".AddColor(Colors.Unimportant));
        else SongsMenu.LoadSong(args[0]);
    }
    void LoadWorld(string[] args)
    {
        if (args.Length == 0) DebugConsole.Log("usage: ".TM() + "world ".AddColor(Colors.Application) + "<relative world path, starting from Scenes/>".AddColor(Colors.Unimportant));
        else WorldsMenu.LoadWorld(args[0]);
    }
}