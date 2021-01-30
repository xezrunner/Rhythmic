using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class SongController : MonoBehaviour
{
    public static SongController Instance;
    public Clock Clock;
    public TracksController AmpTrackController;

    // Controller properties
    public string defaultSong;
    public static string songName;
    public virtual string songFolder { get; set; }

    public List<string> songTracks = new List<string>();
    public List<List<KeyValuePair<int, MetaNote>>> songNotes = new List<List<KeyValuePair<int, MetaNote>>>();

    public bool IsFake = false;
    public bool Enabled = true;

    // Song properties
    public int songBpm;
    public int songCountIn;
    public float songLength; // in seconds
    public float songLengthInzPos;
    public int songLengthInMeasures;
    public float songFudgeFactor; // Default is 1

    public float SlopMs;
    public float SlopPos;

    // Playback properties
    public List<AudioSource> audioSrcList = new List<AudioSource>();
    public AudioSource mainAudioSource; // The BG_CLICK AudioSource

    bool _isPlaying;
    public bool IsPlaying { get { return _isPlaying; } set { _isPlaying = value; /*Player.IsPlaying = value;*/ } } // Sets both local and Player values

    public float songSpeed = 1f;
    public float songOffset;
    public float songPosition { get { return mainAudioSource.time; } } // Current song position in seconds | this is the BG_CLICK AudioSource's current time pos.
    public float songDspTime; // How many seconds have passed since the song started

    public Vector3 beatHaptics;

    /// Time & unit calculations
    #region Time & unit calculations
    /*---- SONG UNITS ------
    Beat (Quarter note): One beat's length in ticks | 1 beat = 480 ticks
    Measure (Bar): 4 beats' length in ticks | 1 measure = 480 * 4 ticks (1920 ticks)
    ----- GENERIC UNITS -----
    s (Seconds)
    ms (Milliseconds)
    zPos (Meters)
    */

    // Common 'meta' units
    public float beatPerSec { get { return songBpm / 60f; } }
    public float secPerBeat { get { return 60f / songBpm; } }

    public int beatTicks = 480;
    public int measureTicks { get { return beatTicks * 4; } }
    public int subbeatTicks { get { return beatTicks / 2; } }

    public float measureLengthInzPos { get { return TickToPos(measureTicks); } }
    public float subbeatLengthInzPos { get { return TickToPos(subbeatTicks); } }

    /// Individual time & position units
    // Ticks - how many ticks in a <unit>
    public float tickInSec;
    public float tickInMs;
    public float tickInPos;

    // Seconds - how many seconds in a <unit>
    public float secInTick;
    public float secInMs;
    public float secInPos;

    // Milliseconds (ms) - how many ms in a <unit>
    public float msInTick;
    public float msInSec;
    public float msInPos;

    // Meters (pos) - how many pos in a <unit>
    public float posInTick;
    public float posInSec;
    public float posInMs;

    /// Convertors
    // Ticks -> 
    public float TickToSec(float ticks) { return secInTick * ticks; }
    public float TickToMs(float ticks) { return msInTick * ticks; }
    public float TickToPos(float ticks) { return posInTick * ticks; }

    // Seconds ->
    public float SecToTick(float sec) { return tickInSec * sec; }
    public float SecToMs(float sec) { return msInSec * sec; }
    public float SecToPos(float sec) { return posInSec * sec; }

    // Milliseconds
    public float MsToTick(float ms) { return tickInMs * ms; }
    public float MsToSec(float ms) { return secInMs * ms; }
    public float MsToPos(float ms) { return posInMs * ms; }

    // Position (meters)
    public float PosToTick(float pos) { return tickInPos * pos; }
    public float PosToSec(float pos, bool scale = false) 
    {
        float value = secInPos * pos;
        if (scale) value *= 1.8f / songFudgeFactor;
        else value /= songFudgeFactor * 1.8f;

        return value;
    }
    public float PosToMs(float pos , bool scale = false) 
    {
        float value = msInPos * pos;
        if (scale) value *= 1.8f / songFudgeFactor;
        else value /= songFudgeFactor * 1.8f;

        return value;
    }

    /// Miscellaneous converters
    public float MeasureToPos(int measureID) { return StartPosition + (measureID * measureLengthInzPos); }

    // zPos to measure/subbeat num conversion
    // TODO: revise
    // Gets the measure number for a z position (Rhythmic Game unit)
    public int GetMeasureNumForZPos(float zPos)
    {
        /*
        zPos = (float)Math.Round(zPos, 1);
        foreach (MeasureInfo measure in songMeasures)
        {
            float endTimeInZPos = (float)Math.Round(measure.endTimeInzPos, 1);
            if (zPos < endTimeInZPos)
                return measure.measureNum;
            else
                continue;
        }
        */
        for (int i = 0; i < songLengthInMeasures; i++)
            if (Math.Round(zPos, 1) < (i + 1) * Math.Round(measureLengthInzPos, 1)) // TODO: might not need rounding?
                return i;

        return -1;
    }
    public int GetSubbeatNumForZPos(int measureNum, float zPos)
    {
        /*
        zPos = (float)Math.Round(zPos, 1);

        MeasureInfo info = songMeasures[measureNum];
        float measureTime = (float)Math.Round((info.startTimeInzPos + subbeatLengthInzPos), 1);

        int finalValue = -1;

        for (int i = 0; i < 8; i++)
        {
            if (zPos < measureTime)
                return i;
            else
                measureTime += subbeatLengthInzPos;
        }

        Debug.LogErrorFormat("AMP_CTRL: couldn't find subbeat for note at measure {0}, zPos {1}", measureNum, zPos);
        return finalValue;
        */
        float position = (float)Math.Round((measureNum + 1) * measureLengthInzPos);

        for (int i = 0; i < 8; i++)
        {
            if (Math.Round(zPos, 1) < position) // TODO: might not need rounding?
                return i;
            else
                position += subbeatLengthInzPos;
        }

        return -1;
    }
    #endregion

    // Starting position adjustment to always reach end of the path
    public bool StartPositionAdjustmentEnabled = true;
    public float StartPosition; // pos

    public void CalculateTimeUnits()
    {
        // Seconds
        secInTick = 60f / (songBpm * beatTicks);
        secInMs = 0.001f; // 1s = 1000ms
        secInPos = (secPerBeat / 4); // * songFudgeFactor / 1.8f

        // Milliseconds
        msInTick = secInTick * 1000;
        msInSec = 1000; // 1ms = 0.001s
        msInPos = secInPos * 1000;

        // Position (meters)
        posInSec = (4 / secPerBeat) / songFudgeFactor * 1.8f;
        posInMs = (4 / secPerBeat / 1000) / songFudgeFactor * 1.8f;
        posInTick = posInSec * secInTick;

        // Ticks
        tickInSec = (songBpm * beatTicks) / 60; // how many ticks in a second // 880
        tickInMs = tickInSec / 1000; // How many ticks in a milliseconds // ???
        tickInPos = secInPos * tickInSec;

        // Find starting position point
        // This adjusts the positioning of stuff in a way that we always reach the very end of the path
        if (StartPositionAdjustmentEnabled)
            StartPosition = PathTools.Path.length - (songLengthInMeasures * measureLengthInzPos);
    }
    public bool StartDistanceAdjustmentEnabled = true;
    public float StartDistance; // pos

    // INIT & LOADING

    public virtual void Awake() => Instance = this;
    public virtual void Start()
    {
        // TODO: The loading sequence or RhythmicGame itself should manage the loading scene!
        // It's possible that GameStarter might not even be needed, or we'll have a different loading mechanism once we'll have UI.
        // For now, we use the SongController to manage the scenes during and after loading.
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("DevScene"));

        // Create clock
        CreateClock();

        if (!Enabled)
        { Debug.LogWarningFormat("SongCtrl: Disabled"); return; }
        if (IsFake) // TODO: fake song information implementation!
        { Debug.LogWarningFormat("SongCtrl: running in fake mode!"); return; }

        // TODO: Loading the song should not be based on what property is set!
        // LoadSong() should be called by the UI or loading mechanism!
        LoadSong(songName == null ? defaultSong : songName); // load default song in case the prop is empty, for testing purposes only!

        // Set / calculate slop ms and pos props
        //SlopMs = RhythmicGame.SlopMs / songFudgeFactor * (1f + 0.8f); // TODO: incorrect?
        SlopMs = PosToMs(MsToPos(RhythmicGame.SlopMs), true);
        //SlopMs = PosToSec(SecToPos(RhythmicGame.SlopMs / 1000f)) * 1000f;
        SlopPos = MsToPos(SlopMs);

        Debug.Log($"SlopMs: {SlopMs} | SlopPos: {SlopPos}");
    }

    // Track streamer
    public TrackStreamer trackStreamer;

    public void CreateTrackStreamer()
    {
        if (!trackStreamer & AmpTrackController)
        {
            trackStreamer = AmpTrackController.gameObject.AddComponent<TrackStreamer>();
            Debug.LogFormat("TRACKS: Created track streamer!");
        }
        else
            Debug.LogWarning("AMP_CTRL: TrackStreamer already exists!");
    }

    public List<Dictionary<int, MetaMeasure>> metaMeasures;
    public List<Dictionary<int, MetaMeasure>> CreateMetaMeasureList()
    {
        List<Dictionary<int, MetaMeasure>> list = new List<Dictionary<int, MetaMeasure>>();

        // Build metalist
        // TODO: move to SongController (make it like songNotes)
        for (int x = 0; x < RhythmicGame.TunnelTrackDuplicationNum; x++)
        {
            foreach (string track in songTracks)
            {
                var inst = AmpTrack.InstrumentFromString(track);
                if (inst == AmpTrack.InstrumentType.FREESTYLE & !RhythmicGame.PlayableFreestyleTracks)
                    continue;
                else if (inst == AmpTrack.InstrumentType.bg_click)
                    continue;

                // create dictionary and metameasures
                Dictionary<int, MetaMeasure> dict = new Dictionary<int, MetaMeasure>();
                for (int i = 0; i < songLengthInMeasures + 1; i++)
                {
                    MetaMeasure metameasure = new MetaMeasure() { ID = i, Instrument = inst, StartDistance = MeasureToPos(i) }; // MEASUREPOS
                    dict.Add(i, metameasure);
                }
                list.Add(dict);
            }
        }

        return list;
    }
    public virtual List<List<KeyValuePair<int, MetaNote>>> CreateNoteList() { return new List<List<KeyValuePair<int, MetaNote>>>(); }

    public virtual void CreateAmpTrackController()
    {
        GameObject ctrlGameObject = new GameObject() { name = "AMP_TRACKS" };
        AmpTrackController = ctrlGameObject.AddComponent<TracksController>();

        //AmpTrackController.OnTrackSwitched += TracksController_OnTrackSwitched;
        Debug.LogFormat("TRACKS: Created track controller!");

        // Track streamer init
        CreateTrackStreamer();
    }

    void CreateClock()
    {
        Clock = gameObject.AddComponent<Clock>();
        Clock.OnBeat += Clock_OnBeat;
    }

    // Vibrate on every clock beat!
    private void Clock_OnBeat(object sender, int e) => BeatVibration();

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

    // TRACK VOLUMES & MIXER
    /// Handling the AudioSource volumes for each track
    // TODO: MIXER!!!

    // When the track changes, change music track volume | e[0]: old ID; e[1]: new ID
    void TracksController_OnTrackSwitched(object sender, int[] e)
    {
#if false
        int trackID = TrackController.Tracks[e[1]].ID;
        if (audioSrcList[trackID].clip == null)
        { Debug.LogWarningFormat("SONGCTRL: Track ID {0} does not have an audio clip! - ignoring track switch volume change", e); return; }

        // Set volumes
        for (int i = 0; i < songTracks.Count; i++)
        {
            AudioSource src = audioSrcList[i];

            if (i == trackID && TrackController.Tracks[i].IsTrackCaptured) // Current track should go full volume if it's captured | TODO: revise?
                src.volume = 1f;
            else if (TrackController.Tracks[i].IsTrackCaptured) // Other tracks that are captured should be quieter
                src.volume = 0.4f;
            else // Other tracks that are NOT captured should be silent
                src.volume = 0f;
        }
#endif
    }

    // Adjust the volume of a speciifc track
    public void AdjustTrackVolume(int track, float volume)
    {
        if (track == -1) return;
        else if (track >= audioSrcList.Count)
        { Debug.LogWarningFormat("SONGCTRL: Track ID {0} does not have an audio clip! - ignoring track switch volume change", track); return; }

        audioSrcList[track].volume = volume;
    }

    // SONG CONTROL
    /// Functions that relate to song playback ///

    // Called once - starts playing the music
    public void Play()
    {
        IsPlaying = true;

        songDspTime = (float)AudioSettings.dspTime;

        // Start playing each AudioSource at the same time
        // TODO: FMOD implementation should give us way more control.
        audioSrcList.ForEach(src => src.PlayScheduled(songDspTime));
    }
    // Pauses or resumes the music
    public void TogglePause()
    {
        IsPlaying = !IsPlaying;

        //RhythmicGame.SetTimescale(Time.timeScale != 0 ? 0f : 1f);

        // Pause / unpause the AudioSources
        audioSrcList.ForEach(src =>
        {
            if (!IsPlaying) src.Pause();
            else src.UnPause();
        });
    }
    public void PlayPause()
    {
        if (songPosition == 0f)
            Play();
        else
            TogglePause();
    }
    // Changes the speed of the music
    public void SetSongSpeed(float speed)
    {
        songSpeed = speed;

        // Changing the pitch of AudioSources also changes their speed
        audioSrcList.ForEach(src => src.pitch = speed);
    }
    // Seeks forward or backward in the music, by seconds
    public void OffsetSong(float offset)
    {
        audioSrcList.ForEach(src => src.time += offset); // offset music by seconds!
                                                         //Player.OffsetPlayer(offset * (Player.PlayerSpeed * secPerBeat / songFudgeFactor * (1f + 0.8f))); // offset player by zPos!
    }

    // GAMEPLAY
    /// Functions that relate to gameplay ///

    // Vibrate controller to the beat
    public void BeatVibration() => VibrationController.VibrateLinear(beatHaptics);
}