using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SongController : MonoBehaviour
{
    public static SongController Instance;
    Player Player { get { return Player.Instance; } }
    TracksController TracksController { get { return TracksController.Instance; } }

    // Controller properties
    public string defaultSong;
    public string songName;
    public virtual string songFolder { get; set; }

    public virtual List<string> songTracks { get; set; }

    public bool IsFake = false;
    public bool Enabled = true;

    // Song properties
    public int songBpm;
    public float secPerBeat;
    public int songCountIn;
    public float songLength;
    public int songLengthInMeasures;
    public float songFudgeFactor; // Default is 0

    // Playback properties
    public List<AudioSource> audioSrcList = new List<AudioSource>();
    public float songOffset;

    public Vector3 beatHaptics;

    // Time & unit calculations
    #region Time & unit calculations
    /*---- SONG UNITS ------
    Beat (Quarter note): One beat's length in ticks | 1 beat = 480 ticks
    Measure (Bar): 4 beats' length in ticks | 1 measure = 480 * 4 ticks (1920 ticks)
    ----- GENERIC UNITS -----
    s (Seconds)
    ms (Milliseconds)
    zPos (Meters)
    */

    // Common units
    public int beatTicks = 480;
    public int measureTicks { get { return beatTicks * 4; } }
    public int subbeatTicks { get { return beatTicks / 2; } }

    // ms (Milliseconds)
    public float tickInMs { get { return 60000f / (songBpm * beatTicks); } }
    public float MsInTick { get { return tickInMs * (songBpm * beatTicks) / 60000f; } }
    // s (Seconds)
    public float tickInSec { get { return 60f / (songBpm * beatTicks); } }
    public float secInTick { get { return tickInSec * (songBpm * beatTicks) / 60f; } }
    // zPos (Meters)
    public float tickInzPos { get { return tickInSec / (tickInSec * beatTicks) * 4 / songFudgeFactor; } }
    public float zPosInTick { get { return (tickInzPos / 4f * (tickInSec * beatTicks)) * (songBpm * beatTicks); } }

    // Converters
    public float TickTimeToMs(float tick = 1f) { return tickInMs * tick; }
    public float MsToTickTIme(float ms = 1f) { return MsInTick * ms; }

    public float TickTimeToSec(float tick = 1f) { return tickInSec * tick; }
    public float SecToTickTime(float sec = 1f) { return secInTick * sec; }

    public float TickTimeTozPos(float tick = 1f) { return tickInzPos * tick; }
    public float zPosToTickTime(float zPos = 1f) { return zPosInTick * zPos; }
    #endregion

    // INIT & LOADING

    public virtual void Awake() => Instance = this;
    public virtual void Start()
    {
        // TODO: The loading sequence or RhythmicGame itself should manage the loading scene!
        // It's possible that GameStarter might not even be needed, or we'll have a different loading mechanism once we'll have UI.
        // For now, we use the SongController to manage the scenes during and after loading.
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("DevScene"));

        if (!Enabled)
        { Debug.LogWarningFormat("SongCtrl: Disabled"); return; }
        if (IsFake) // TODO: fake song information implementation!
        { Debug.LogWarningFormat("SongCtrl: running in fake mode!"); return; }

        // TODO: Loading the song should not be based on what property is set!
        // LoadSong() should be called by the UI or loading mechanism!
        LoadSong(songName == null ? defaultSong : songName); // load default song in case the prop is empty, for testing purposes only!
    }

    public event EventHandler<float> LoadingProgress;
    public event EventHandler LoadingFinished;

    public virtual void LoadSong(string song)
    {
        // Set basic song metadata
        songName = song;
        /* Handle rest of the loading in the game-specific song controller */
    }

    // TODO: implementation
    // This will load in a fake song for testing purposes (class: FakeSongInfo)
    public void LoadFakeSong() { }

    // GAMEPLAY

    // Vibrate controller to the beat
    public void BeatVibration()
    {
        VibrationController.VibrateLinear(beatHaptics);
    }
}