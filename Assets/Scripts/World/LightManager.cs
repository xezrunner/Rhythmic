using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    #region Editor params
    [HideInInspector] public string Editor_LightToFind;
    #endregion

    //public static LightManager Instance;

    public List<LightGroup> LightGroups = new List<LightGroup>();

    public static bool StartAllLightsFadedOut = false; // Whether all light should be set to faded out at start

    void Awake()
    {
        /*
        if (Instance) // Do not allow multiple instances!
        {
            Debug.LogError($"LightManager [init]: Only one instance of a LightSystem can exist! [current: {Instance.gameObject.name}]");
            return;
        }
        Instance = this;
        */
    }
    void Start()
    {
        Debug.Log($"LightManager: Started [object: {gameObject.name}]");

        if (StartAllLightsFadedOut)
            AnimateIntensities(LightGroups, 0, 0);

        // TEMP:
        AnimateIntensities(LightGroups, -1, 2, 0);
    }
    
    public LightGroup FindLightGroup(string name, int id = 0) => LightGroups.Where(i => i.Name == name && i.ID == id).FirstOrDefault();
    public LightGroup FindLightGroup(string name) => LightGroups.Where(i => i.Name == name).FirstOrDefault();
    public LightGroup FindLightGroup(int id) => LightGroups.Where(i => i.ID == id).FirstOrDefault();

    public WorldLight FindLight(string name, LightGroup lightGroup = null, int id = -1)
    {
        WorldLight result = null;

        foreach (LightGroup group in LightGroups)
        {
            if (lightGroup != null && group != lightGroup) continue; // Ignore other lightgroups if we are looking for a specific one
            if (result) break;

            if (id == -1)
                result = group.Find(name);
            else
                result = group.Find(name, id);
        }

        if (!result)
        {
            Logger.LogMethodE($"[{name}, {(lightGroup ? lightGroup.Name : "none")}, {id}] yielded no result!", this);
            return null;
        }
        else
            return result;
    }

    public void AnimateIntensities(List<LightGroup> groups, float target, float durationSec = 1f, float? from = null) =>
        groups.ForEach(i => AnimateIntensities(i.Lights, target, durationSec, from));
    public void AnimateIntensities(LightGroup group, float target, float durationSec = 1f, float? from = null) => AnimateIntensities(group.Lights, target, durationSec, from);
    public void AnimateIntensities(List<WorldLight> lights, float target, float durationSec = 1f, float? from = null) => 
        lights.ForEach(i => i.AnimateIntensity(target, durationSec, from));
}