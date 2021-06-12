using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class test2 : MonoBehaviour
{
    public TMP_Text Text;
    public float Framerate = 60f;
    
    public char start_char = '\ue100';
    public char end_char = '\ue176';
    
    void Awake() => Text.text = null;
    
    public void Play()
    {
        StartCoroutine(Main_Coroutine());
        Text.text = start_char.ToString();
    }
    
    public void Stop() => stop_requested = true;
    
    bool stop_requested = false;
    IEnumerator Main_Coroutine()
    {
        char c = start_char;
        while (true)
        {
            Text.text = c.ToString();
            c++; if (c > end_char) c = start_char;
            
            float fps_ms = (1000f / Framerate);
            yield return new WaitForSeconds(fps_ms / 1000f);
            
            if (stop_requested) yield break;
        }
    }
}
