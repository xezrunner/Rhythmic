using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using static Logger;

public class DebugSystem : MonoBehaviour
{
    public static DebugSystem Instance;

    public DebugSystemStartupComs startup_manager;
    public List<DebugCom> components = new List<DebugCom>();

    [Header("Content references")]
    public Canvas UI_Canvas;
    public TMP_Text DebugMenu_Text;
    public TMP_Text QuickLine_Text;
    public TMP_Text DebugComponent_Text;

    public void Awake()
    {
        // Destroy if an instance already exists:
        if (Instance)
        {
            Destroy(gameObject);
            LogE("An instance already exists!".T(this));
        }
        Instance = this;

        // Create an EventSystem for the UI canvas if one doesn't already exist:
        if (!FindObjectOfType<EventSystem>())
        {
            new GameObject("EventSystem").AddComponent<EventSystem>();
            LogW("There was no EventSystem, so we created one.".T(this));
        }

        if (!startup_manager && LogW("No startup components manager was assigned.".TM(this))) return;
        List<Type> startup_types = startup_manager.GetStartupComsTypeList();
        if (startup_types.Count == 0 && LogW("There are no coms to load!".T(this))) return;

        LoadStartupComs(startup_types);

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

        DebugCom com = (DebugCom)gameObject.AddComponent(type);
        if (id != "") com.id = id; // Assign ID in case we want a component to be unique
        components.Add(com);
    }
    public void AddUIDebugPrefab(string prefab_path, string id = "")
    {
        GameObject prefab = (GameObject)Resources.Load(prefab_path);
        if (!prefab && LogE("Prefab was null! - '%'".TM(this), prefab_path)) return;

        GameObject obj = Instantiate(prefab, UI_Canvas.transform);
        DebugCom com = obj.GetComponent<DebugCom>();
        com.Prefab_Parent = obj.transform;

        if (!com)
        {
            LogE("Could not find a DebugCom in prefab '%'.".TM(this), prefab_path);
            Destroy(obj);
            return;
        }

        if (id != "") com.id = id;
        components.Add(com);
    }

    public void RemoveComponent(Type type)
    {
        if (type.BaseType != typeof(DebugCom) && LogE("Invalid type passed: %".T(this), type.Name))
            return;

        // Remove all occurances of given type.
        int found = 0;
        foreach (DebugCom com in components)
            if (com.GetType() == type) { com.Destroy(); ++found; }

        if (found == 0) LogW("There was no component of type '%'.".T(this), type.Name);
    }
    public void RemoveComponent(string id)
    {
        // Remove unique item.
        foreach (DebugCom com in components)
            if (com.id == id) { com.Destroy(); return; }

        LogW("There was no component with ID '%'.".T(this), id);
    }

    // --------------- //

    DebugCom CurrentComponent;

    public void SwitchToComponent(int index)
    {
        if (components == null || components.Count == 0) return;
        CurrentComponent = components[0];
        Log("Switched to '%'.".T(this), CurrentComponent.GetType().Name);
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
        DebugComponent_Text.SetText(CurrentComponent.Main());
    }

    float elapsed_t;
    public void UPDATE_HandleComponent(bool force = false)
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

    public void Update()
    {
        UPDATE_HandleComponent();
    }

    // --------------- //

#if UNITY_EDITOR
    [MenuItem("GameObject/Create DebugSystem", priority = 0)]
    public static void CreateDebugSystemPrefab()
    {
        UnityEngine.Object prefab = Resources.Load("Prefabs/DebugSystem");
        PrefabUtility.InstantiatePrefab(prefab);
    }
#endif
}
