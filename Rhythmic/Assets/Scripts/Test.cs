using UnityEngine;

using static Logging;

public class Test : MonoBehaviour {
    void Start() {
        DebugConsole.register_command(test_log, "Logs a message.");
        DebugConsole.register_command(test_log_warn, "Logs a warning message.");
        DebugConsole.register_command(test_log_error ,"Logs an error message.");

        DebugConsole.register_command(new Ref(() => var_a, (v) => var_a = (int)v), null, "var_a");
    }

    public int var_a = 15;

    void test_log()  => log("test!");
    void test_log_warn() => log_warn("test!");
    void test_log_error() => log_error("test!".color(Color.red));
}