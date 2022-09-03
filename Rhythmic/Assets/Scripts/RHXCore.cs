using UnityEngine;
using static Logging;

public enum GameDifficulty { Easy = 0, Intermediate = 1, Advanced = 2, Expert = 3, Super = 4 }

public class RHXCore : MonoBehaviour {
    static RHXCore instance;
    public static RHXCore get_instance() {
        if (!instance) log_error("no RHXCore instance!");
        return instance;
    }

    public const string CORE_SCENE_NAME = "_RHXCoreScene";
    public static bool IS_INTERNAL = true;

    void Awake() {
        instance = this;
    }

    SongController song_controller;

    [Header("Default song to load into during development:")]
    public string default_song = "allthetime";
    public string requested_song;

    void Start() {
        log("RHX Startup");

        // Set RHX Core scene as default:
        SceneLoader.set_scene_as_active(CORE_SCENE_NAME);

        if (IS_INTERNAL) requested_song = default_song;

        // Create SongController:
        GameObject obj = new GameObject("SongController");
        song_controller = obj.AddComponent<SongController>();
        song_controller.rhx_core = this;
    }
}