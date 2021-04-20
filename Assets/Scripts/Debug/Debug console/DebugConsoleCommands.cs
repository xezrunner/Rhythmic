using DebugMenus;
using System;
using System.Collections.Generic;
using System.Linq;

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

    /// <returns>Whether we encountered an error.</returns>
    public bool Invoke(params string[] args)
    {
        try
        {
            if (is_action_empty) Action_Empty();
            else Action_Param(args);
            return true;
        }
        catch (Exception ex)
        { DebugConsole.Log("Failed to execute command: " + "%".AddColor(Colors.Error), ex.Message); return false; }
    }
}

public partial class DebugConsole
{
    public List<ConsoleCommand> Commands = new List<ConsoleCommand>();
    public int Commands_Count = 0;

    // This is the main procedure for registering common console commands.
    // Other classes are free to register console commands at any point by using RegisterCommand().
    void Console_RegisterCommands() // **********************************
    {

        // Console-meta commands:
        RegisterCommand("clear", _Clear);
        RegisterCommand("quit", MainMenu.QuitGame, "Stops the game in the editor / quits the game in builds."); // TODO: Should use a global Quit procedure to quit the game once MetaSystem and/or GameState is in place
        RegisterCommand(help, "Lists all commands / gives help text for particular commands. usage: " + "help".AddColor(Colors.Application) + " <command>");
        RegisterCommand(toggle_autocomplete);
        RegisterCommand(set_autocomplete);
        RegisterCommand(logger_log, "Calls Logger.Log(). NOTE: \\% parameters are not supported yet!");

        // Test commands **************************************************
        RegisterCommand(test, $"usage: {"test".AddColor(Colors.Application)} <arguments>"); // temp!
        RegisterCommand(clear_text_test);
        RegisterCommand(logger_parser_test, "Tests the new Logger parser system.");
        RegisterCommand(test_console_limits, "Tests the console max text length limit.");
        RegisterCommand(get_console_text_lines, "Shows current console text line count.");
        RegisterCommand(scroll_to_bottom); RegisterCommand(scroll_to_top); RegisterCommand(scroll_to);
        RegisterCommand(set_console_line_limit, "Sets the console amount of lines allowed in the console.");
        RegisterCommand(set_console_text_limit, "Sets the maximum amount of characters allowed in a line.");

        // Common commands:
        RegisterCommand(song, $"usage: {"song".AddColor(Colors.Application)} <song_name>");
        RegisterCommand(world, $"usage: {"world".AddColor(Colors.Application)} <relative world path, starting from Scenes/>");

        RegisterCommand(switch_to_track, $"usage: {"switch_to_track".AddColor(Colors.Application)} <track_id>");
        RegisterCommand(capture_measure_range);
        RegisterCommand(capture_measure_amount);
        RegisterCommand(refresh_sequences, "usage: " + "refresh_sequences".AddColor(Colors.Application) + " <track_id (optional)>");
        RegisterCommand(refresh_notes, "usage: " + "refresh_notes".AddColor(Colors.Application) + " <track_id (optional)>");
        RegisterCommand(refresh_all, "usage: " + "refresh_all".AddColor(Colors.Application) + " <track_id (optional)>");
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

    // Displays the help text for any command:
    public static void Help_Command(string command) => Instance?._Help_Command(command);
    void _Help_Command(string command)
    {
        for (int i = 0; i < Commands.Count; i++)
        {
            ConsoleCommand c = Commands[i];
            if (c.Command == command)
            {
                if (c.HelpText != "") Log($"{c.HelpText}".AddColor(Colors.Unimportant));
                else Log("No help text for command %", command);
                return;
            }
        }
    }

    // ----- Common commands ----- //
    /// You should add non-common commands from a different class.

    void help(string[] args)
    {
        if (args.Length > 0)
        {
            Help_Command(args[0]);
            return;
        }

        string s = "Listing all commands: \n";
        for (int i = 0; i < Commands.Count; ++i)
        {
            ConsoleCommand c = Commands[i];

            s += $"{c.Command}".AddColor(Colors.IO);
            if (c.HelpText != "") s += $" :: {Commands[i].HelpText}".AddColor(Colors.Unimportant);
            if (i != Commands.Count - 1) s += "\n";
        }

        s += $"\nTotal command count: {Commands_Count.ToString().AddColor(Colors.IO)}";
        Log(s);
    }
    void set_autocomplete(string[] args)
    {
        if (args != null && args.Length != 0)
            autocomplete_enabled = args[0].ParseBool();
        Log("Autocomplete: %", (autocomplete_enabled ? "enabled" : "disabled"));
    }
    void toggle_autocomplete()
    {
        autocomplete_enabled = !autocomplete_enabled;
        if (autocomplete_enabled) Log("Autocomplete enabled.");
        else Log("Autocomplete disabled.");
    }
    void logger_log(string[] args) => Logger.Log(args); // TODO: "" quotes should be parsed as one individual string parameter

    void test(string[] a)
    {
        if (a.Length == 0)
        { _Log("We got no arguments.".M()); return; }

        string s = "";
        for (int i = 0; i < a.Length; ++i) s += a[i] + ' ';
        _Log("got the following args: %".TM(this), s);
    }
    void clear_text_test()
    {
        string s = $"Hello! {"Wow".AddColor(Colors.Network)}, this {"is".Italic()} {"really".AddColor(Colors.Application)} cool!";
        Log("The original text is: %", s);
        s = s.ClearColors();
        Log("The color-cleared text is: %", s);
    }
    void logger_parser_test(string[] args) => Logger.Log("%".M(), args);
    void test_console_limits()
    {
        string s = "";
        for (int i = 0; i < Line_Max_Length * 2; ++i)
            s += '0';
        Log(s);
    }
    void get_console_text_lines() => Log("Console text line count: %", UI_Text.text.Split('\n').Length);
    void set_console_line_limit(string[] args) => Text_Max_Lines = args[0].ParseInt();
    void set_console_text_limit(string[] args) => Line_Max_Length = args[0].ParseInt();
    void scroll_to_bottom() => ScrollConsole(SCROLL_BOTTOM);
    void scroll_to_top() => ScrollConsole(SCROLL_TOP);
    void scroll_to(string[] args) => ScrollConsole(float.Parse(args[0]));

    /// Songs and worlds:
    void song(string[] args) => SongsMenu.LoadSong(args[0]);
    void world(string[] args) => WorldsMenu.LoadWorld(args[0]);

    TracksController TracksController { get { return TracksController.Instance; } }

    /// Track switching
    void switch_to_track(string[] args) => AmpPlayerTrackSwitching.Instance.SwitchToTrack(int.Parse(args[0]));

    /// Track capturing
    void capture_measure_range(string[] args) => TracksController.CaptureMeasureRange(args[0].ParseInt(), args[1].ParseInt(), args[2].ParseInt());
    void capture_measure_amount(string[] args) => TracksController.CaptureMeasureAmount(args[0].ParseInt(), args[1].ParseInt(), args[2].ParseInt());

    /// Track refreshing
    void refresh_sequences(string[] args) => TracksController.RefreshSequences(args[0] == null ? null : TracksController.MainTracks[args[0].ParseInt()]);
    void refresh_notes(string[] args) => TracksController.RefreshTargetNotes(args[0] == null ? null : TracksController.MainTracks[args[0].ParseInt()]);
    void refresh_all(string[] args) => TracksController.RefreshAll(args[0] == null ? null : TracksController.MainTracks[args[0].ParseInt()]);
}