using UnityEngine;
using UnityEngine.SceneManagement;

public class XZ_Startup : MonoBehaviour {
    public static string CORE_SCENE_NAME = "_XZCoreScene";
    
    void Awake() {
        load_core_scene();
    }

    void load_core_scene() {
        bool success = false;
        for (int i = 0; i < SceneManager.sceneCount; ++i) {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.name == CORE_SCENE_NAME) {
                success = true;
                break;
            }
        }
        if (!success) SceneManager.LoadSceneAsync(CORE_SCENE_NAME, LoadSceneMode.Additive);
    }
}
