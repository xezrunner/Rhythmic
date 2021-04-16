using DebugMenus;
using System;
using System.Collections.Generic;

// These take in strings as parameters.
// It was initially developed with it taking in objects, but it shouldn't really matter.
// Your console commands should be written with it taking in a string array that'll represent the
// parameters given in the console (separated by spaces).
public struct ConsoleCommand
{
    public ConsoleCommand(string command, Action<string[]> action) { Command = command; Action = action; }

    public string Command;
    public Action<string[]> Action;
}

public partial class DebugConsole
{
    // This is the main procedure for registering common console commands.
    // Other classes are free to register console commands at any point by using RegisterCommand().
    void RegisterCommonCommands()
    {
        _RegisterCommand("test", test);
        _RegisterCommand("song", LoadSong);
        _RegisterCommand("test", test);
    }

    public List<ConsoleCommand> Commands = new List<ConsoleCommand>();
    public int Commands_Count = 0;

    // NOTE: If you don't check for existing commands, depending on Process_ReturnOnFoundCommand, you may run multiple commands at once!
    public static bool Register_CheckForExistingCommands = true;
    public static void RegisterCommand(string command, Action<string[]> action) => Instance?._RegisterCommand(command, action);
    void _RegisterCommand(string command, Action<string[]> action)
    {
        if (Register_CheckForExistingCommands)
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

    /// Songs:
    void LoadSong(string[] args)
    {
        if (args.Length == 0) DebugConsole.Log("usage: ".TM() + "song ".AddColor(Colors.Application) + "<song name>".AddColor(Colors.Unimportant));
        else SongsMenu.LoadSong(args[0]);
    }
}