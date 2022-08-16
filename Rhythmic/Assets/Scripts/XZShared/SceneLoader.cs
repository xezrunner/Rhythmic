using System.Collections;
using UnityEngine.SceneManagement;

using static Logging;

public static class SceneLoader {
    public static IEnumerator COROUTINE_LoadScene(string scene_name, bool as_active = false) {
        var operation = SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);
        while (!operation.isDone) yield return null;
        if (as_active) {
            (Scene scene, bool success) result = find_scene_by_name(scene_name);
            if (result.success) SceneManager.SetActiveScene(result.scene);
        }
    }
    
    public static (Scene scene, bool success) find_scene_by_name(string scene_name) {
        for (int i = 0; i < SceneManager.sceneCount; ++i) {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.name == scene_name) return (s, true);
        }
        log_error("could not find scene '%'".interp(scene_name));
        return (default, false);
    }

    public static IEnumerator COROUTINE_UnloadScene(string scene_name) {
        // TODO: checks!
        var operation = SceneManager.UnloadSceneAsync(scene_name);
        while (!operation.isDone) yield return null;
    }

    public static bool set_scene_as_active(string scene_name) {
        return SceneManager.SetActiveScene(find_scene_by_name(scene_name).scene);
    }
}