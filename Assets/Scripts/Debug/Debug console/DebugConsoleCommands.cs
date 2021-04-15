using System;
using System.Collections.Generic;

// These take in strings as parameters.
// It was initially developed with it taking in objects, but it shouldn't really matter.
// Your console commands should be written with it taking in a string array that'll represent the
// parameters given in the console (separated by spaces).
public struct ConsoleCommand
{
    public ConsoleCommand(string command, Action<string[]> action)
    {
        Command = command;
        Action = action;
    }

    public string Command;
    public Action<string[]> Action;
}

public partial class DebugConsole
{
    // This is the main procedure for registering common console commands.
    // Other classes are free to register console commands at any point by using RegisterCommand().
    void RegisterCommonCommands()
    {
        RegisterCommand("test", test);
    }

    public List<ConsoleCommand> Commands = new List<ConsoleCommand>();
    public int Commands_Count = 0;

    public void RegisterCommand(string command, Action<string[]> action)
    {
        ConsoleCommand c = new ConsoleCommand(command, action);
        Commands.Add(c); ++Commands_Count;
    }

    // ----- Common commands ----- //
    /// You should add non-common commands from a different class.

    public void test(string[] a)
    {
        if (a.Length == 0)
        { Log("We got no arguments.".M()); return; }

        string s = "";
        for (int i = 0; i < a.Length; ++i) s += a[i] + ' ';
        Log("got the following args: %".TM(this), s);
    }
}