using System.IO;

using static Logging;

public interface ISongLoader {
    public song_info load_song(string song_name, string lookup_path);
}

public static class SongLoader {
    public enum GameType { RHX_2022, FREQ_2001, AMP_2003, AMP_2016 }

    public static (bool success, song_info info) load_song(string song_name) {
        (GameType game_type, string song_path) lookup = lookup_song_by_name(song_name);
        if (lookup.song_path.is_empty()) {
            log_error("Can't load song '%'.".interp(song_name));
            return (false, default);
        }

        switch (lookup.game_type) {
            default: {
                log_error("Unimplemented.");
                return (false, default);
            }
            case GameType.AMP_2016: return (true, new AMP_2016.AMP2016_SongLoader().load_song(song_name, lookup.song_path));
        }
    }

    public static (GameType game_type, string song_path) lookup_song_by_name(string song_name) {
        foreach (var kv in Variables.song_lookup_paths) {
            GameType lookup_gametype = kv.Key;
            string   lookup_path     = kv.Value;

            if (!Directory.Exists(lookup_path)) {
                log_error("Lookup path doesn't exist: %".interp(lookup_path));
                continue;
            }
            
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
                log(LogLevel.IO, "found song '%' at path: %".interp(song_name, lookup_path));
                return (lookup_gametype, lookup_path);
            }
        }

        log(LogLevel.IO | LogLevel.Error, "failed to find song '%' at any of the lookup paths".interp(song_name));
        return (GameType.RHX_2022, null);
    }
}