using PathCreation;
using UnityEngine;

public enum WorldVisualQuality { Default = 0, Low = 1, Medium = 2, High = 3, Extreme = 4 }

public class WorldSystem : MonoBehaviour
{
    public static WorldSystem Instance;

    [Header("World info")]
    public string Name = "world_";
    public string FriendlyName = "The World";

    [Header("Path system")]
    public PathCreator PathCreator;
    public VertexPath Path;

    [Header("Lighting")]
    public LightManager LightManager;

    [Header("Visual properties")]
    public WorldVisualQuality WorldVisualQuality = WorldVisualQuality.Default;
    public bool GeometryEnabled = true;
    public bool LightingEnabled = true;
    public bool EffectsEnabled = true;

    [Header("Fog properties")]
    public bool FogEnabled = true;
    public float FogDensity = -1;
    public Color FogColor = new Color(0.5f, 0.5f, 0.5f);
    public bool AuxilliaryFogEnabled = true;
    public bool AtmosphericFogEnabled = true;

    void Awake()
    {
        if (Instance) // Do not allow multiple instances!
        {
            Debug.LogError($"WorldSystem [init]: Only one instance of a WorldSystem can exist! [current: {Instance.gameObject.name}]");
            return;
        }
        Instance = this;

        // Let's find the path if fallback policy is on!
        // TODO: Songs will also have the ability to have a path!
        if (RhythmicGame.AutoFindPathFallback && Path == null)
        {
            if (PathCreator == null) Debug.LogError("World: Path was null -> PathCreator was null!");
            else if (PathCreator.path == null) Debug.LogError($"World: Path was null -> PathCreator [{PathCreator.name}.path] was null!");
            else Path = PathCreator.path;
        }

        // Assign Path in the global PathTools!
        PathTools.Path = Path;
    }


}