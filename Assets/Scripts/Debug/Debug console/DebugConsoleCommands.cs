using DebugMenus;
using System;
using System.Collections.Generic;

// These take in strings as parameters.
// It was initially developed with it taking in objects, but it shouldn't really matter.
// Your console commands should be written with it taking in a string array that'll represent the
// parameters given in the console (separated by spaces).
public struct ConsoleCommand
{
    public ConsoleCommand(string command, Action action, string helpText = "") { Command = command; Action_Empty = action; Action_Param = null; is_action_empty = true; HelpText = helpText; }
    public ConsoleCommand(string command, Action<string[]> action, string helpText = "") { Command = command; Action_Param = action; Action_Empty = null; is_action_empty = false; HelpText = helpText; }

    public string Command;
    public string HelpText; // --help is a hard-coded argument that'll give you the help text without invoking the command.

    public bool is_action_empty; // HACK!
    public Action Action_Empty;
    public Action<string[]> Action_Param;

    public void Invoke(params string[] args)
    {
        try
        {
            if (is_action_empty) Action_Empty();
            else Action_Param(args);
        }
        catch (Exception ex)
        { DebugConsole.Log("Failed to execute command: " + "%".AddColor(Colors.Error), ex.Message); }
    }
}

public partial class DebugConsole
{
    public List<ConsoleCommand> Commands = new List<ConsoleCommand>();
    public int Commands_Count = 0;

    // This is the main procedure for registering common console commands.
    // Other classes are free to register console commands at any point by using RegisterCommand().
    void RegisterCommonCommands() // **********************************
    {
        RegisterCommand("clear", _Clear);

        RegisterCommand(test, $"usage: {"test".AddColor(Colors.Application)} <arguments>"); // temp!
        RegisterCommand(clear_text_test);
        RegisterCommand(logger_parser_test, "Tests the new Logger parser system.");

        RegisterCommand(toggle_autocomplete);
        RegisterCommand(set_autocomplete);

        RegisterCommand(song, $"usage: {"song".AddColor(Colors.Application)} <song_name>");
        RegisterCommand(world, $"usage: {"world".AddColor(Colors.Application)} <relative world path, starting from Scenes/>");

        RegisterCommand(switch_to_track, $"usage: {"switch_to_track".AddColor(Colors.Application)} <track_id>");
    }

    // NOTE: If you don't check for existing commands, depending on ReturnOnFoundCommand, you may run multiple commands at once!
    public static bool Register_CheckForExistingCommands = true;

    bool RegisterCommand_CheckDuplication(string command)
    {
        if (Register_CheckForExistingCommands) // TODO: Performance, especially in non-debug builds (?)
            for (int i = 0; i < Commands_Count; ++i)
                if (Commands[i].Command == command) { Logger.LogMethodW($"Command {command.AddColor(Colors.Application)} was already registered! Ignoring current attempt..."); return true; }
        return false;
    }

    #region Public -> RegisterCommand
    //public static void RegisterCommand(string command, Action<string[]> action) => Instance?._RegisterCommand(command, action);

    public static void RegisterCommand(string command, Action<string[]> action, string helpText = "") => Instance?._RegisterCommand(command, action, helpText); // Parameters
    public static void RegisterCommand(string command, Action action, string helpText = "") => Instance?._RegisterCommand(command, action, helpText); // Empty

    // Name-less register overloads
    public static void RegisterCommand(Action action, string helpText = "") => Instance?._RegisterCommand(action.Method.Name, action, helpText); //Empty
    public static void RegisterCommand(Action<string[]> action, string helpText = "") => Instance?._RegisterCommand(action.Method.Name, action, helpText); // Parameters

    #endregion

    void _RegisterCommand(string command, Action<string[]> action, string helpText = "") // Parameters
    {
        if (RegisterCommand_CheckDuplication(command)) return;

        ConsoleCommand c = new ConsoleCommand(command, action, helpText);
        Commands.Add(c); ++Commands_Count;
    }
    void _RegisterCommand(string command, Action action, string helpText = "") // Empty
    {
        if (RegisterCommand_CheckDuplication(command)) return;

        ConsoleCommand c = new ConsoleCommand(command, action, helpText);
        Commands.Add(c); ++Commands_Count;
    }

    // Name-less register overloads
    void _RegisterCommand(Action action, string helpText = "") => _RegisterCommand(action.Method.Name, action, helpText); //Empty
    void _RegisterCommand(Action<string[]> action, string helpText = "") => _RegisterCommand(action.Method.Name, action, helpText); // Parameters

    // ----- Common commands ----- //
    /// You should add non-common commands from a different class.

    void logger_parser_test(string[] args)
    {
        Logger.Log("%".M(), args);
    }
    void clear_text_test()
    {
        string s = $"Hello% {"Wow".AddColor(Colors.Network)}, this {"is".Italic()} {"really".AddColor(Colors.Application)} cool!";
        Log("The original text is: %", s);
        s = s.ClearColors();
        Log("The color-cleared text is: %", s);
    }

    void test(string[] a)
    {
        if (a.Length == 0)
        { _Log("We got no arguments.".M()); return; }

        string s = "";
        for (int i = 0; i < a.Length; ++i) s += a[i] + ' ';
        _Log("got the following args: %".TM(this), s);
    }
    void set_autocomplete(string[] args)
    {
        if (args != null && args.Length != 0)
        {
            // TODO: We probably want a parser that will parse numeric values to bools as well as things like true/false and 'enabled'/'disabled'!
            bool result; int int_result;

            if (int.TryParse(args[0], out int_result)) result = (int_result == 1);
            else result = bool.Parse(args[0]);

            autocomplete_enabled = result;
        }
        Log("Autocomplete: %", (autocomplete_enabled ? "enabled" : "disabled"));
    }
    void toggle_autocomplete()
    {
        autocomplete_enabled = !autocomplete_enabled;
        if (autocomplete_enabled) Log("Autocomplete enabled.");
        else Log("Autocomplete disabled.");
    }

    /// Songs & worlds:
    void song(string[] args) => SongsMenu.LoadSong(args[0]);
    void world(string[] args) => WorldsMenu.LoadWorld(args[0]);

    /// Track switching
    void switch_to_track(string[] args) => AmpPlayerTrackSwitching.Instance.SwitchToTrack(int.Parse(args[0]));
}