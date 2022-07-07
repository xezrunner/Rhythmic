using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugConsole : MonoBehaviour {
    static DebugConsole instance;
    public static DebugConsole get_instance() {
        if (instance) return instance;
        return null;
    }

    public static void write_line(string message, Logging.LogLevel level) {
        
    }
}
