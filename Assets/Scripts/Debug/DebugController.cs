using System;
using System.Collections.Generic;
using UnityEngine;

public enum DebugComponentFlag
{
    None = 0,
    Uninitialized = 1,
    DebugKeys = 1 << 1,
    DebugLogging = 1 << 2,

    DebugUI = 1 << 3,
    DebugMenu = 1 << 4,
    DebugStats = 1 << 5,
    DebugInterfaces = 1 << 6,
    DebugEditor = 1 << 7,

    MorlettoDebug = 1 << 8,

    Default = Level2,
    Full = (DebugKeys | DebugLogging | DebugUI | DebugMenu | DebugInterfaces | DebugEditor | MorlettoDebug),
    Level0 = (DebugKeys | DebugUI | DebugMenu),
    Level1 = (DebugKeys | DebugLogging | DebugUI | DebugMenu | DebugStats | DebugEditor),
    Level2 = (DebugKeys | DebugLogging | DebugUI | DebugMenu | DebugStats | DebugInterfaces | DebugEditor),
}

public partial class DebugController : MonoBehaviour
{
    DebugUI DebugUI { get { return DebugUI.Instance; } }

    public static DebugController Instance;

    [Header("Content references")]
    public Transform UICanvas;

    [Header("Properties")]
    [NonSerialized] public DebugComponentFlag DefaultState = DebugComponentFlag.Default; //DebugComponentFlag.DebugLogging | DebugComponentFlag.DebugUI | DebugComponentFlag.DebugMenu | DebugComponentFlag.DebugStats;
    DebugComponentFlag _State = DebugComponentFlag.Uninitialized;
    public DebugComponentFlag State
    {
        get { return _State; }
        set { if (value == _State) return; _State = value; HandleState(); }
    }

    void Awake()
    {
        if (Instance != null)
        { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(this);
        GameState.CreateGameState(); // Create GameState in case game was started abnormally
    }
    void Start()
    {
        // TODO TODO TODO!!! This causes a Unity bug where the editor locks up when the server is not properly closed & disposed!
        // Start Rhythmic Console Server (for standalone console app)
        //ConsoleServer.StartConsoleServer();

        if (RhythmicGame.DebugControllerFlags == DebugComponentFlag.Uninitialized)
            State = DefaultState;
        else
        {
            bool needsInit = State == RhythmicGame.DebugControllerFlags;
            State = RhythmicGame.DebugControllerFlags;
            if (needsInit) HandleState();
        }

        // Automatically activate DebugStats (TODO: should make DebugStats an additive component)
        // TODO: DebugStats shouldn't auto-activate based on whether the flag is present. (prop: AutoShowDebugStats?)
        if (RhythmicGame.AutoLoadDebugStats)
        {
            //DebugUI.SwitchToComponent(typeof(DebugStats)); // DebugStats is now an internal, persistent component of its own.
            DebugStats._Instance.StatsMode = StatsMode.DefaultAutoLoad;
        }
    }

    // TODO: ActiveComponents list?
    public static bool CreateContainersForComponents = true;

    void HandleState()
    {
        // Activate / Deactivate components:
        foreach (MetaDebugComponent com_meta in MetaComponents)
        {
            // Unpack:
            Type com_type = com_meta.Type;
            DebugComponentAttribute com_attr = com_meta.Attribute;
            DebugComponent com_instance = com_meta.Instance;

            DebugComponentFlag com_flag = com_attr.DebugFlag;

            // Error checking (attr):
            bool isError = false;

            if (com_attr == null)
            { Logger.LogError($"Debug component {com_type.Name} does not have an attribute!"); isError = true; }
            else if (com_attr.ComponentType == DebugComponentType.Prefab && com_attr.PrefabPath == "")
            { Logger.LogError($"Debug component {com_type.Name} has Prefab component type with no prefab path specified!"); isError = true; }

            if (isError) continue;

            // Instance is null - create component!
            if (State.HasFlag(com_flag) && com_instance == null)
            {
                // Component:
                if (com_attr.ComponentType == DebugComponentType.Component)
                {
                    GameObject parent_obj;

                    if (CreateContainersForComponents)
                    {
                        parent_obj = new GameObject();
                        parent_obj.name = com_type.Name;
                        parent_obj.transform.SetParent(gameObject.transform);
                    }
                    else
                        parent_obj = gameObject;

                    // Create and add component:
                    com_instance = (DebugComponent)parent_obj.AddComponent(com_type);
                    // Assign SelfParent if the component has its own container!
                    com_instance.SelfParent = (parent_obj != gameObject) ? parent_obj : null;
                }
                // Prefab:
                else if (com_attr.ComponentType == DebugComponentType.Prefab)
                {
                    GameObject com_prefab = (GameObject)Resources.Load(com_attr.PrefabPath);

                    GameObject com_obj = Instantiate(com_prefab);
                    com_obj.name = com_type.Name;

                    // Special case: DebugUI goes under UICanvas!
                    Transform parent = (com_type == typeof(DebugUI)) ? UICanvas.transform : transform;
                    com_obj.transform.SetParent(parent, false); // If attaching to UICanvas, the param worldPositionStays needs to be false.
                }
            }
            // Instance exists, but we do not have its flag! - remove component!
            else if (!State.HasFlag(com_flag) && com_instance)
                com_instance.RemoveComponent();
        }
    }
}