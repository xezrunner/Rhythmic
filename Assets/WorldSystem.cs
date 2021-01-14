using PathCreation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorldVisualQuality { Default = 0, Low = 1, Medium = 2, High = 3, Extreme = 4 }

public class WorldSystem : MonoBehaviour
{
    [Header("World info")]
    public string Name = "world_";
    public string FriendlyName = "The World";

    [Header("Path system")]
    public PathCreator PathCreator;
    public VertexPath Path;

    [Header("Visual properties")]
    public WorldVisualQuality WorldVisualQuality = WorldVisualQuality.Default;
    public bool GeometryEnabled = true;
    public bool LightingEnabled = true;
    public bool EffectsEnabled = true;

    [Header("Fog properties")]
    public bool FogEnabled = true;
    public float FogDensity = -1;
    public Color FogColor;
    public bool AuxilliaryFogEnabled = true;
    public bool AtmosphericFogEnabled = true;

    void Awake()
    {
        // Let's find the path!
        if (Path == null)
        {
            if (PathCreator == null) Debug.LogError("World: Path was null -> PathCreator was null!");
            else if (PathCreator.path == null) Debug.LogError($"World: Path was null -> PathCreator [{PathCreator.name}.path] was null!");
            else Path = PathCreator.path;
        }
    }
}
