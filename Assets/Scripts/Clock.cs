/* Not allowing tick events due to concerns with performance. */
//#define TICK_EVENTS 
#undef TICK_EVENTS

using System;
using UnityEngine;

public class Clock : MonoBehaviour
{
    public static Clock Instance;
    GenericSongController SongController;

    SongTimeUnit time_units;

    void Awake()
    {
        Instance = this;
        SongController = GenericSongController.Instance;
        time_units = SongController.time_units;
    }

    // Clocks
    // TODO / NOTE: These should be NonSerialized:
    public float seconds; // This is fed by SongController's mainAudioSource
    [NonSerialized] public float seconds_smooth; // This is fed by SongController's mainAudioSource
    [NonSerialized] public float tick;
    public float bar;
    public float beat;
    public float pos;
    [NonSerialized] public float pos_smooth;
    //public float subbeat;

    // F stands for Floor (TODO: ambigious between float?) | TODO: Performance!
    public int Fbar { get { return Mathf.FloorToInt(bar); } } // TODO: Naming confusion for float?
    public int Fbeat { get { return Mathf.FloorToInt(beat); } }
    
    float last_song_position;
    public float song_deltatime;
    public float song_deltatime_smooth;

#if TICK_EVENTS
    int lastTick = -1;
#endif
    int lastBar = -1;
    int lastBeat = -1;
    //int lastSubbeat = -1;

#if TICK_EVENTS
    public event EventHandler<int> OnTick;
#endif
    public event EventHandler<int> OnBar;
    public event EventHandler<int> OnBeat;
    //public event EventHandler<int> OnSubbeat;
    
    //public event EventHandler OnPlayerSlop;
    
    public float smooth_factor = 0.1f;
    
    // Main clock loop
    void Update()
    {
        if (!SongController) return;
        if (!SongController.is_playing) return;
        
        //seconds = Mathf.MoveTowards(seconds, SongController.song_info.song_length_sec, step);
        seconds = SongController.song_position; // We want this to be accurate.
        
        float delta = Time.fixedDeltaTime; // The time elapsed since the last frame.
        float skew = SongController.song_position - seconds_smooth - delta; // The difference in time between the song and the current Clock time
        float smooth_delta = delta + smooth_factor * skew; // Smoothen the difference with + (factor * skew)
        seconds_smooth += delta + smooth_factor * skew; // The smoothened seconds equal fixedDeltaTime + (factor * skew)
        
        song_deltatime = SongController.song_position - last_song_position;
        song_deltatime_smooth = smooth_delta; // TODO: Clear up naming here?
        last_song_position = SongController.song_position;
        
        // Set tick, bar, beat and subbeat values based on seconds
        tick = time_units.tick_in_sec * seconds; // 1
        bar = tick / time_units.bar_ticks; // 1920
        beat = tick / time_units.beat_ticks; // 480
        //subbeat = tick / SongController.subbeatTicks; // 240
        pos = time_units.pos_in_tick * tick;
        pos_smooth = time_units.pos_in_sec * seconds_smooth; // time_units.pos_in_sec * seconds_smooth;

        // Invoke events if last integer values aren't the same as current (changed!)
#if TICK_EVENTS
        if ((int)tick != lastTick) OnTick?.Invoke(this, (int)tick);
#endif
        if ((int)bar != lastBar) OnBar?.Invoke(this, (int)bar);
        if ((int)beat != lastBeat) OnBeat?.Invoke(this, (int)beat);
        //if ((int)subbeat != lastSubbeat) OnSubbeat?.Invoke(this, (int)subbeat);
        
        // Update last ticks
#if TICK_EVENTS
        lastTick = (int)tick;
#endif
        lastBar = (int)bar;
        lastBeat = (int)beat;
        //lastSubbeat = (int)subbeat;

        if (bar > SongController.song_info.song_length_bars)
            SongController.is_song_over = true;
    }
}