using UnityEngine;

using static Logging;

public class Test : MonoBehaviour {
    void Start() {
        DebugConsole.register_command(test_command);
    }
    void test_command() {
        log("hi!");
    }
}