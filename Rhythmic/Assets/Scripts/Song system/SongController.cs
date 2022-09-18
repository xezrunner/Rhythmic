using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using static Logging;

public class SongController : MonoBehaviour {
    static SongController instance;
    public static SongController get_instance() {
        if (!instance) log_error("no SongController instance!");
        return instance;
    }
    void Awake() {
        instance = this;
    }

    public RHXCore rhx_core;

    void Start() {
        log("initializing SongController...");

        if (!rhx_core) {
            log_warn("no rhx_core was passed at creation! trying to get it...");
            rhx_core = RHXCore.get_instance();
        }
        if (!rhx_core) log_error("failed to get an RHXCore instance!");

        if (!rhx_core.requested_song.is_empty()) {
            log("song requested at init: %".interp(rhx_core.requested_song));
            load_song(rhx_core.requested_song);
        } else log("no song requested - waiting for a song in rhx_core...");
    }

    public bool waiting_for_song = true;
    public song_info current_song_info;

    [ConsoleCommand]
    public static void cmd_load_song(string[] args) => get_instance()?.load_song(args[0]);

    public void load_song(string song_name) {
        waiting_for_song = false;
        (bool success, song_info info) result = SongLoader.load_song(song_name);
        if (!result.success) {
            log_error("Failed to load song '%'.".interp(song_name));
            return;
        }
        current_song_info = result.info;

        load_audio();
    }

    GameObject audio_container;
    Dictionary<string, AudioSource> audio_sources = new();

    void load_audio() {
        if (!audio_container) {
            audio_container = new GameObject();
            audio_container.transform.SetParent(transform);
        }
        audio_container.name = "Audio container (%)".interp(current_song_info.name);

        foreach (var it in audio_sources.Values) Destroy(it);
        audio_sources.Clear();

        foreach (var info in current_song_info.tracks) {
            // I hate using coroutines, but it is necessary here:
            StartCoroutine(UNITY_LoadTrackAudioSourceAndClip(info, AudioType.OGGVORBIS));
        }
    }

    IEnumerator UNITY_LoadTrackAudioSourceAndClip(song_track info, AudioType audio_type, bool stream_audio = true) {
        if (!info.audio_exists) {
            log_warn("skipping non-existent audio for track '%' at path '%')".interp(info.name, info.audio_path));
            yield break;
        }

        AudioSource source = audio_container.AddComponent<AudioSource>();
        audio_sources.Add(info.name, source);

        AudioClip clip = null;

        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(info.audio_path, AudioType.OGGVORBIS)) {
            ((DownloadHandlerAudioClip)request.downloadHandler).streamAudio = true;
            yield return request.SendWebRequest();

            // TODO: timeout!

            if (request.result != UnityWebRequest.Result.Success) {
                log_error("% for: '%'".interp(request.result, info.name));
            } else 
                clip = DownloadHandlerAudioClip.GetContent(request);
        }

        if (clip == null) {
            log_error("clip is null for: '%'", info.name);
            audio_sources.Remove(info.name);
            yield break;
        }

        clip.name = "%".interp(info.name);
        source.clip = clip;

    }

    [ConsoleCommand]
    static void temp_play_audio_sources() {
        var inst = get_instance(); if (!inst) return;

        // NOTE: When we'll be playing back the audio sources later, we should use a smaller delay (such as 0.1).
        // Prior versions didn't use a delay and could have theoretically led to desync during inital playback.
        // TODO: Test desync / delay!
        double audio_time = AudioSettings.dspTime + 0.5;

        foreach (var source in inst.audio_sources.Values) {
            log("starting playback with 0.5 delay (audio_time: %) for: '%'".interp(audio_time, source.clip.name));
            source.PlayScheduled(audio_time);
        }
    }

    [ConsoleCommand]
    static void temp_stop_audio_sources() {
        var inst = get_instance(); if (!inst) return;

        foreach (var source in inst.audio_sources.Values) {
            source.Stop();
        }
    }

    [ConsoleCommand]
    public static void print_song_info() {
        if (!get_instance()) return;
        if (instance.waiting_for_song) {
            log_warn("no song!");
            return;
        }

        log_dump_obj(instance.current_song_info);

        logging_options.indentation_level = 1;
        foreach (var track in instance.current_song_info.tracks)
            log_dump_obj(track);
        logging_options.indentation_level = 0;

        log("audio sources: %".interp(instance.audio_sources.Count));
    }

    void Update() {
        if (waiting_for_song && !rhx_core.requested_song.is_empty()) load_song(rhx_core.requested_song);
    }
}
