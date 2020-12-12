using PathCreation;
using UnityEngine;

public class AmpPlayerLocomotion : MonoBehaviour
{
    [Header("Common")]
    public AmpPlayer Player;
    public VertexPath Path;
    public Tunnel Tunnel { get { return Tunnel.Instance; } }
    public AmpTrackController TracksController { get { return AmpTrackController.Instance; } }

    [Header("Camera and objects")]
    public Camera MainCamera;
    public Transform Interpolatable;
    public Transform Contents;

    [Header("Properties")]
    public float EasingStrength = 1.0f;

    // Track switching
    public float PositionOffset;
    public float RotationOffset;

    void Start()
    {
        // Try getting Path if not found
        if (Path == null) GetPath();
    }

    void GetPath()
    {
        Debug.LogWarningFormat("Locomotion: Path is null! - Finding \"Path\" GameObject...");
        var path = GameObject.Find("Path").GetComponent<PathCreator>().path;

        if (path == null)
            Debug.LogError("Locomotion: Path not found! Locomotion fallback to straight path!");
        else
            Path = path;
    }

    void Update()
    {
        
    }
}