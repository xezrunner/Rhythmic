using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Logger;

public class MetaSystem : MonoBehaviour
{
    public static MetaSystem Instance;

    public Canvas UI_Canvas;

    void Awake()
    {
        if (DetectOtherInstances()) return; // Do not continue execution if we already have an instance.
        Instance = this;
    }
    
    static bool META_DestroyOtherInstObjects = true;
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
}
