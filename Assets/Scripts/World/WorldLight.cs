using System;
using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
//[RequireComponent(typeof(Light))]
public class WorldLight : MonoBehaviour
{
    #region Editor params
    [HideInInspector] public bool light_foldOut = true;

    [HideInInspector] public float MaxIntensity = 8;
    [HideInInspector] public float MaxRadius = 10000;
    [HideInInspector] public float MaxDistance = 2000;
    //[HideInInspector] public bool Editor_AllowChangingDistance = false;

    [HideInInspector] public float Gizmos_MainSize = 35;
    [HideInInspector] public static float DefaultRotationRadius = 300;
    #endregion

    [Header("Light entity")]
    public Light Light;

    /// Properties
    public LightGroup LightGroup;

    public string Name { get { return gameObject.name; } set { gameObject.name = value; } }
    [Header("Properties")]
    public int ID; // -1 is reserved!

    /// Light properties

    public Color Color { get { return Light.color; } set { Light.color = value; } }
    [NonSerialized] public float? OG_Intensity;
    public float Intensity { get { return Light.intensity; } set { Light.intensity = value; } }
    public float Range { get { return Light.range; } set { Light.range = value; } }

    /// Transform

    public Vector3 LightPosition { get { return Light.transform.localPosition; } set { Light.transform.localPosition = value; } }
    public float Distance
    {
        get { return transform.position.z; }
        set { transform.position = new Vector3(transform.position.x, transform.position.y, value); }
    }

    [Header("Transform test properties")]
    public float CircleDivision = 1.2f;

    // TODO: Angles (?)

    // Circle:
    float rotRadius;
    Vector2 center;
    public float RotationRadius
    {
        get { return rotRadius; }
        set
        {
            if (value == -1)
                //value = (RhythmicGame.Resolution.x / CircleDivision);
                value = DefaultRotationRadius;
            else if (value < 0)
            { Debug.LogError($"WorldLight [{Name}]: Invalid RotationRadius value: {value}"); return; }

            rotRadius = value;
        }
    }

    // Rotation:
    float rotValue;
    public float Rotation
    {
        get { return rotValue; }
        set
        {
            rotValue = value;

            float posX = rotRadius != 0 ? rotRadius * Mathf.Sin(value * Mathf.Deg2Rad) : 0;
            float posY = rotRadius != 0 ? rotRadius * Mathf.Cos(value * Mathf.Deg2Rad) : 0;

            LightPosition = new Vector3(posX, posY, LightPosition.z);
        }
    }

    /// Functionality

    void Awake()
    {
        // EDITOR & GAME:

        // GAME ONLY:
        if (Application.isEditor && !Application.isPlaying) return;

        // Store original Intensity
        OG_Intensity = Intensity;

        // -1 ID check
        if (ID == -1)
        {
            Logger.LogW($"WorldLight [{((Name != "" || Name != null) ? Name : gameObject.name)} has an ID of -1, which is reserved. Setting it to 0!");
            ID = 0;
        }

        // Add ourselves to the desired LightGroup
        if (LightGroup)
            LightGroup.Add(this);
        else
            Logger.LogW($"WorldLight [{((Name != "" || Name != null) ? Name : gameObject.name)} | {ID}] is not attached to a LightGroup!");
    }
    void OnDrawGizmos()
    {
        if (!RhythmicGame.DebugDrawWorldLights) return;

        var size = Gizmos_MainSize;
        Gizmos.DrawCube(transform.position, new Vector3(size, size, size));

        if (rotRadius != 0)
            Gizmos.DrawWireSphere(transform.position, RotationRadius);
    }

    public void AnimateIntensity(float target, float durationSec = 1f, float? from = null) => StartCoroutine(_AnimateIntensity(target, durationSec, from));
    IEnumerator _AnimateIntensity(float target, float durationSec = 1f, float? from = null)
    {
        if (target == -1 && OG_Intensity.HasValue)
            target = OG_Intensity.Value;

        float prevIntensity = from.HasValue ? from.Value : Intensity;

        if (durationSec <= 0)
        { Intensity = target; yield break; }

        float elapsedTime = 0;
        float t = 0;

        while (t <= 1) // TODO: < rather than <= (?)
        {
            // Lerp the intensity
            Intensity = Mathf.Lerp(prevIntensity, target, t);

            elapsedTime += Time.deltaTime; // TODO: song delta time!
            t = elapsedTime / durationSec;

            yield return null;
        }

        Logger.LogMethod("Done!", this);
    }
}