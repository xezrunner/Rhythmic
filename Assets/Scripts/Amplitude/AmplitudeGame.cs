using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using static AmpTrack;

public static class AmplitudeGame
{
    // AMPLITUDE properties
    // @"H://HMXAMPLITUDE//Extractions//amplitude_ps4_extraction//ps4//songs";
    //public static string AMP_songFolder = string.Format("{0}//amp_songs", Application.dataPath);
    public enum AMP_FileExtension { mid, mogg, moggsong }

    public static string song_ogg_path = "H:\\HMXAMPLITUDE\\Extractions\\ps4_songs";

    public static string AMP_songFolder
    {
        get
        {
#if UNITY_STANDALONE
            string dataPath;
            if (Directory.Exists(@$"{Application.dataPath}\amp_songs"))
                dataPath = Application.dataPath + @"/amp_songs";
            else
                dataPath = @"H:/Unity projects/Rhythmic/Assets/amp_songs";
#elif UNITY_ANDROID
            string dataPath = Application.persistentDataPath;
#endif
            return string.Format("{0}", dataPath);
        }
    }
    public static string AMP_GetSongFilePath(string songName, AMP_FileExtension extension)
    {
        return string.Format(@"{0}\{1}\{1}.{2}", AMP_songFolder, songName, extension);
    }

    // This list contains the note numbers that correspond to each lane, for each difficulty level
    public static List<int[]> difficultyNoteNumbers = new List<int[]>()
    {
        // Beginner
        new int[3] {96,98,100},
        // Intermediate
        new int[3] {102,104,106},
        // Advanced
        new int[3] {108,110,112},
        // Expert
        new int[3] {114,116,118}
    };

    public static int[] CurrentNoteNumberSet { get { return difficultyNoteNumbers[(int)RhythmicGame.Difficulty]; } }

    public static LaneSide GetLaneTypeFromNoteNumber(int num)
    {
        int? index = Array.IndexOf(CurrentNoteNumberSet, num);

        if (index == null)
        { Debug.LogErrorFormat("AMP_TRACK: Couldn't find the lane type for note number {0}!", num); return LaneSide.UNKNOWN; }

        switch (index)
        {
            case 0: // left
                return LaneSide.Left;
            case 1: // center
                return LaneSide.Center;
            case 2: // right
                return LaneSide.Right;

            default:
                return LaneSide.UNKNOWN;
        }
    }
}
