using System.Collections.Generic;

public class Song
{
    public string name;
    public string path;

    public int length_bars;
    public int countin;

    public List<Song_Track> tracks;
    public int track_count;

    public string world_name;
    public float tunnel_scale = 1.0f;

    public List<int> enable_order;
    public List<int> section_bars;
    public List<float[]> score_goals;

    // TODO: Metadata...
}

// TODO TODO TODO:
// Song_Instrument should be a class that has custom name,
// color and other information!!!
public enum Song_Instrument
{
    UNKNOWN = -1,
    Drums = 0, D = 0,
    Bass = 1, B = 1,
    Synth = 2, S = 2,
    Guitar = 3, G = 3,
    FX = 4,
    Vocals = 5, V = 5,
    Freestyle = 7
}

public struct Song_Track
{
    public int id;
    public string name;
    public Song_Instrument instrument;
    public List<Song_Note> notes;

    public string audio_path;
    public int[] stereo_mix; // AMP-only?

    // TODO: instrument seq data!
}

public struct Song_Note
{
    public long pos_ticks;
    public int lane;
}