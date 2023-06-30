using UnityEngine;
using UnityEngine.InputSystem;
using static Logger;

public class Clock : MonoBehaviour {
    public static Clock Instance;

    SongSystem  song_system;
    AudioSystem audio_system;
    TrackSystem track_system;
    PlayerTrackSwitching player_trackswitch;

    public Song_TimeUnits time_units;

    void Start() {
        track_system = TrackSystem.Instance;
        player_trackswitch = PlayerTrackSwitching.Instance;
    }

    public void SetupClock(SongSystem song_system) {
        if (Instance && LogE("An instance of Clock already exists!")) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        this.song_system = song_system;
        time_units = song_system.song.time_units;
        audio_system = song_system.audio_system;
    }

    public long ticks;
    public float seconds;
    public float seconds_smooth;
    public float ms;
    public float beat;
    public float bar;
    public float pos;
    public float pos_smooth;

    public float smooth_factor = 0.1f;

    public bool is_scrubbing = false; // TEMP
    public Transform cube;

    float seconds_ref;
    void Update() {
        // TEMP:
        if (Keyboard.current != null) {
            if (Keyboard.current.spaceKey.wasPressedThisFrame) {
                audio_system.is_playing = !audio_system.is_playing;

                if (audio_system.is_playing) audio_system.AUDIO_Play();
                else audio_system.AUDIO_Pause();
            }

            is_scrubbing = Keyboard.current.altKey.isPressed;

            if (Keyboard.current.altKey.isPressed && Keyboard.current.numpad9Key.isPressed)
                seconds += (Keyboard.current.ctrlKey.isPressed ? 0.8f : 6f) * Time.deltaTime;
            if (Keyboard.current.altKey.isPressed && Keyboard.current.numpad3Key.isPressed)
                seconds -= (Keyboard.current.ctrlKey.isPressed ? 0.8f : 3f) * Time.deltaTime;

            if (Keyboard.current.numpad8Key.wasPressedThisFrame) {
                if (!audio_system.is_playing) seconds += (time_units.sec_in_bar * 4);
                else audio_system.audio_progress += (time_units.sec_in_bar * 4);
            }
            if (Keyboard.current.numpad7Key.wasPressedThisFrame) {
                seconds = (track_system.next_notes[player_trackswitch.current_track_id].ms / 1000f);
            }

        }

        if (audio_system.is_playing)
            seconds = Mathf.SmoothDamp(seconds, audio_system.audio_progress, ref seconds_ref, 0.1f);
        //seconds = audio_system.audio_progress;
        //seconds += audio_system.audio_deltatime;
        //seconds += Time.deltaTime;


        // ...

        ticks = (long)(time_units.tick_in_sec * seconds);
        ms = seconds * 1000f;
        beat = time_units.beat_per_ms * ms; // tick / Variables.beat_ticks;
        bar = ((float)ticks / Variables.bar_ticks);
        pos = time_units.pos_in_sec * seconds;
    }
}

public enum TimeUnit {
    AbsoluteTicks = 0, absolute = 0, ticks = 0,
    Hours         = 1, h  = 1,
    Minutes       = 2, m  = 2,
    Seconds       = 3, s  = 3,
    Milliseconds = 4,  ms = 4,
    Meters = 5,
    Beats  = 6, Bars = 7
}