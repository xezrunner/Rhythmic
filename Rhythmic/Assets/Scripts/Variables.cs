using System.Collections.Generic;
using static SongLoader;
using static Logging;

public static class Variables {
    public static Dictionary<GameType, string> song_lookup_paths = new() {
        {GameType.AMP_2016,  @"H:\Miscellaneous\HMXAMPLITUDE\ps4_songs" }
    };
    [ConsoleCommand]
    static void cmd_list_song_lookup_paths() {
        foreach (var kv in song_lookup_paths) DebugConsole.write_line("for: %  path: \"%\"".interp(kv.Key, kv.Value), LogLevel.Debug);
    }
}