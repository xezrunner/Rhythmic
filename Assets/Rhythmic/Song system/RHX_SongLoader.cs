using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Logger;

public static class RHX_SongLoader
{
    public const string RHX_FileExtension = ".rhx_song";

    public static Song LoadSong(string song_name)
    {
        Log("Loading song: '%'".T(nameof(RHX_SongLoader)), song_name);

        string song_path = FindSongFile(song_name);
        if (song_path == null && LogE("Could not find song: %".T(nameof(RHX_SongLoader)), song_name)) return null;

        string[] files = Directory.GetFiles(song_path);
        string rhx_song_file_path = null;

        // Find the .rhx_song file within the song directory:
        foreach (string f in files)
        {
            if (f.GetExt() != RHX_FileExtension) continue;
            rhx_song_file_path = f;
            break;
        }

        Song song = LoadSongFile(rhx_song_file_path);

        return null;
    }

    static string FindSongFile(string song_name)
    {
        List<string> lookup_paths = GameState.Variables.RHX_song_lookup_paths;
        for (int i = 0; i < lookup_paths.Count; ++i)
        {
            string s = lookup_paths[i];
            s = s.Replace("<data>", Application.dataPath);

            Log("RHX: [%]: %", i, s);

            if (!Directory.Exists(s) && LogE("This lookup directory does not exist: %", s))
                continue;

            string s_song_dir = "%/%".Parse(s, song_name);
            if (!Directory.Exists(s_song_dir) && LogE("The song was not found here: %", s_song_dir))
                continue;

            return s_song_dir;
        }

        return null;
    }

    static Song LoadSongFile(string file_path)
    {
        if (!File.Exists(file_path) && LogE("File does not exist: %", file_path)) return null;

        using (TextReader r = File.OpenText(file_path))
        {
            string text = r.ReadToEnd();

        }


        return null;
    }

    
}

public enum RHX_Token_Type { test }
public struct RHX_Token
{
    
}

public class RHX_Tokenizer
{
    public RHX_Tokenizer(string text) { Text = text; }

    public string Text;
}