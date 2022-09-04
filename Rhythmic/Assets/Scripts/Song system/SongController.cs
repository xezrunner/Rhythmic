using UnityEngine;
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

    public void load_song(string song_name) {
        waiting_for_song = false;
        current_song_info = SongLoader.load_song(rhx_core.default_song);
    }

    [ConsoleCommand]
    public static void print_song_info() {
        if (!get_instance()) return;
        if (instance.waiting_for_song) {
            log_warn("no song!");
            return;
        }

        log_dump_obj(instance.current_song_info);
    }

    void Update() {
        if (waiting_for_song && !rhx_core.requested_song.is_empty()) load_song(rhx_core.requested_song);
    }
}
