using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Logger;

/* TODO:
[ ] We need to figure out our EventSystem situation for the UI!
*/

public class MetaSystem : MonoBehaviour
{
    public static MetaSystem Instance;

    public Canvas UI_Canvas;
    
    void Awake()
    {
        if (DetectOtherInstances()) return; // Do not continue execution if we already have an instance.
        Instance = this;
    }
    
    static bool META_DestroyOtherInstObjects = true; // Whether to destroy GameObjects, or just the components only.
    bool DetectOtherInstances()
    {
        // Check if we already assigned an instance::
        if (Instance != null)
        {
            LogE("Another instance of MetaSystem was detected - destroying object '%'!".T(this), gameObject.name);
            if (META_DestroyOtherInstObjects) Destroy(gameObject);
            else Destroy(this);

            return true;
        }

        /// This might not be needed:
        #region GameObject checking: 
        // MetaSystem[] instances = GameObject.FindObjectsOfType<MetaSystem>();
        // foreach (MetaSystem inst in instances)
        // {
        //     if (inst != Instance)
        //     {
        //         LogE("Another instance of MetaSystem was detected - destroying object '%'!".T(this), gameObject.name);

        //         if (META_DestroyOtherInstObjects) Destroy(gameObject);
        //         else Destroy(this);

        //         return true;
        //     }
        // }
        #endregion

        return false;
    }
    
    void Start()
    {
        if (UI_Canvas) Log("We got a UI canvas!".T(this));
        else LogW("We don't have a UI Canvas!".T(this));
    }
    
    public void BUTTON_StartIntroScene() => UI_LoadPage("MetaSystem/Intro/Intro_Xesign");
    public void BUTTON_PreloadIntroScene() => UI_LoadPage("MetaSystem/Intro/Intro_Xesign", true);
    
    public MetaButton Maskability_Button;
    public void BUTTON_ToggleMaskability()
    {
        MetaButton.Maskability = !MetaButton.Maskability;
        Maskability_Button.SetText(Logger.ParseArgs("Control maskability: %", MetaButton.Maskability ? "ON" : "OFF"));
    }

    // ------- UI land ------- //

    /// The idea for MetaSystem UI:
    // We'll have Unity Prefabs serve as UI pages that we'll instantiate and parent to the UI_Canvas.
    // Some UIs may require 3D objects - we might want to enable adding Scenes as UI as well, that will get
    // special treatment in their visibility and lifetime.
    /// --------------------------

    public List<GameObject> Pages = new List<GameObject>();
    public List<GameObject> Preloaded_Pages = new List<GameObject>();

    /// <param name="preload_only">When true, the page will not show up / start executing immediately.</param>
    /// <returns>True if successful, false if unsuccessful.</returns>
    public bool UI_LoadPage(string page_path, bool preload_only = false, UIPageType page_type = UIPageType.Prefab)
    {
        if (page_path.IsEmpty()) return false;

        /// Prefabs:
        if (page_type == UIPageType.Prefab)
        {
            GameObject obj_inst = null;
            GameObject prefab = null;

            // TODO: Improve searching performance! (Hashmap? Hashtable? -> in DebugConsole as well)
            foreach (GameObject obj in Preloaded_Pages)
            {
                if (obj.name == page_path)
                {
                    // Give back the prefab if we found a pre-loaded page and we requested to load it: 
                    if (!preload_only) { prefab = obj; Log("We got a pre-loaded page: '%'".AddColor(Colors.Network), page_path); }
                    else { LogE("This page is pre-loaded already: '%'".M(), page_path); return false; }
                }
            }

            if (!prefab) prefab = (GameObject)Resources.Load(page_path);
            prefab.name = page_path;
            if (prefab == null) { LogE("Invalid page request: '%' as type: %".M(), page_path, page_type); return false; }

            if (!preload_only) // Adding prefab to UI canvas:
            {
                obj_inst = Instantiate(prefab, UI_Canvas.transform); obj_inst.name = prefab.name;
                Pages.Add(obj_inst);
                Log("Successfully loaded: '%'".AddColor(Colors.Network), page_path);
            }
            else
            {
                /// TODO: It is possible that we might want to preload things by adding them to the Canvas, but setting
                /// a special tag for them that will make them start out disabled. This way, they would reduce the load times
                /// for when we actually want to load that page in / switch to it.
                Preloaded_Pages.Add(prefab);
                Log("Successfully pre-loaded: '%'", page_path);
            }
        }

        /// TODO: Scenes (?)

        return true;
    }
}

public enum UIPageType { Prefab, Scene }
