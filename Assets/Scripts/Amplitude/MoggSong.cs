using System.Collections.Generic;
using System.IO;
using System.Globalization;
using UnityEngine;

public class MoggSong : MonoBehaviour
{
    public string Moggsong { get; set; }

    // Song properties
    public int songLengthInMeasures { get; set; }
    public int songCountInTime { get; set; }
    public List<string> songTracks { get; set; } = new List<string>();
    public int songBpm { get; set; }

    // TODO: Mixer!
    // TODO: Score goals!

    // Fudge Factor
    // If a song doesn't have a fudge factor defined, assume it's 0.
    float? _songFudgeFactor;
    public float songFudgeFactor { get { if (!_songFudgeFactor.HasValue) return 0f; else return _songFudgeFactor.Value; } set { _songFudgeFactor = value; } }

    // TOOD: Unknown props
    public int[] songEnableOrder { get; set; }
    public int[] songSectionStartBars { get; set; }

    // TODO: Metadata!

    // Song boss level
    // n + 1 streaks required
    public int songBossLevel { get; set; }

    // TODO: IMPROVE THIS BY USING TOKENS FOR EVERYTHING!
    // TODO: IMPROVE THIS BY USING SWITCH STATEMENTS (?)
    public void LoadMoggSong(string songName)
    {
        string finalPath = RhythmicGame.AMP_GetSongFilePath(songName, RhythmicGame.AMP_FileExtension.moggsong); // moggsong path
        char[] CRLF = new char[2] { '\n', '\r' }; // newline characters

        TextReader tr = File.OpenText(finalPath);
        string[] fileLines = tr.ReadToEnd().Split(CRLF); // create array of moggsong lines

        int counter = 0;
        foreach (string line in fileLines) // go through each line
        {
            // TODO: use TrimStart?

            if (line.Contains("length ")) // Song length
            {
                int index = line.IndexOf("length ");
                string[] tokens = line.Substring(index + 7, line.Length - (7 + index)).Split(':');
                songLengthInMeasures = int.Parse(tokens[0]);

                if (line.Contains("LESS THAN")) // moggsongs report 4 less than actual, although tutorial doesn't
                    songLengthInMeasures += 4;
            }
            else if (line.Contains("countin ")) // TODO: this might be the accountation for song length reporting 4 less than actual. But why?
                songCountInTime = int.Parse(line.Substring(line.IndexOf("countin ") + 8, 1));
            else if (line.Contains("tunnel_scale ")) // Song fudge factor
                songFudgeFactor = float.Parse(line.Substring(14, 3), CultureInfo.InvariantCulture.NumberFormat);
            else if (line.Contains("bpm ")) // Song BPM | TODO: this is really hacky!
            {
                string finalBPM = line.Substring(5, 3);

                if (finalBPM.EndsWith(")"))
                    finalBPM = line.Substring(5, 2);

                songBpm = int.Parse(finalBPM);
            }
            else if (line.Contains("boss_level ")) // Song boss level
                songBossLevel = int.Parse(line.Substring(12, 1));

            else if (line.Contains("SONG_BUS") || line.Contains("FREESTYLE_FX")) // Song tracks
            {
                string[] tokens = line.Substring(7).Split(' ');
                string trackName = tokens[0];
                if (trackName == "freestyle" & !RhythmicGame.PlayableFreestyleTracks)
                    continue;
                else
                    songTracks.Add(trackName);
            }

            counter++;
        }
    }
}