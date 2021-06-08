using System.Collections.Generic;
using UnityEngine;

public class SongInfo
{
    // :Info
    public string song_name;
    public string song_world;
    public float song_bpm;
    public float song_length;
    public int song_countin;
    public int song_length_bars;
    public List<string> song_tracks;
    public List<string> midi_tracks;
    
    // :Props
    public int[] checkpoint_bars;
    public float[] synesth_rate;
    public float slowmo_rate = 0.75f;
    public float tunnel_scale = 1.0f;
    
    /// Data:
    public SongTimeUnit time_units;
    
    // Notes:
    public int total_note_count;
    public MetaNote[,][] data_notes;

    // TODO: more...?
}