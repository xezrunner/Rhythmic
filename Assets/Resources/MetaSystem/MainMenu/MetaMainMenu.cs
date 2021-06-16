using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaMainMenu : MonoBehaviour
{
    MetaSystem MetaSystem;
    
    public void Awake() => MetaSystem = MetaSystem.Instance;
    
    public void DEBUG_StartGame()
    {
        GameState.LoadScene("RH_Main");
        MetaSystem.META_SetVisibility(false);
        
        //MetaSystem.UI_UnloadPage(gameObject);    
        gameObject.SetActive(false);
    }
}
