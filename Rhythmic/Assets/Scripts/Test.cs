using UnityEngine;
using UnityEngine.SceneManagement;
using static Logging;

public class Test : MonoBehaviour {
    void Awake() {
        SceneManager.LoadSceneAsync("_CoreScene", LoadSceneMode.Additive);
    }
    
    void Start() {

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