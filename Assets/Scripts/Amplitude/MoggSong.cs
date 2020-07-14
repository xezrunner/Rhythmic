using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using UnityEngine;
using UnityEditor.Rendering.PostProcessing;
using UnityEditor;
using System.Linq;
using UnityEngine.SocialPlatforms;

public class MoggSong : MonoBehaviour
{
    public string Moggsong { get; set; }

    // Song properties
    public int songLengthInMeasures { get; set; }
    public int songCountInTime { get; set; }
    public List<string> songTracks { get; set; } = new List<string>();
    public float songBpm { get; set; }

    // TODO: Mixer!
    // TODO: Score goals!

    // Fudge Factor
    // If a song doesn't have a fudge factor defined, assume it's 1.
    // TODO: have it just simply be assigned to be 1 by defaulT??
    float? _songFudgeFactor;
    public float songFudgeFactor { get { if (!_songFudgeFactor.HasValue) return 1f; else return _songFudgeFactor.Value; } set { _songFudgeFactor = value; } }

    public int[] songEnableOrder { get; set; }
    public int[] songSectionStartBars { get; set; }

    public int songBossLevel { get; set; }

    // TODO: Metadata!

    public void LoadMoggSong(string songName)
    {
        string finalPath = RhythmicGame.AMP_GetSongFilePath(songName, RhythmicGame.AMP_FileExtension.moggsong);

        char[] CRLF = new char[2] { '\n', '\r' };
        TextReader tr = File.OpenText(finalPath);
        string[] fileLines = tr.ReadToEnd().Split(CRLF);

        int counter = 0;
        foreach (string line in fileLines) // TODO: re-do to use tokens / use TrimStart as well?
        {
            if (line.Contains("length "))
                songLengthInMeasures = int.Parse(line.Substring(line.IndexOf("length ") + 7, 3));
            else if (line.Contains("countin "))
                songCountInTime = int.Parse(line.Substring(line.IndexOf("countin ") + 8, 1));
            else if (line.Contains("tunnel_scale ")) // fudge factor!
                songFudgeFactor = float.Parse(line.Substring(14, 3), CultureInfo.InvariantCulture.NumberFormat);
            else if (line.Contains("bpm "))
                songBpm = float.Parse(line.Substring(5, 3));
            else if (line.Contains("boss_level "))
                songBossLevel = int.Parse(line.Substring(12, 1));

            else if (line.Contains("SONG_BUS")) // track
            {
                string[] tokens = line.Substring(7).Split(' ');
                string trackName = tokens[0];
                songTracks.Add(trackName);
            }

            counter++;
        }
    }
}
