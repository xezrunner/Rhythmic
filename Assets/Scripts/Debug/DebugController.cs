using System;
using System.Collections.Generic;
using UnityEngine;

public enum DebugComponentFlag
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
    public DebugComponentFlag DefaultState = DebugComponentFlag.Default;
    DebugComponentFlag _State = DebugComponentFlag.Uninitialized;
    public DebugComponentFlag State
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

        if (RhythmicGame.DebugControllerState == DebugComponentFlag.Uninitialized)
            State = DefaultState;
        else
        {
            bool needsInit = State == RhythmicGame.DebugControllerState;
            State = RhythmicGame.DebugControllerState;
            if (needsInit) HandleState();
        }
    }

    // TODO: ActiveComponents list?
    public static bool CreateContainersForComponents = true;

    void HandleState()
    {
        foreach (MetaDebugComponent com_meta in DebugComponents.GetMetaComponents())
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
                // Component
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
                else if (com_attr.ComponentType == DebugComponentType.Prefab)
                {
                    GameObject com_prefab = (GameObject)Resources.Load(com_attr.PrefabPath);

                    GameObject com_obj = Instantiate(com_prefab);
                    com_obj.name = com_type.Name;

                    // Special case: DebugUI goes under UICanvas!
                    Transform parent = (com_flag == DebugComponentFlag.DebugUI) ? UICanvas.transform : transform;
                    com_obj.transform.SetParent(parent, false); // If attaching to UICanvas, the param worldPositionStays needs to be false.
                }
            }
            // Instance exists, but we do not have its flag! - remove component!
            else if (!State.HasFlag(com_flag) && com_instance)
                com_instance.RemoveComponent();
        }
    }

    //void HandleState0()
    //{
    //    foreach (KeyValuePair<DebugComponentAttribute, object> kv in DebugComponents.GetMetaComponents())
    //    {
    //        DebugComponentAttribute com_attr = kv.Key;

    //        DebugControllerState com_state = com_attr.State;
    //        DebugComponentType com_type = com_attr.ComponentType;
    //        GameObject com_prefab = null;
    //        DebugComponent com_instance;

    //        // Grab the values
    //        if (com_type == DebugComponentType.Prefab)
    //        {
    //            if (kv.Value == null)
    //            { Debug.LogError("Debug component value was null!"); continue; }

    //            object[] values = (object[])kv.Value;
    //            com_instance = (values[0] != null) ? (DebugComponent)values[0] : null;
    //            com_prefab = (values[1] != null) ? (GameObject)values[1] : null;
    //        }
    //        else
    //            com_instance = (kv.Value != null) ? (DebugComponent)kv.Value : null;

    //        if (!State.HasFlag(com_state) && com_instance) // NO FLAG: Remove the debug component
    //        {
    //            com_instance.RemoveDebugComponent(com_type);
    //            continue;
    //        }
    //        else // FLAG: Add the debug component if it doesn't exist yet
    //        {
    //            if (com_type == DebugComponentType.Component) // TODO: different objects for each debug com?
    //            { if (com_instance == null && com_attr.Type != null) gameObject.AddComponent(com_attr.Type); }
    //            else if (com_type == DebugComponentType.Prefab)
    //            {
    //                if (com_instance != null) continue; // Do we already have an instance?
    //                if (com_prefab == null)
    //                { Logger.LogError("DebugController/InitState(): Prefab was null!"); continue; }

    //                GameObject obj = Instantiate(com_prefab); // Instantiate prefab
    //                obj.transform.SetParent(UICanvas, false); // Parent to the DebugController UICanvas object
    //            }
    //        }
    //    }
    //}
}