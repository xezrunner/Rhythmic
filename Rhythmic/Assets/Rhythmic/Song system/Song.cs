using System.Collections.Generic;
using UnityEngine;
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
        beat_per_sec = (bpm / 60f);
        ms_per_beat = (60000f / bpm);
        sec_per_beat = (60f / bpm);

        // ticks:
        tick_in_ms = beat_per_ms * Variables.beat_ticks;
        tick_in_sec = beat_per_sec * Variables.beat_ticks;

        // ms:
        ms_in_tick = ms_per_beat / Variables.beat_ticks;
        ms_in_beat = ms_in_tick * Variables.beat_ticks;
        ms_in_bar = ms_in_tick * Variables.bar_ticks;

        // sec:
        sec_in_tick = sec_per_beat / Variables.beat_ticks;
        sec_in_beat = sec_in_tick * Variables.beat_ticks;
        sec_in_bar = sec_in_tick * Variables.bar_ticks;

        // pos:
        pos_in_sec = Variables.UNITS_MetersPerSecond * (1f + tunnel_scale);
        pos_in_tick = pos_in_sec * sec_in_tick;

        pos_in_beat = pos_in_tick * Variables.beat_ticks;
        pos_in_bar = pos_in_tick * Variables.bar_ticks;
        pos_in_ms = 4 * ms_per_beat; //(4f / ms_per_beat)   + ((4f / ms_per_beat) ) ;  //* tunnel_scale);    // ???

        // remainder:
        ms_in_pos = (4f * ms_per_beat);
        sec_in_pos = (4f * sec_per_beat);
        tick_in_pos = (4f * Variables.beat_ticks);
    }

    public float beat_per_ms;
    public float beat_per_sec;
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

// TODO: Custom names for instruments
public class Song_Instrument
{
    public Song_Instrument(InstrumentType instr, string custom_name = null, Color? color = null)
    {
        instrument = instr;
        this.custom_name = (custom_name != null) ? custom_name : instr.ToString();
        this.color = (color.HasValue) ? color.Value : GetColorForInstrument(instr);
    }

    public InstrumentType instrument = InstrumentType.UNKNOWN;
    public string custom_name = null;
    public Color color;

    public static Color GetColorForInstrument(InstrumentType instr)
    {
        switch (instr)
        {
            case InstrumentType.Drums:
                return Colors.RGBToFloat(255, 61, 246);
            case InstrumentType.Bass:
                return Colors.RGBToFloat(9, 79, 255);
            case InstrumentType.Synth:
            case InstrumentType.FX:
                return Colors.RGBToFloat(218, 195, 43);
            case InstrumentType.Guitar:
                return Colors.RGBToFloat(213, 21, 11);
            case InstrumentType.Vocals:
                return Colors.RGBToFloat(32, 202, 45);
            default: return Color.white;
        }
    }
}

public enum InstrumentType
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
    public List<Song_Note>[] notes; // Array of (list of notes) by measures

    public string audio_path;
    public int[] stereo_mix; // AMP-only?

    // TODO: instrument seq data!
}

public struct Song_Note
{
    public Song_Note(int code, int lane, long ticks, Song_TimeUnits time_units)
    {
        this.lane = lane;
        this.code = code;

        this.ticks = ticks;
        bar = (int)(ticks / Variables.bar_ticks);

        long ticks_after_bar = ticks - (bar * Variables.bar_ticks);
        pos = time_units.pos_in_tick * ticks_after_bar;
    }

    public int lane;
    public int code;

    public long ticks;
    public int bar;
    public float pos;
}