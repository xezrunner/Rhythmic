using System.Collections.Generic;
using UnityEngine;

public enum DebugControllerState
{
    Uninitialized = 0,
    None = 1 << 0,
    DebugKeys = 1 << 1,
    DebugLogging = 1 << 2,

    DebugUI = 1 << 3,
    DebugMenu = 1 << 4,
    DebugInterfaces = 1 << 5,
    DebugEditor = 1 << 6,

    MorlettoDebug = 1 << 7,

    Default = Level1,
    Full = (DebugKeys | DebugLogging | DebugUI | DebugMenu | DebugInterfaces | DebugEditor | MorlettoDebug),
    Level0 = (DebugKeys | DebugUI | DebugMenu),
    Level1 = (DebugKeys | DebugLogging | DebugUI | DebugMenu | DebugEditor),
    Level2 = (DebugKeys | DebugLogging | DebugUI | DebugMenu | DebugInterfaces | DebugEditor),
}

public class DebugController : MonoBehaviour
{
    public static DebugController Instance;

    [Header("Content references")]
    public Transform UICanvas;

    [Header("Properties")]
    public DebugControllerState DefaultState = DebugControllerState.Default;
    DebugControllerState _State = DebugControllerState.Uninitialized;
    public DebugControllerState State
    {
        get { return _State; }
        set { if (value == _State) return; _State = value; HandleState(); }
    }

    void Awake()
    {
        Instance = this;
        GameState.CreateGameState(); // Create GameState in case game was started abnormally
    }
    void Start()
    {
        // TODO TODO TODO!!! This causes a Unity bug where the editor locks up when the server is not properly closed & disposed!
        // Start Rhythmic Console Server (for standalone console app)
        //ConsoleServer.StartConsoleServer();

        if (RhythmicGame.DebugControllerState == DebugControllerState.Uninitialized)
            State = DefaultState;
        else
        {
            bool needsInit = State == RhythmicGame.DebugControllerState;
            State = RhythmicGame.DebugControllerState;
            if (needsInit) HandleState();
        }
    }

    void HandleState()
    {
        foreach (KeyValuePair<DebugComAttribute, object> kv in DebugComponent.Components)
        {
            DebugComAttribute com_attr = kv.Key;

            DebugControllerState com_state = com_attr.State;
            DebugComponentType com_type = com_attr.ComponentType;
            GameObject com_prefab = null;
            DebugComponent com_instance;

            // Grab the values
            if (kv.Value == null)
            { Debug.LogError("Debug component value was null!"); continue; }

            if (com_type == DebugComponentType.Prefab)
            {
                object[] values = (object[])kv.Value;
                com_instance = (values[0] != null) ? (DebugComponent)values[0] : null;
                com_prefab = (values[1] != null) ? (GameObject)values[1] : null;
            }
            else
                com_instance = (DebugComponent)kv.Value;

            if (!State.HasFlag(com_state)) // NO FLAG: Remove the debug component
            {
                com_instance.RemoveDebugComponent(com_type);
                continue;
            }
            else // FLAG: Add the debug component if it doesn't exist yet
            {
                if (com_type == DebugComponentType.Component) // TODO: different objects for each debug com?
                { if (com_instance == null && com_attr.Type != null) gameObject.AddComponent(com_attr.Type); }
                else if (com_type == DebugComponentType.Prefab)
                {
                    if (com_instance != null) continue; // Do we already have an instance?
                    if (com_prefab == null)
                    { Logger.LogError("DebugController/InitState(): Prefab was null!"); continue; }

                    GameObject obj = Instantiate(com_prefab); // Instantiate prefab
                    obj.transform.SetParent(UICanvas, false); // Parent to the DebugController UICanvas object
                }
            }
        }
    }
}