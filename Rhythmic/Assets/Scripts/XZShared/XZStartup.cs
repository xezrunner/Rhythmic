using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// This is the very first thing that should run, from the Startup scene.
// We should load all the [ core systems (XZShared + game) ] first, once.
// After all that is finished, unload the entirety of the Startup scene.
//
// Game-specific core systems and the target scene should be configured within 
// the Startup scene -> Startup object -> Startup component.
// The Startup scene is meant to be game-specific - create your own and add the
// Startup object + component onto it.
//
// When configuring the project, make sure Startup is the first scene to load in Build settings!

public class XZStartup : MonoBehaviour {
    public const  string STARTUP_SCENE_NAME = "Startup";
    public static string CORE_SCENE_NAME = "_XZCoreScene";
    
    void Awake() {
        // Ensure that the XZShared core scene is loaded:
        load_core_scene(CORE_SCENE_NAME);
        // Load in all game-specific core scenes:
        foreach (string s in game_specific_core_scenes) load_core_scene(s);
        // Load and transition to the target scene:
        StartCoroutine(COROUTINE_TransitionToTargetAndUnload());
    }

    [Header("The scene to load to from Startup:")]
    [SerializeField] public string       target_scene;
    [SerializeField] public List<string> game_specific_core_scenes;

    // NOTE: No code can be executed from Startup once this finishes. Use as final cleanup.
    IEnumerator COROUTINE_TransitionToTargetAndUnload() {
        var target_load = SceneManager.LoadSceneAsync(target_scene, LoadSceneMode.Additive);
        while (!target_load.isDone) yield return null;

        // Unload Startup:
        var self_unload = SceneManager.UnloadSceneAsync(STARTUP_SCENE_NAME);
        while (!self_unload.isDone) yield return null;
    }

    void load_core_scene(string name) {
        bool found = false;
        for (int i = 0; i < SceneManager.sceneCount; ++i) {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.name == name) {
                found = true;
                break;
            }
        }
        if (!found) SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
        else Debug.LogWarning("core scene '%' already loaded.".interp(name));
    }
}
