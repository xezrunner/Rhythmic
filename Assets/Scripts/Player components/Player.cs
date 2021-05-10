using UnityEngine;

public partial class Player : MonoBehaviour
{
    public static Player Instance;
    public SongController SongController { get { return SongController.Instance; } }
    public TracksController TracksController { get { return TracksController.Instance; } }

    [Header("Common")]
    public AudioSource AudioSrc;

    [Header("Properties")]
    public string Name;

    [Header("Camera properties")]
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