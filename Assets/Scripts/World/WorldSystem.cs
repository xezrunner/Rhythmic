using PathCreation;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum WorldVisualQuality { Default = 0, Low = 1, Medium = 2, High = 3, Extreme = 4 }

public class WorldSystem : MonoBehaviour
{
    public static WorldSystem Instance;

    public World CurrentWorld;
    public Skybox CurrentSkybox;

    public string Name { get { return !CurrentWorld ? "null" : CurrentWorld.Name; } }
    public string FriendlyName { get { return !CurrentWorld ? "None" : CurrentWorld.FriendlyName; } }

    [Header("Path system")]
    public PathCreator PathCreator;
    public VertexPath Path;
    public bool IsPathEnabled = true;

    [Header("Lighting")]
    public LightManager LightManager;
    public void AddLightGroups(List<LightGroup> groups)
    {
        if (LightManager.LightGroups.Count > 0)
            Logger.LogMethodW("There are LightGroups for some reason...", this);

        if (groups.Count == 0)
            Logger.LogMethodW($"World {Name} has no light groups.", this);
        else
            groups.ForEach(i => LightManager.LightGroups.Add(i)); // Add all groups to our LM!
    }

    [Header("Visual properties")]
    public WorldVisualQuality WorldVisualQuality = WorldVisualQuality.Default;
    public bool GeometryEnabled = true;
    public bool LightingEnabled = true;
    public bool EffectsEnabled = true;

    [Header("[TEST] Funky contour")]
    public Vector3[] FunkyContour;

    void Awake()
    {
        if (Instance) // Do not allow multiple instances!
        {
            Logger.LogError($"WorldSystem [init]: Only one instance of a WorldSystem can exist! [current: {Instance.gameObject.name}]");
            return;
        }
        Instance = this;

        // Let's find the path if fallback policy is on!
        // TODO: Songs will also have the ability to have a path!
        if (RhythmicGame.AutoFindPathFallback && Path == null)
        {
            if (PathCreator == null) Logger.LogError("World: Path was null -> PathCreator was null!");
            else if (PathCreator.path == null) Logger.LogError($"World: Path was null -> PathCreator [{PathCreator.name}.path] was null!");
            else Path = PathCreator.path;
        }

        // Assign Path in the global PathTools!
        if (IsPathEnabled) PathTools.Path = Path;
    }
}