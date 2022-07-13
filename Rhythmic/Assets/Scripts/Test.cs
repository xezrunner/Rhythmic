using UnityEngine;

using static Logging;

public class Test : MonoBehaviour {
    void Start() {
        DebugConsole.register_command(test_log, "Logs a message.");
        DebugConsole.register_command(test_log_warn, "Logs a warning message.");
        DebugConsole.register_command(test_log_error ,"Logs an error message.");
    }
    void test_log()  => log("test!");
    void test_log_warn() => log_warn("test!");
    void test_log_error() => log_error("test!");
}