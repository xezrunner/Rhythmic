using System;
using UnityEngine;
using static Logger;

public class Clock : MonoBehaviour
{
    SongSystem song_system;
    AudioSystem audio_system;
    public static Clock Instance;

    public void SetupClock(SongSystem song_system)
    {
        if (Instance && LogE("An instance of Clock already exists!"))
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        this.song_system = song_system;
        audio_system = song_system.audio_system;
    }

    public float seconds;
    public float seconds_smooth;
    public float beat;
    public float bar;
    public float pos;
    public float pos_smooth;

    public float smooth_factor = 0.1f;

    [NonSerialized] public bool is_testing = true; // TEMP
    public float testing_speed;
    void Update()
    {
        if (!audio_system.is_playing || !is_testing) return;

        if (is_testing)
            seconds += Time.deltaTime * testing_speed;


    }
}

public enum TimeUnit
{
    AbsoluteTicks = 0, absolute = 0, ticks = 0,
    Hours = 1, h = 1,
    Minutes = 2, m = 2,
    Seconds = 3, s = 3,
    Milliseconds = 4, ms = 4,
    Meters = 5,
    Beats = 6, Bars = 7
}