using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Logger;

public class RHX_SongLoader : SongLoader
{
    public const string RHX_FileExtension = ".rhx_song";

    public override Song LoadSong(string song_name)
    {
        Log("Loading song: '%'".T(nameof(RHX_SongLoader)), song_name);

        string song_path = FindSongPath(song_name, Song_Type.RHYTHMIC); //FindSongFile(song_name);
        if (song_path == null && LogE("Could not find song: %", song_name)) return null;

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

        return song;
    }

    Song LoadSongFile(string file_path)
    {
        if (!File.Exists(file_path) && LogE("File does not exist: %", file_path)) return null;

        using (TextReader r = File.OpenText(file_path))
        {
            string text = r.ReadToEnd();
            List<RHX_Token> tokens = new RHX_SongFile_Parser(text).Tokenize();
            foreach (RHX_Token t in tokens)
                Log("[RHX_FILE] Type: % value: %", t.type, t.value);
        }


        return null;
    }


}