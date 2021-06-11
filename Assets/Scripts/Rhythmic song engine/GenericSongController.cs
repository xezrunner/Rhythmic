using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

/*
This is a base for a song controller class.
*/

/* TODO:
- Sync game to audio time (song_delta_time)
- Start audio at the same time (fix)
*/

public partial class GenericSongController : MonoBehaviour
{
    public static GenericSongController Instance;

    // TODO: This is for testing purposes only!
    public static string default_song = "allthetime";

    [Header("Controller props")]
    public bool is_enabled = true;
    public bool is_fake;
    public bool is_song_over;

    /// TODO: Do we really need all these props? They're in the SongMetaFile anyway.
    [Header("Song props")]
    public SongInfo song_info; // TODO: Rename to song_info!!!
    public SongTimeUnit time_units;

    // TODO: Start distance implementation!
    public float start_distance;
    public float pos_in_sec;
    public float bar_length_pos;
    public float slop_pos;

    bool audio_clips_loaded = false;
    public List<AudioClip> audio_clips;

    public GameObject audio_sources_container;
    public List<AudioSource> audio_sources;
    
    public void Awake()
    {
        Instance = this;
        CreateRequiredObjects();
    }
    public void Start()
    {
        if (!is_enabled) { Logger.LogW("Song controller disabled."); return; }
        if (is_fake) Logger.LogW("Song controller is in fake mode.");
        
        LoadSong(default_song, GameLogic.AMPLITUDE); // NOTE: temp
        
        DebugConsole.RegisterCommand("set_song_timescale", (string[] args) => SetSongTimescale(args[0].ParseFloat()));
        DebugConsole.RegisterCommand("set_song_timescale_smooth", (string[] args) => { Logger.Log("0: % 1: %", args[0], args.Length > 1 ? args[1] : "-"); SetSongTimescale_Smooth(args[0].ParseFloat(), args.Length > 1 ? args[1].ParseFloat() : 0.3f); });
    }
    void CreateRequiredObjects()
    {
        // TODO: We should probably use Tags instead!
        // TODO: InstantiatePrefab() function!
        if (!GameObject.Find("DebugController"))
        {
            GameObject debug_ctrl_prefab = (GameObject)Resources.Load("Prefabs/Debug/DebugController");
            GameObject obj = Instantiate(debug_ctrl_prefab);
            // TODO: Debug console loads quite late - we miss some messages before/at its initialization process. | Redirect Unity logger / grab stuff from there?
            Logger.LogConsoleW("DebugController created from SongController.");
        }
        if (!GameObject.Find("Player"))
        {
            GameObject player_prefab = (GameObject)Resources.Load("Prefabs/Player");
            GameObject obj = Instantiate(player_prefab);
            Logger.LogConsoleW("Player created from SongController.");
        }
    }

    public void LoadSong(string song_name, GameLogic mode = GameLogic.RHYTHMIC)
    {
        // Validate song existence && load song data:
        // TODO: naming - SongLoader.LoadSongMetaFile - we don't just load the meta file...
        SongInfo song_info = SongLoader.LoadSongMetaFile(song_name, mode);
        if (song_info == null)
        {
            Logger.LogError("Failed to load song '%' - meta info was null", song_name);
            // TODO: Throw player back to menu (if meta) / handle failure
            return;
        }
        this.song_info = song_info;
        time_units = song_info.time_units;
        // Setup core time/pos units:
        bar_length_pos = time_units.BarToPos(1);
        pos_in_sec = time_units.pos_in_sec;
        slop_pos = time_units.MsToPos(RhythmicGame.SlopMs);

        audio_clips = LoadSongClips(song_info, mode);

        CreateClock();
        CreateTracksController(song_info);
        CreateTrackStreamer();
    }

    List<AudioClip> LoadSongClips(SongInfo info, GameLogic mode = GameLogic.RHYTHMIC)
    {
        List<AudioClip> clips = null;

        // TODO: This looks nasty:
        if (mode == GameLogic.AMPLITUDE) StartCoroutine(AMPLITUDE_LoadAudioClips(info));
        else if (mode == GameLogic.RHYTHMIC) Logger.LogE("Not yet implemented for RHYTHMIC!".M());

        StartCoroutine(CreateAudioSourcesFromClips());

        return clips;
    }

    const float _audio_src_creation_timeout_ms = 10000;
    float _audio_src_creation_timeout_elapsed_ms;
    IEnumerator CreateAudioSourcesFromClips()
    {
        while (!audio_clips_loaded)
        {
            _audio_src_creation_timeout_elapsed_ms += Time.deltaTime * 1000f;
            if (_audio_src_creation_timeout_elapsed_ms >= _audio_src_creation_timeout_ms)
            { Logger.LogE("Timeout - audio_clips did not get a value.".M()); yield break; }
            yield return null;
        }
        _audio_src_creation_timeout_elapsed_ms = 0f; // Might not need to reset this? Perhaps in ResetController() only?

        if (audio_sources_container) Destroy(audio_sources_container);
        else
        {
            audio_sources_container = new GameObject("Audio sources");
            audio_sources_container.transform.parent = transform;
        }

        if (audio_clips.Count == 0)
        { Logger.LogE("audio_clips count was 0!".M()); yield break; }

        audio_sources = new List<AudioSource>();

        for (int i = 0; i < audio_clips.Count; ++i)
        {
            AudioClip clip = audio_clips[i];
            if (clip == null)
                Logger.LogW("Warning: clip index % name '%' is null (length: %)", i.ToString(), clip.name, clip?.length.ToString()); // TODO: Logger: params with object instead!

            // TODO: We might want to create a child object for containing the audio sources, for hierarchy.
            AudioSource src = audio_sources_container.AddComponent<AudioSource>();
            src.clip = clip;
            audio_sources.Add(src);
        }

        song_info.song_length_sec = audio_clips.Last().length;
        Logger.Log("Added % audio sources.".AddColor(Colors.Network).M(), audio_sources.Count);
    }

    // TODO: Implement!
    void ResetController() { }

    public TracksController TracksController;
    public TrackStreamer TrackStreamer;

    void CreateTracksController(SongInfo song_info)
    {
        if (TracksController)
        {
            Logger.LogWarning("TracksController already exists!".T(this));
            return;
        }

        GameObject obj = new GameObject("Track controller");
        TracksController = obj.AddComponent<TracksController>();
        TracksController.Init(song_info);
    }

    // TrackStreamer is added onto the TracksController as a component.
    void CreateTrackStreamer()
    {
        if (!TracksController)
        {
            Logger.LogWarning("TracksController doesn't exist!".TM(this));
            return;
        }

        TrackStreamer = TracksController.gameObject.AddComponent<TrackStreamer>();
    }

    void CreateClock()
    {
        Clock clock = gameObject.AddComponent<Clock>();
        // TODO: Clock OnBeat events for vibration ->
        // Should also be controllable within the song! Might even get a new place?
    }

    // Playback control ... //


    public bool is_playing;
    public float song_position;

    public float song_delta_time;
    public float song_time_scale = 1f;

    void Update()
    {
        if (Keyboard.current.xKey.isPressed) OffsetSong(5);
        if (!is_playing) return;

        song_delta_time = audio_sources[0].time - song_position;
        song_position = audio_sources[0].time; // TODO: this is bad!!!
    }

    public void Play()
    {
        if (!is_enabled)
        { Logger.LogW("Controller disabled!".M()); return; }
        if (audio_sources == null || audio_sources.Count == 0)
        { Logger.LogE("No audio sources!".TM(this)); return; }

        is_playing = true;
        double audio_dsp_time = AudioSettings.dspTime + 0.1d;

        foreach (AudioSource src in audio_sources) src.PlayScheduled(audio_dsp_time);
    }
    public void Pause() { foreach (AudioSource src in audio_sources) src.Pause(); is_playing = false; }
    public void Unpause() { foreach (AudioSource src in audio_sources) src.UnPause(); is_playing = true; }
    public void TogglePause() { if (is_playing) Pause(); else Unpause(); }

    /// <summary> Plays if not yet playing. Otherwise, toggles pause. </summary>
    public void PlayPause()
    {
        // Ignore Alt + Enter to avoid conflict with fullscreen shortcut:
        if (Keyboard.current != null && Keyboard.current.altKey.isPressed) return;

        if (song_position == 0f) Play();
        else TogglePause();
    }

    public void SetPlayingState(bool state)
    {
        if (state && song_position == 0f) Play();
        else if (state) Unpause();
        else Pause();
    }

    /// <summary>Adds to the song position. Use negative values to travel backwards.</summary>
    // NOTE: Backwards travel is unstable/untested/not implemented!
    public void OffsetSong(float offset)
    {
        if (is_song_over) return;

        bool prev_is_playing = is_playing;
        Pause();

        foreach (AudioSource src in audio_sources)
        {
            if (src.time + offset < 0) Logger.LogW("Negative song limit reached!".M());
            else if (src.time + offset >= src.clip.length) Logger.LogW("Song limit reached!".M());
            else src.time += offset;
        }

        if (prev_is_playing) Unpause();
    }

    public void SetSongTimescale(float value)
    {
        int count = audio_sources.Count;
        for (int i = 0; i < count; ++i)
        {
            AudioSource src = audio_sources[i];
            src.pitch = value;
        }

        Time.timeScale = value;
        song_time_scale = value;
    }

    public void SetSongTimescale_Smooth(float value, float smooth_time = 0.3f) => StartCoroutine(_SetSongTimescale_Smooth(value, smooth_time));
    public bool is_setting_smooth_timescale = false;
    IEnumerator _SetSongTimescale_Smooth(float value, float time = 0.3f)
    {
        is_setting_smooth_timescale = false;
        yield return null; // Cancel any other of possible running coroutines
        is_setting_smooth_timescale = true;

        float t = 0f;
        float elapsed_time = 0f;
        float scale_temp = song_time_scale;

        // Using a timeout of 5 seconds
        while (is_setting_smooth_timescale)
        {
            song_time_scale = Mathf.Lerp(scale_temp, value, t);

            int count = audio_sources.Count;
            for (int i = 0; i < count; ++i)
            {
                AudioSource src = audio_sources[i];
                src.pitch = song_time_scale;
            }

            elapsed_time += Time.unscaledDeltaTime;
            t = elapsed_time / time;

            if (!is_setting_smooth_timescale) yield break;
            else if (t == 1f) break;
            else yield return null;
        }

        is_setting_smooth_timescale = false;
        SetSongTimescale(value);
        yield break;
    }

    static bool prev_playing_state_before_focus_loss;
    void OnApplicationFocus(bool has_focus)
    {
        Logger.LogConsoleW("Application focus state: %", has_focus);
        if (!has_focus) prev_playing_state_before_focus_loss = is_playing;
        if (prev_playing_state_before_focus_loss) SetPlayingState(has_focus);
    }
}