using System.Collections;
using UnityEngine;

public partial class AmpPlayer : MonoBehaviour
{
    public static AmpPlayer Instance;
    public SongController SongController { get { return SongController.Instance; } }
    public AmpTrackController TracksController { get { return AmpTrackController.Instance; } }

    // References to instances
    [Header("Common")]
    public AudioSource AudioSrc;

    // Properties & variables
    [Header("Camera properties (as reference)")]
    public float AVPlayerOffset; // AV calibration offset
    public float CameraPullbackOffset; // Camera pullback

    void Awake() => Instance = this;

    void Start()
    {
        
    }

    // Functionality
    partial void HealthUpdate();

    void Update()
    {
        HealthUpdate();
    }
}