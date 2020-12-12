using System.Collections;
using UnityEngine;

public partial class AmpPlayer : MonoBehaviour
{
    public static AmpPlayer Instance;
    public SongController SongController { get { return SongController.Instance; } }
    public AmpTrackController TracksController { get { return AmpTrackController.Instance; } }

    // References to instances
    public AudioSource AudioSrc;

    // Properties & variables
    public float AVPlayerOffset; // AV calibration offset
    public float CameraPullbackOffset; // Camera pullback

    void Awake() => Instance = this;

    void Start()
    {
        
    }

    // Functionality
    public partial void HealthUpdate();

    void Update()
    {
        HealthUpdate();
    }
}