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
    
        UI_LoadPage("MetaSystem/Intro/Intro_Xesign");
    }
    
    // ------- UI land ------- //
    
    /// The idea for MetaSystem UI:
    // We'll have Unity Prefabs serve as UI pages that we'll instantiate and parent to the UI_Canvas.
    // Some UIs may require 3D objects - we might want to enable adding Scenes as UI as well, that will get
    // special treatment in their visibility and lifetime.
    /// --------------------------
    
    List<GameObject> Pages = new List<GameObject>();
    
    /// <param name="preload_only">When true, the page will not show up / start executing immediately.</param>
    /// <returns>True if successful, false if unsuccessful.</returns>
    public bool UI_LoadPage(string page_path, bool preload_only = false, UIPageType page_type = UIPageType.Prefab)
    {
        if (page_path.IsEmpty()) return false;
        
        // Lookup: | TODO: Should probably have its own subroutine?
        GameObject obj_inst;
        if (page_type == UIPageType.Prefab)
        {
            GameObject prefab = (GameObject)Resources.Load(page_path);
            if (prefab == null) { Logger.LogE("Invalid page request: '%' as type: %", page_path, page_type); return false; }
            
            obj_inst = Instantiate(prefab, UI_Canvas.transform);
            Pages.Add(obj_inst);

            // TODO: preload_only
        }
        
        /// TODO: Scenes (?)
        
        return true;
    }
}

public enum UIPageType { Prefab, Scene }
