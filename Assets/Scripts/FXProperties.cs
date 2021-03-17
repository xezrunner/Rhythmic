using UnityEngine;

public enum DestructFXPolicy { Proximity, ProximitySparkles, Detailed, Cheap };

public class FXProperties : MonoBehaviour
{
    public static FXProperties Instance;

    void Awake() => Instance = this;

    [Header("Note dot light FX")]
    //public float Note_DotLightIntensity = 1;
    public float Note_DotLightGlowIntensity = 15;
    public float Note_DotLightPulseIntensity = 2;
    public float Note_DotLightAnimStep = 1; // in seconds

    [Header("Track destruct FX")]
    // If using the Proximity policy, effects are detailed while we're relatively close to them (in distance).
    // Detailed or cheap are constant throughout the effect duration.
    // TODO: decide effect properties when not in proximity
    public DestructFXPolicy Destruct_Policy = DestructFXPolicy.Proximity;
    public bool Destruct_ForceEffects = false; // TEST: force effects even when we shouldn't play them
    public float Destruct_ProximityDistanceBar = 3; // TODO: time units?

    public float Destruct_ShardGlow = 2.6f;
    public float Destruct_SparkleGlow = 2.4f;
}