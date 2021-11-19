using System;
using UnityEngine;
using static Logger;

public class Clock : MonoBehaviour
{
    public static Clock Instance;

    SongSystem song_system;
    Song_TimeUnits time_units;
    AudioSystem audio_system;

    public void SetupClock(SongSystem song_system)
    {
        if (Instance && LogE("An instance of Clock already exists!"))
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        this.song_system = song_system;
        time_units = song_system.song.time_units;
        audio_system = song_system.audio_system;
    }

    public float tick;
    public float seconds;
    public float seconds_smooth;
    public float ms;
    public float beat;
    public float bar;
    public float pos;
    public float pos_smooth;

    public float smooth_factor = 0.1f;

    public bool is_testing = true; // TEMP
    public Transform cube;
    void Update()
    {
        if (!is_testing) return;
        seconds += Time.deltaTime;

        // ...

        tick = time_units.tick_in_sec * seconds;
        ms = seconds * 1000f;
        beat = time_units.beat_per_ms * ms; // tick / Variables.beat_ticks;
        bar = tick / Variables.bar_ticks;
        pos = time_units.pos_in_sec * seconds;

        if (!cube) cube = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;

        cube.position = PathTransform.pathcreator_global.path.XZ_GetPointAtDistance(pos);
        cube.rotation = PathTransform.pathcreator_global.path.XZ_GetRotationAtDistance(pos);

        Camera.main.transform.position = (cube.position + (-cube.forward * 30f) + (cube.up * 10f));
        Camera.main.transform.rotation = cube.rotation * Quaternion.Euler(13, 0, 0);
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