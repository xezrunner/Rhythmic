using System.IO;

using static Logging;

public interface ISongLoader {
    public song_info load_song(string song_name, string lookup_path);
}

public static class SongLoader {
    public enum GameType { RHX_2022, FREQ_2001, AMP_2003, AMP_2016 }

    [ConsoleCommand] public static void load_song(string[] args) => load_song(args[0]);
    public static song_info load_song(string song_name) {
        (GameType game_type, string song_path) lookup = lookup_song_by_name(song_name);
        if (lookup.song_path.is_empty()) throw new("Can't load song '%'.".interp(song_name));

        switch (lookup.game_type) {
            default: throw new("Unimplemented.");
            case GameType.AMP_2016: return new AMP_2016.AMP2016_SongLoader().load_song(song_name, lookup.song_path);
        }
    }

    public static (GameType game_type, string song_path) lookup_song_by_name(string song_name) {
        foreach (var kv in Variables.song_lookup_paths) {
            GameType lookup_gametype = kv.Key;
            string   lookup_path     = kv.Value;

            bool found = false;
            foreach (string dir in Directory.GetDirectories(lookup_path)) {
                DirectoryInfo dir_info = new(dir); // @Perf!
                if (dir_info.Name == song_name) {
                    lookup_path = dir;
                    found = true;
                    break;
                }
            }

            if (found) {
                log("found song '%' at path: %".interp(song_name, lookup_path), LogLevel.IO);
                return (lookup_gametype, lookup_path);
            }
        }

        log("failed to find song '%' at any of the lookup paths".interp(song_name), LogLevel.IO | LogLevel.Error);
        return (GameType.RHX_2022, null);
    }
}