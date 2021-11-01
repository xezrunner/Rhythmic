using System;
using UnityEngine;

public class Clock : MonoBehaviour
{
    public static Clock Instance;

    public GameVariables Vars = GameState.Variables;
    public SongSystem SongSystem;
    public Song Song;

    public float seconds, ms;
    public float seconds_deltatime, ms_deltatime;
    [NonSerialized] public float seconds_smooth, ms_smooth;
    [NonSerialized] public float seconds_dt_smooth_factor = 0.1f;
    [NonSerialized] public float seconds_smooth_factor = 0.1f;
    float seconds_last;

    public ulong ticks;
    public int bar;
    public int beat;
    public float pos;

    void Awake()
    {
        Instance = this;
        SongSystem = SongSystem.Instance;
        Song = SongSystem.song;
    }

    void Update()
    {
        if (!SongSystem) return;
        if (!SongSystem.is_playing) return;

        seconds = SongSystem.song_progress_sec;
        ms = SongSystem.song_progress_sec * 1000f;

        // Smoothen seconds:
        float delta = Time.fixedDeltaTime; // The time elapsed since the last frame.
        float skew = SongSystem.song_progress_sec - seconds_smooth - delta; // The difference in time between the song and the current Clock time
        float smooth_delta = delta + seconds_smooth_factor * skew; // Smoothen the difference with + (factor * skew)
        seconds_smooth += delta + seconds_smooth_factor * skew; // The smoothened seconds equal fixedDeltaTime + (factor * skew)

        ms_smooth = seconds_smooth * 1000f;
        seconds_deltatime = SongSystem.song_progress_sec - seconds_last;
        seconds_last = SongSystem.song_progress_sec;

        // Calculate unit values:
        ticks = (ulong)(ms * Song.time_info.tick_per_ms);
        // TODO: GameVariables!
        // TODO: Figure out what to do with the casting situation here:
        bar = ( (int)ticks / Vars.bar_ticks);
        beat = ((int)ticks / Vars.beat_ticks);
        pos = ms * Song.time_info.pos_per_ms;
    }
}
