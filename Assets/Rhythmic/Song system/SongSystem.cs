using UnityEngine;
using static Logger;

public class SongSystem : MonoBehaviour
{
    public static SongSystem Instance;

    public string song_to_load;

    public Song song;
    public TimeUnit song_time_unit;
    public float tunnel_scale_global;

    public void Awake()
    {
        Instance = this;
    }

    public void InitializeSong(string song_name = null, Song_Type song_type = Song_Type.RHYTHMIC)
    {
        if (song_name == null) song_name = song_to_load; // test

        if (song_type == Song_Type.RHYTHMIC)  song = RHX_SongLoader.LoadSong(song_name);
        if (song_type == Song_Type.AMPLITUDE) song = AMP_SongLoader.LoadSong(song_name);

        if (song == null && LogE("Failed to load song %".TM(this), song_name)) return;
        Log("Current song: %".T(this), song_name); // TODO: grab name from the song itself
    }
}