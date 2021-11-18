#define DEBUGSYS_USE_INPUT_SYSTEM

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using static Logger;
using UnityEngine.InputSystem.UI;

public class DebugSystem : MonoBehaviour
{
    public const string PREFAB_PATH = "Prefabs/DebugSystem";

    public static DebugSystem Instance;

    public DebugSystemStartupManager startup_manager;
    public List<DebugCom> components = new List<DebugCom>();

    [Header("Content references")]
    public RectTransform UI_Canvas;
    public TMP_Text Framerate_Text;
    public TMP_Text DebugMenu_Text;
    public TMP_Text QuickLine_Text;
    public TMP_Text DebugComponent_Text;

    public static bool DEBUGSYSTEM_WarnOnEventSystemInexistence = true;

    public void Awake()
    {
        // Destroy if an instance already exists:
        if (Instance)
        {
            Destroy(gameObject);
            LogE("An instance already exists!".T(this));
        }
        Instance = this;


        if (!startup_manager && LogW("No startup components manager was assigned.".TM(this))) return;
        List<Type> startup_types = startup_manager.GetStartupComsTypeList();
        if (startup_types.Count == 0 && LogW("There are no coms to load!".T(this))) return;

        LoadStartupComs(startup_types);

        // Create an EventSystem for the UI canvas if one doesn't already exist:
        if (!FindObjectOfType<EventSystem>())
        {
            GameObject obj = new GameObject("EventSystem");
            obj.AddComponent<EventSystem>();

#if DEBUGSYS_USE_INPUT_SYSTEM
            obj.AddComponent<InputSystemUIInputModule>();
#else
            obj.AddComponent<StandaloneInputModule>();
#endif

            if (DEBUGSYSTEM_WarnOnEventSystemInexistence)
                LogW("There was no EventSystem / UI Input Manager, so we created them.".T(this));
        }

        // TEST:
        SwitchToComponent(0);
    }

    void LoadStartupComs(List<Type> types)
    {
        foreach (Type t in types)
            AddComponent(t);
    }

    public void AddComponent(Type type, string id = "")
    {
        if (type == null && LogE("Type was null!".TM(this))) return;
        if (type.BaseType != typeof(DebugCom) && LogE("Invalid type passed: %".TM(this), type.Name))
            return;

        DebugComAttribute attr = (DebugComAttribute)Attribute.GetCustomAttribute(type, typeof(DebugComAttribute));
        if (attr == null) attr = new DebugComAttribute();

        // If we have a prefab, redirect to prefab procedure:
        if (attr.is_prefab)
        {
            AddUIDebugPrefab(attr.prefab_path);
            return;
        }

        DebugCom com = (DebugCom)gameObject.AddComponent(type);
        if (!com) return;

        if (id != "") com.com_id = id; // Assign ID in case we want a component to be unique
    }
    public void AddUIDebugPrefab(string prefab_path, string id = "")
    {
        GameObject prefab = (GameObject)Resources.Load(prefab_path);
        if (!prefab && LogE("Prefab was null! - '%'".TM(this), prefab_path)) return;

        GameObject obj = Instantiate(prefab, UI_Canvas.transform);
        obj.name.Insert(0, "dP - ");

        DebugCom com = obj.GetComponent<DebugCom>();
        com.Prefab_Parent = obj.transform;

        if (!com)
        {
            LogE("Could not find a DebugCom in prefab '%'.".TM(this), prefab_path);
            Destroy(obj);
            return;
        }

        if (id != "") com.com_id = id;
        components.Add(com);
    }

    public void RemoveComponent(Type type)
    {
        if (type.BaseType != typeof(DebugCom) && LogE("Invalid type passed: %".T(this), type.Name))
            return;

        // Remove all occurances of given type.
        int found = 0;
        foreach (DebugCom com in components)
            if (com.GetType() == type) { com.Com_Destroy(); ++found; }

        if (found == 0) LogW("There was no component of type '%'.".T(this), type.Name);
    }
    public void RemoveComponent(string id)
    {
        // Remove unique item.
        foreach (DebugCom com in components)
            if (com.com_id == id) { com.Com_Destroy(); return; }

        LogW("There was no component with ID '%'.".T(this), id);
    }

    // --------------- //

    DebugCom CurrentComponent;

    public void SwitchToComponent(int index)
    {
        if (components == null || components.Count == 0) return;
        CurrentComponent = components[0];
        // Log("Switched to '%'.".T(this), CurrentComponent.GetType().Name);
        HandleCurrentComponent();
    }
    public void SwitchToComponent(Type type)
    {
        foreach (DebugCom com in components)
        {
            Type t = com.GetType();
            if (t != type) continue;

            CurrentComponent = com;
            Log("Switched to '%'.".T(this), t.Name);

            HandleCurrentComponent();
            return;
        }

        LogE("Could not find component '%'.".TM(this), type.Name);
    }

    public void HandleCurrentComponent()
    {
        if (!CurrentComponent) return;
        DebugComponent_Text.SetText(CurrentComponent.Com_Main());
    }

    float elapsed_t;
    void UPDATE_HandleComponent(bool force = false)
    {
        if (!CurrentComponent) return;

        if (CurrentComponent.Attribute.update_freq < 0) return;
        if (elapsed_t > CurrentComponent.Attribute.update_freq)
        {
            elapsed_t = 0;
            HandleCurrentComponent();
        }

        elapsed_t += Time.unscaledDeltaTime;
    }

    float framerate_delta;
    void UPDATE_FramerateUI()
    {
        if (Time.timeScale == 0) return;

        framerate_delta += (Time.unscaledDeltaTime - framerate_delta) * 0.1f;
        int fps = Mathf.CeilToInt(1.0f / framerate_delta);
        Framerate_Text?.SetText("Framerate: % FPS".Parse(fps).AddColor(GetFramerateColor(fps)));
    }

    Color GetFramerateColor(int fps)
    {
        switch (fps) // NOTE: order matters here!
        {
            default: return Colors.Unimportant;
            case int _ when fps < CoreGameUtils.FRAMERATE_LowestAcceptable:
                return Colors.Error;
            case int _ when fps < (float)Screen.currentResolution.refreshRate:
                return Colors.Warning;
            case int _ when fps < Application.targetFrameRate - 1:
                return Colors.Info;
        }
    }

    public void Update()
    {
        UPDATE_HandleComponent();
        UPDATE_FramerateUI();
    }

    // --------------- //

    public static GameObject CreateDebugSystemObject()
    {
        GameObject prefab = (GameObject)Resources.Load(PREFAB_PATH);
        return Instantiate(prefab);
    }

#if UNITY_EDITOR
    [MenuItem("GameObject/Create DebugSystem", priority = 0)]
    public static void EDITOR_CreateDebugSystemPrefab()
    {
        UnityEngine.Object prefab = Resources.Load(PREFAB_PATH);
        PrefabUtility.InstantiatePrefab(prefab);
    }
#endif
}
