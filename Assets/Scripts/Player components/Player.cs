using System.IO;
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
    void Start() { }

    // Functionality
    partial void HealthUpdate();

    public void PlayOneShot(string sound_name, string lookup_folder = "")
    {
        // TODO: This should look up sounds in directories, probably.
        // Perhaps we should have a list of pre-cached sounds that it looks at first?
        // For now, we just look for your sound in the Resources/Sounds folder.
        string path = Path.Combine("Sounds", lookup_folder, sound_name);
        AudioClip clip = (AudioClip)Resources.Load(path);

        if (!clip) Logger.LogConsoleE("Failed to load sound clip %".M(), sound_name);
        AudioSrc.PlayOneShot(clip, 0.2f);
    }

    // Main loop:
    void Update()
    {
        HealthUpdate();
    }
}