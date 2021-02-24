using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Light))]
public class WorldLight : MonoBehaviour
{
    [Header("Light entity")]
    public Light Light;

    /// Properties
    public LightGroup LightGroup;

    [Header("Properties")]
    public string Name;
    public int ID;

    /// Light properties

    public Color Color { get { return Light.color; } set { Light.color = value; } }
    public float Intensity { get { return Light.intensity; } set { Light.intensity = value; } }
    public float Range { get { return Light.range; } set { Light.range = value; } }

    /// Transform

    public Vector3 Position { get; set; }
    public Vector3 ActualPosition { get { return transform.position; } set { transform.position = value; } }

    [Header("Transform test properties")]
    public float CircleDivision = 1.2f;

    // Circle:
    float rotRadius;
    Vector2 center;

    public float RotationRadius
    {
        get { return rotRadius; }
        set
        {
            if (value == -1) { RotationRadius = (RhythmicGame.Resolution.x / CircleDivision); return; }
            if (value < 0) { Debug.LogError($"WorldLight [{Name}]: Invalid RotationRadius value: {value}"); return; }

            rotRadius = value;
            center = new Vector2(0, rotRadius);
        }
    }

    float rotValue;
    public float Rotation
    {
        get { return rotValue; }
        set
        {
            rotValue = value;

            float posX = rotRadius != 0 ? rotRadius * Mathf.Sin(value * Mathf.Deg2Rad) : 0;
            float posY = rotRadius != 0 ? rotRadius * Mathf.Cos(value * Mathf.Deg2Rad) : 0;

            ActualPosition = Position + new Vector3(posX, posY, 0);
        }
    }

    /// Functionality

    void Awake()
    {
        // EDITOR & GAME:
        Position = transform.position;

        // GAME ONLY:
        if (Application.isEditor && !Application.isPlaying) return;
    }

    void OnDrawGizmos()
    {
        if (!RhythmicGame.DebugDrawWorldLights) return;

        if (rotRadius != 0)
            Gizmos.DrawWireSphere(center - new Vector2(0, rotRadius), rotRadius);
    }
}