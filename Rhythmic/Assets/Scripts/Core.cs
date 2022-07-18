using UnityEngine;
using UnityEngine.EventSystems;

using static Logging;

public class Core : MonoBehaviour
{
    static Core instance;
    public static Core get_instance() {
        if (instance) return instance;
        string s = "No Core instance has been found! This is bad!";
        log_error(s);
        throw new(s);
    }

    public static bool IS_INTERNAL = true;

    [Header("Assignables")]
    [SerializeField] Canvas ui_debug_canvas;
    [SerializeField] EventSystem event_system;
    [SerializeField] DebugConsole console;

    public GameState game_state;

    void Awake() {
        instance = this;

        console = DebugConsole.get_instance();
        if (!console) log_warn("no debug console!");

        DebugConsole.write_line("[core] initialized");
    }

    void Start() {
        
    }
}