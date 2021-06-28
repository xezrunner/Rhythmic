using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckgateController : MonoBehaviour
{
    Clock Clock;
    Player Player;
    ChromaticSeparation FX;
    
    void Start()
    {
        Clock = Clock.Instance;
        Player = Player.Instance;
        FX = Player.ChromaticSeparation_FX;
    }

    // NOTE: These should have the same values! (Default are likely 0f when initializing the array.)
    public float[] checkgate_anim_values = new float[6] { 2f, 0.05f, 1f, 0f, 0.3f, 0f };
    public float[] checkgate_anim_timings = new float[6] { 0.5f, 0.06f, 0.4f, 0.35f, 0.5f, 0f };
    
    public void Checkgate_Action(/*Checkgate object arg?*/)
    {
        Logger.Log("Checkgate Action! | Player: %", Player.Instance != null);
        
        anim_active = true;
        anim_index = 0;
    }
    
    [NonSerialized] public float index_value;
    [NonSerialized] public float index_time;
    
    [NonSerialized] public bool anim_active;
    [NonSerialized] public int anim_index;
    [NonSerialized] public float anim_index_time;
    [NonSerialized] public float anim_value;
    
    float ref_anim_value;
    void Update()
    {
        // Animation:
        if (!anim_active) return;
        if (anim_index > checkgate_anim_values.Length) {
            anim_active = false; return;
        }
        
        // Grab the target index and value:
        index_value = checkgate_anim_values[anim_index];
        index_time = checkgate_anim_timings[anim_index];
        
        // Keep track of time, reset on finished:
        if (anim_index_time > index_time)
        {
            ++anim_index;
            anim_index_time = 0f;
            return;
        }
        anim_index_time += Time.deltaTime; //Clock.song_deltatime_smooth;
        
        // Animate FX:
        {
            anim_value = Mathf.Lerp(anim_value, index_value, anim_index_time / index_time);
            //FX.Intensity = anim_value;
            FX.Intensity = Mathf.SmoothDamp(FX.Intensity, anim_value, ref ref_anim_value, index_time * Time.deltaTime);
        }
    }
}
