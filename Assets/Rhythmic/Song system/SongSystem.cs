using System;
using UnityEngine;
using static Logger;

public class SongSystem : MonoBehaviour
{
    public static SongSystem Instance;

    public string song_to_load;

    public Song song;
    public float tunnel_scale_global;

    Clock current_clock;

    // TODO: move these elsewhere?
    public bool is_playing; 
    public float song_progress_sec;

    public void Awake()
    {
        Instance = this;
    }

    SongLoader song_loader;
    void AddSongLoader(Song_Type type)
    {
        Type type_to_add = SongLoader.GetSongLoaderType(type);
        song_loader = (SongLoader)gameObject.AddComponent(type_to_add);
    }
    void RemoveSongLoader() => Destroy(song_loader);

    public void InitializeSong(string song_name = null, Song_Type song_type = Song_Type.RHYTHMIC)
    {
        if (song_name == null) song_name = song_to_load; // test

        if (song_loader) RemoveSongLoader();
        AddSongLoader(song_type);

        song = song_loader.LoadSong(song_name);

        CreateClock();

        if (song == null && LogE("Failed to load song %".TM(this), song_name)) return;
        Log("Current song: %".T(this), song_name); // TODO: grab name from the song itself
    }

    void CreateClock()
    {
        if (current_clock) Destroy(current_clock);
        current_clock = gameObject.AddComponent<Clock>();
    }
}