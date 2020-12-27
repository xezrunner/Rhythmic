using System.Collections.Generic;
using UnityEngine;

public partial class AmpPlayer : MonoBehaviour
{
    public static AmpPlayer Instance;
    public SongController SongController { get { return SongController.Instance; } }
    public AmpTrackController TracksController { get { return AmpTrackController.Instance; } }

    [Header("Common")]
    public AudioSource AudioSrc;

    [Header("Camera properties (as reference)")]
    public float AVPlayerOffset; // AV calibration offset
    public float CameraPullbackOffset; // Camera pullback

    [Header("Catchers")]
    public List<Catcher> Catchers = new List<Catcher>();

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