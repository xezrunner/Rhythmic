/* Not allowing tick events due to concerns with performance. */
//#define TICK_EVENTS 
#undef TICK_EVENTS

using System;
using UnityEngine;

public class Clock : MonoBehaviour
{
    public static Clock Instance;
    SongController SongController;

    void Awake()
    {
        Instance = this;
        SongController = SongController.Instance;
    }

    // Clocks
    public float seconds; // This is fed by SongController's mainAudioSource
    public float tick;
    public float bar;
    public float beat;
    public float subbeat;
    public float zPos;

    // F stands for Floor (TODO: ambigious between float?)
    public int Fbar { get { return Mathf.FloorToInt(bar); } }
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

    public event EventHandler OnPlayerSlop;

    // Main clock loop
    void FixedUpdate()
    {
        if (!SongController.IsPlaying) return;

        // Smoothly interpolate clock ticks
        float step = 1 * SongController.songTimeScale * Time.unscaledDeltaTime;

        // TODO: we somehow want this to mvoe in sync with the songposition, while still being smooth.
        //seconds = Mathf.MoveTowards(SongController.songPosition, SongController.songLength, step);
        seconds = Mathf.MoveTowards(seconds, SongController.songLength, step); // Main clock value is seconds

        // Set tick, bar, beat and subbeat values based on seconds
        tick = SongController.tickInSec * seconds; // 1
        bar = tick / SongController.measureTicks; // 1920
        beat = tick / SongController.beatTicks; // 480
        subbeat = tick / SongController.subbeatTicks; // 240
        zPos = SongController.posInTick * tick;

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
    }
}
