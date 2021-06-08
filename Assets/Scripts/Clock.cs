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
        time_units = SongController.song_info.time_units;
    }
    
    // Clocks
    public float seconds; // This is fed by SongController's mainAudioSource
    public float tick;
    public float bar;
    public float beat;
    public float subbeat;
    public float zPos;
    
    // F stands for Floor (TODO: ambigious between float?) | TODO: Performance!
    public int Fbar { get { return Mathf.FloorToInt(bar); } } // TODO: Naming confusion for float?
    public int Fbeat { get { return Mathf.FloorToInt(beat); } }

#if TICK_EVENTS
    int lastTick = -1;
#endif
    int lastBar = -1;
    int lastBeat = -1;
    int lastSubbeat = -1;

#if TICK_EVENTS
    public event EventHandler<int> OnTick;
#endif
    public event EventHandler<int> OnBar;
    public event EventHandler<int> OnBeat;
    public event EventHandler<int> OnSubbeat;
    
    //public event EventHandler OnPlayerSlop;
    
    // Main clock loop
    void Update()
    {
        if (!SongController) return;
        if (!SongController.is_playing) return;
        
        // Smoothly interpolate clock ticks
        float step = 1 * SongController.song_time_scale * Time.unscaledDeltaTime;
        
        // TODO: we somehow want this to mvoe in sync with the songposition, while still being smooth.
        //seconds = Mathf.MoveTowards(SongController.songPosition, SongController.songLength, step);
        seconds = Mathf.MoveTowards(seconds, SongController.song_info.song_length_sec, step); // Main clock value is seconds
        
        // Set tick, bar, beat and subbeat values based on seconds
        tick = time_units.tick_in_sec * seconds; // 1
        bar = tick / time_units.bar_ticks; // 1920
        beat = tick / time_units.beat_ticks; // 480
        //subbeat = tick / SongController.subbeatTicks; // 240
        zPos = time_units.pos_in_tick * tick;
        
        // Invoke events if last integer values aren't the same as current (changed!)
#if TICK_EVENTS
        if ((int)tick != lastTick) OnTick?.Invoke(this, (int)tick);
#endif
        if ((int)bar != lastBar) OnBar?.Invoke(this, (int)bar);
        if ((int)beat != lastBeat) OnBeat?.Invoke(this, (int)beat);
        if ((int)subbeat != lastSubbeat) OnSubbeat?.Invoke(this, (int)subbeat);
        
        // Update last ticks
#if TICK_EVENTS
        lastTick = (int)tick;
#endif
        lastBar = (int)bar;
        lastBeat = (int)beat;
        lastSubbeat = (int)subbeat;
        
        if (bar > SongController.song_info.song_length_bars)
            SongController.is_song_over = true;
    }
}