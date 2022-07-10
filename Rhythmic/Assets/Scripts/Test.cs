using UnityEngine;

using static Logging;

public class Test : MonoBehaviour {
    void Start() {
        DebugConsole.register_command(test_command);
        DebugConsole.register_command(test_command2);
        DebugConsole.register_command(test_command3);
    }
    void test_command()  => log("hi!");
    void test_command2() => log("hi2!");
    void test_command3() => log("hi3!");
}