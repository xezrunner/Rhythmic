using UnityEngine;

using static Logging;

public class RHXCore : MonoBehaviour {
    void Start() {
        log("RHX Startup");

        song_info info = SongLoader.load_song(default_song);
    }

    [Header("Default song to load into during development:")]
    public string default_song = "allthetime";
}