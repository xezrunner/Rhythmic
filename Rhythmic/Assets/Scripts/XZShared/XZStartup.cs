using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class XZStartup : MonoBehaviour {
    public const  string STARTUP_SCENE_NAME = "Startup";
    public static string CORE_SCENE_NAME = "_XZCoreScene";
    
    void Awake() {
        // Ensure that the XZShared core scene is loaded:
        load_core_scene(CORE_SCENE_NAME);
        // Load in all game-specific core scenes:
        foreach (string s in game_specific_core_scenes) load_core_scene(s);
        
        StartCoroutine(COROUTINE_TransitionToTarget());
    }

    [Header("The scene to load to from Startup:")]
    [SerializeField] public string       target_scene;
    [SerializeField] public List<string> game_specific_core_scenes;

    IEnumerator COROUTINE_TransitionToTarget() {
        var target_load = SceneManager.LoadSceneAsync(target_scene, LoadSceneMode.Additive);
        while (!target_load.isDone) yield return null;
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
