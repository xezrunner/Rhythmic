using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using static Logging;

public class Test : MonoBehaviour {
    void Awake() {
        maybe_load_core_scene();
    }
    
    void Start() {
    }

    void maybe_load_core_scene() {
        for (int i = 0; i < SceneManager.sceneCount; ++i) {
            // Don't load the core scene if we have it in the editor already:
            if (SceneManager.GetSceneAt(i).name == "_CoreScene") return;
        }
        SceneManager.LoadSceneAsync("_CoreScene", LoadSceneMode.Additive);
    }

    [ConsoleCommand("A test variable")]
    public static int var_a = 15;

    [ConsoleCommand("Logs a test message")]
    static void test_log()  => log("test!");
    [ConsoleCommand("Logs a test warning message")]
    static void test_log_warn() => log_warn("test!");
    [ConsoleCommand("Logs a test error message")]
    static void test_log_error() => log_error("test!".color(Color.red));
}