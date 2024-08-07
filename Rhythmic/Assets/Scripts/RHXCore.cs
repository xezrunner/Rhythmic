using UnityEngine;
using static Logging;

// TODO: This shouldn't be here! Probably belongs in SongController to be honest, perhaps Player, or some sort of [gameplay "Session"].
public enum GameDifficulty { Easy = 0, Intermediate = 1, Advanced = 2, Expert = 3, Super = 4 }

public class RHXCore : MonoBehaviour {
    static RHXCore instance;
    public static RHXCore get_instance() {
        if (!instance) log_error("no RHXCore instance!");
        return instance;
    }

    public const  string CORE_SCENE_NAME = "_RHXCoreScene";
    public static bool   IS_INTERNAL = true;

    void Awake() {
        instance = this;
    }

    [Header("Default song to load into during development:")]
    public string default_song = "allthetime";
    public string requested_song;

    public SongController  song_controller;
    public TrackController track_controller;

    // TODO: players, sessions etc...

    void Start() {
        log("RHX Startup");

        log("Hello %!", args: "World");

        // Set RHX Core scene as active/default:
        SceneLoader.set_scene_as_active(CORE_SCENE_NAME);

        // TEMP: Immediately request a song during internal testing.
        if (IS_INTERNAL) requested_song = default_song;

        // Create SongController:
        {
            GameObject obj = new GameObject("SongController");
            song_controller = obj.AddComponent<SongController>();
        }

        // Create TrackController:
        {
            GameObject obj = new GameObject("TrackController");
            track_controller = obj.AddComponent<TrackController>();
        }
    }
}