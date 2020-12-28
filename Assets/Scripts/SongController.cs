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
    public float secPerBeat { get { return songBpm / 60f; } }
    public float beatPerSec { get { return 60f / songBpm; } }

    public int beatTicks = 480;
    public int measureTicks { get { return beatTicks * 4; } }
    public int subbeatTicks { get { return beatTicks / 2; } }

    public float measureLengthInzPos { get { return TickTimeTozPos(measureTicks); } }
    public float subbeatLengthInzPos { get { return TickTimeTozPos(subbeatTicks); } }

    // TDOO: Re-do to local values!!!
    // TODO: REVISE!!!

    // ms (Milliseconds)
    public float tickInMs { get { return (60000f / (songBpm * beatTicks)); } }
    public float msIntick { get { return (songBpm * beatTicks) / 60000f; } }
    public float msInzPos { get { return (secPerBeat / 1000) / songFudgeFactor * (1f + 0.8f); } }
    // s (Seconds)
    public float tickInSec { get { return (60f / (songBpm * beatTicks)); } }
    public float secInTick { get { return (songBpm * beatTicks) / 60f; } }
    public float secInzPos { get { return secPerBeat / songFudgeFactor * (1f + 0.8f); } }
    // zPos (Meters)
    // TODO: these zPos conversions do not work!!!
    public float tickInzPos { get { return tickInSec / (tickInSec * beatTicks) * 4 / songFudgeFactor * (1f + 0.8f); } }
    public float zPosInTick { get { return (tickInzPos / 4f * (tickInSec * beatTicks)) * (songBpm * beatTicks); } }
    public float zPosInSec { get { return 60f / songBpm; } }
    public float zPosInMs { get { return 60000f / songBpm; } }
    // Converters
    public float TickTimeToMs(float tick = 1f) { return tickInMs * tick; }
    public float MsToTickTIme(float ms = 1f) { return msIntick * ms; }

    public float TickTimeToSec(float tick = 1f) { return tickInSec * tick; }
    public float SecToTickTime(float sec = 1f) { return secInTick * sec; }
    public float SecTozPos(float sec = 1f) { return sec * secInzPos; }

    public float TickTimeTozPos(float tick = 1f) { return tickInzPos * tick; }
    public float zPosToTickTime(float zPos = 1f) { return zPosInTick * zPos; }
    public float zPosToSec(float zPos = 1f) { return zPosInSec * zPos; }
    public float zPosToMs(float zPos = 1f) { return zPosInMs * zPos; }

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

        SlopMs = zPosToSec(SecTozPos(RhythmicGame.SlopMs / 1000)) * 1000f / songFudgeFactor * (1 + 0.8f);
        SlopPos = SecTozPos(SlopMs / 1000f);
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
                    MetaMeasure metameasure = new MetaMeasure() { ID = i, Instrument = inst };
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