using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    WorldSystem WorldSystem { get { return WorldSystem.Instance; } }

    [Header("World info")]
    public string Name = "world_";
    public string FriendlyName = "The World";

    [Header("Path system")]
    public PathCreator PathCreator;

    [Header("Lighting")]
    List<LightGroup> LightGroups = new List<LightGroup>();

    [Header("Fog properties")]
    public bool FogEnabled = true;
    public float FogDensity = -1;
    public Color FogColor = new Color(0.5f, 0.5f, 0.5f);
    public bool AuxilliaryFogEnabled = true;
    public bool AtmosphericFogEnabled = true;

    void OnDestroy()
    {
        // Remove our LightGroups from WorldSystem
        LightGroups.ForEach(i => WorldSystem.LightManager.LightGroups.Remove(i));
    }

    void Start()
    {
        if (!WorldSystem)
        { Logger.LogE($"World [{Name}]: WorldSystem is null!"); return; }

        // Add ourselves to WorldSystem:
        WorldSystem.CurrentWorld = this;

        // Add LightGroups to WorldSystem
        WorldSystem.AddLightGroups(LightGroups);
    }
}
