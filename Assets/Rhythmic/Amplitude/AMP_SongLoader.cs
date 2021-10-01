using System.IO;
using static Logger;

public class AMP_SongLoader : SongLoader
{
    public const string AMP_MoggSongExtension = ".moggsong";
    public const string AMP_MoggExtension = ".mogg";
    public const string AMP_MidiExtension = ".mid";

    public static bool AMP_Test_UsePrecompiledMusic = true; // TEST!

    public override Song LoadSong(string song_name)
    {
        Log("Loading AMP song: '%'...", song_name);

        string song_path = FindSongPath(song_name, Song_Type.AMPLITUDE);
        if (song_path == null && LogE("Could not find AMP song: %", song_name))
            return null;

        string[] files = Directory.GetFiles(song_path);
        string amp_song_file_path = null;

        foreach (string f in files)
        {
            if (f.GetExt() != AMP_MoggSongExtension) continue;
            amp_song_file_path = f;
            break;
        }

        Song song = LoadSongFile(amp_song_file_path);

        return song;
    }

    Song LoadSongFile(string file_path)
    {
        AMP_MoggSong moggsong = new AMP_MoggSong(file_path);


        return null;
    }
}
