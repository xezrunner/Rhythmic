using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public partial class SongController : MonoBehaviour
{
    public static SongController Instance;

    [NonSerialized] public Clock Clock;
    [NonSerialized] public TracksController TracksController;

    [Header("Controller properties")]
    public string defaultSong;

    public static string songName;
    public virtual string songFolder { get; set; }

    public bool IsEnabled = true;
    public bool IsFake = false;
    public bool IsPlaying;
    public bool IsSongOver;

    // Song properties
    [NonSerialized] public int songBpm;
    [NonSerialized] public int songCountIn;
    [NonSerialized] public float songLength;
    [NonSerialized] public float songLengthInzPos;
    [NonSerialized] public int songLengthInMeasures;

    // Playback properties
    [Header("Playback properties")]
    public float songTimeScale = 1f; // Playback speed.
    public float songOffset;
    public float songPosition { get { return BG_CLICKSrc.time; } } // Current song position in seconds | this is the BG_CLICK AudioSource's current time pos.
    public float songDspTime; // How many seconds have passed since the song started

    [Header("Slop properties")]
    public float SlopMs;
    public float SlopPos;

    [Header("Haptic feedback")]
    public Vector3 beatHaptics;

    // Starting position adjustment to always reach end of the path
    [Header("Start position adjustment")]
    public bool StartDistanceAdjustmentEnabled = true;
    public float StartDistance; // pos
    
    public AudioSource BG_CLICKSrc; // The BG_CLICK AudioSource
    public List<AudioSource> audioSrcList = new List<AudioSource>();

    public List<string> songTracks = new List<string>();
    public List<List<KeyValuePair<int, MetaNote>>> songNotes = new List<List<KeyValuePair<int, MetaNote>>>();

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

        if (!IsEnabled)
        { Debug.LogWarningFormat("SongCtrl: Disabled"); return; }
        if (IsFake) // TODO: fake song information implementation!
        { Debug.LogWarningFormat("SongCtrl: running in fake mode!"); return; }

        // TODO: Loading the song should not be based on what property is set!
        // LoadSong() should be called by the UI or loading mechanism!
        LoadSong(songName == null ? defaultSong : songName); // load default song in case the prop is empty, for testing purposes only!

        // Set / calculate slop ms and pos props
        //SlopMs = RhythmicGame.SlopMs / songFudgeFactor * (1f + 0.8f); // TODO: incorrect?
        //SlopMs = PosToSec(SecToPos(RhythmicGame.SlopMs / 1000f)) * 1000f; // incorrect?

        //SlopMs = PosToMs(MsToPos(RhythmicGame.SlopMs), true);
        SlopMs = RhythmicGame.SlopMs;
        SlopPos = MsToPos(SlopMs);

        Debug.Log($"SlopMs: {SlopMs} | SlopPos: {SlopPos}");
    }

    // Track streamer
    [NonSerialized] public TrackStreamer trackStreamer;

    public void CreateTrackStreamer()
    {
        if (!trackStreamer & TracksController)
        {
            trackStreamer = TracksController.gameObject.AddComponent<TrackStreamer>();
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
        TracksController = ctrlGameObject.AddComponent<TracksController>();

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
        if (!IsEnabled) { Logger.LogMethod("Can't play - the controller is disabled.", this); return; }
        if (Keyboard.current.altKey.isPressed) return; // Alt + Enter is for fullscreen! Ignore!

        if (songPosition == 0f)
            Play();
        else
            TogglePause();
    }
    // Changes the speed of the music
    public void SetSongSpeed(float speed)
    {
        songTimeScale = speed;

        // Changing the pitch of AudioSources also changes their speed
        audioSrcList.ForEach(src => src.pitch = speed);
    }
    // Seeks forward or backward in the music, by seconds
    public void OffsetSong(float offset)
    {
        if (IsSongOver) return;
        audioSrcList.ForEach(src => src.time += offset); // offset music by seconds!
                                                         //Player.OffsetPlayer(offset * (Player.PlayerSpeed * secPerBeat / songFudgeFactor * (1f + 0.8f))); // offset player by zPos!
    }

    // GAMEPLAY
    /// Functions that relate to gameplay ///

    // Vibrate controller to the beat
    public void BeatVibration() => VibrationController.VibrateLinear(beatHaptics);
}