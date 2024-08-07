using System.Collections.Generic;
using UnityEngine;
using static Logging;

public class Test : MonoBehaviour {
    void Awake() {
    }

    void Start() {
    }

    public class test_class_for_log_dump_obj {
        public static bool static_test0 = false;
        static bool static_test1 = true;

        public int int_val = 69;
        public float float_val = 420.69f;
        public string string_val = "Hello world!";

        private int int_val_priv = 420;

        public int[] int_arr = {1, 2, 3 };
        public List<string> string_list = new() {"Hello", "world", "!" };
        public float[] null_arr = null;
        public List<double> null_list = null;
    }

    [ConsoleCommand] static void test_log_obj_dump() {
        var obj_for_testing = new test_class_for_log_dump_obj();
        log_dump_obj_with_name(obj_for_testing, nameof(obj_for_testing));
    }

    [ConsoleCommand("A test variable")]
    public static int var_a = 15;

    [ConsoleCommand("A test property")]
    public static int var_b { get; set; } = 25;

    [ConsoleCommand("Logs a test message")]
    static void test_log()  => log("test!");
    [ConsoleCommand("Logs a test warning message")]
    static void test_log_warn() => log_warning("test!");
    [ConsoleCommand("Logs a test error message")]
    static void test_log_error() => log_error("test!".color(Color.red));
}