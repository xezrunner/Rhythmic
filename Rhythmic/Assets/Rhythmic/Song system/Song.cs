using System.Collections.Generic;
using static Logger;

public class Song
{
    public string name;
    public string path;

    public int length_bars;
    public int countin;
    public float bpm;

    public List<Song_Track> tracks;
    public int track_count;

    public string world_name;
    public float tunnel_scale = 1.0f;

    public List<int> enable_order;
    public List<int> section_bars;
    public List<float[]> score_goals;

    public Song_TimeUnits time_units;

    // TODO: Metadata...
}

public class Song_TimeUnits
{
    public Song_TimeUnits(float bpm, float tunnel_scale = 1.0f) // tunnel_scale is TEMP!!
    {
        if (bpm <= 0 && LogE("Invalid BPM!".TM(this))) return;

        /// TODO: Tunnel scale!!!

        beat_per_ms = (bpm / 60000f);
        ms_per_beat = (60000f / bpm);
        sec_per_beat = (60f / bpm);

        // ticks:
        tick_in_ms = (bpm * Variables.beat_ticks) / 60000f;
        tick_in_sec = (bpm * Variables.beat_ticks) / 60f;

        // ms:
        ms_in_tick = 60000f / (bpm * Variables.beat_ticks);
        ms_in_beat = ms_in_tick * Variables.beat_ticks;
        ms_in_bar = ms_in_tick * Variables.bar_ticks;

        // sec:
        sec_in_tick = 60f / (bpm * Variables.beat_ticks);
        sec_in_beat = sec_in_tick * Variables.beat_ticks;
        sec_in_bar = sec_in_tick * Variables.bar_ticks;

        // pos:
        pos_in_tick = (4f / 480f) + ((4f / 480f) * tunnel_scale); // ???
        pos_in_beat = pos_in_tick * Variables.beat_ticks;
        pos_in_bar = pos_in_tick * Variables.bar_ticks;
        pos_in_ms = (4f / ms_per_beat) + ((4f / ms_per_beat) * tunnel_scale);    // ???
        pos_in_sec = (4f / sec_per_beat) + ((4f / sec_per_beat) * tunnel_scale); // ???

        // remainder:
        ms_in_pos =   ((4f * ms_per_beat));
        sec_in_pos  = ((4f * sec_per_beat));
        tick_in_pos = ((4f * Variables.beat_ticks));

        return;
    }

    public float beat_per_ms;
    public float ms_per_beat;
    public float sec_per_beat;

    // ticks:
    public float tick_in_ms;
    public float tick_in_sec;
    public float tick_in_pos;

    // ms:
    public float ms_in_tick;
    public float ms_in_beat;
    public float ms_in_bar;
    public float ms_in_pos;

    // sec:
    public float sec_in_tick;
    public float sec_in_beat;
    public float sec_in_bar;
    public float sec_in_pos;

    // pos:
    public float pos_in_tick;
    public float pos_in_sec;
    public float pos_in_ms;
    public float pos_in_beat;
    public float pos_in_bar;
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

public class Song_Track
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