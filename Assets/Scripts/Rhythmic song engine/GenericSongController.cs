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
    
    [Header("Controller props")]
    public bool is_enabled = true;
    public bool is_fake;
    public bool is_song_over;
    
    /// TODO: Do we really need all these props? They're in the SongMetaFile anyway.
    [Header("Song props")]
    public SongInfo song_info; // TODO: Rename to song_info!!!
    
    // TODO: Start distance implementation!
    public float start_distance;
    public float pos_in_sec;
    public float bar_length_pos;
    public float slop_pos;
    
    bool audio_clips_loaded = false;
    public List<AudioClip> audio_clips;
    
    public GameObject audio_sources_container;
    public List<AudioSource> audio_sources;
    
    public virtual void Awake() => Instance = this;
    public virtual void Start()
    {
        if (!is_enabled) { Logger.LogW("Song controller disabled."); return; }
        if (is_fake) Logger.LogW("Song controller is in fake mode.");
        
        LoadSong("allthetime", GameLogic.AMPLITUDE); // NOTE: temp
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
        // Setup core time/pos units:
        bar_length_pos = song_info.time_units.BarToPos(1);
        pos_in_sec = song_info.time_units.pos_in_sec;
        slop_pos = song_info.time_units.MsToPos(RhythmicGame.SlopMs);
        
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
    
    public void Play()
    {
        if (!is_enabled)
        { Logger.LogW("Controller disabled!".M()); return; }
        if (audio_sources == null || audio_sources.Count == 0)
        { Logger.LogE("No audio sources!".TM(this)); return; }
        
        is_playing = true;
        double audio_dsp_time = AudioSettings.dspTime;
        
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
    
    static bool prev_playing_state_before_focus_loss;
    void OnApplicationFocus(bool has_focus)
    {
        Logger.LogConsoleW("Application focus state: %", has_focus);
        if (!has_focus) prev_playing_state_before_focus_loss = is_playing;
        if (prev_playing_state_before_focus_loss) SetPlayingState(has_focus);
    }
}