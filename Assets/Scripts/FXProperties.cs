using UnityEngine;

public class FXProperties : MonoBehaviour
{
    public static FXProperties Instance;

    void Awake() => Instance = this;

    public float Note_DotLightIntensity = 1;
    public float Note_DotLightGlowIntensity = 15;
    public float Note_DotLightPulseIntensity = 2;
    public float Note_DotLightAnimStep = 1; // in seconds
}