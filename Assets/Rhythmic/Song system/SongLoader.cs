using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Logger;

// TODO: Make this an interface instead?
// Move FindSongPath and similar to a static utility class instead?

public class SongLoader : MonoBehaviour
{
    public static Type GetSongLoaderType(Song_Type type)
    {
        switch (type)
        {
            case Song_Type.AMPLITUDE: return typeof(AMP_SongLoader);
            case Song_Type.RHYTHMIC: return typeof(RHX_SongLoader);
            default: return null;
        }
    }

    // TODO: Don't mix / and \ characters!
    public string FindSongPath(string song_name, Song_Type song_type)
    {
        if (song_name.IsEmpty() && LogE("SongLoader: Song name empty!")) return null;

        List<string> lookup_paths = null;
        if (song_type == Song_Type.RHYTHMIC) lookup_paths = GameState.Variables.RHX_song_lookup_paths;
        else if (song_type == Song_Type.AMPLITUDE) lookup_paths = GameState.Variables.AMP_song_lookup_paths;

        if ((lookup_paths == null || lookup_paths.Count == 0) &&
            LogE("SongLoader: lookup paths are empty!")) return null;

        for (int i = 0; i < lookup_paths.Count; ++i)
        {
            string p = lookup_paths[i];

            // if (p.Contains("<data>")
            p = p.Replace("<data>", Application.dataPath);

            if (!Directory.Exists(p) && LogW("Lookup dir doesn't exist: '%'", p))
                continue;

            string[] dirs = Directory.GetDirectories(p);
            // foreach (string d in dirs) Log("ASD: [%]: %", i, d);

            string target_dir = "";
            foreach (string d in dirs)
            {
                if (!d.EndsWith(song_name)) continue;

                target_dir = d;
                Log("Target dir: %", target_dir);

                return target_dir;
            }
        }

        LogE("Target dir was not found for song %", song_name);
        return null;
    }

    public virtual Song LoadSong(string song_name) { return null; }
}
