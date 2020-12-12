using PathCreation;
using System;
using UnityEngine;

public class AmpPlayerLocomotion : MonoBehaviour
{
    [Header("Common")]
    public AmpPlayer Player;
    public PathCreator PathSystem;
    public VertexPath Path;
    public Tunnel Tunnel { get { return Tunnel.Instance; } }
    public SongController SongController { get { return SongController.Instance; } }
    public AmpTrackController TracksController { get { return AmpTrackController.Instance; } }

    [Header("Camera and objects")]
    public Camera MainCamera;
    public Transform Interpolatable;
    public Transform Contents;

    [Header("Properties")]
    public float EasingStrength = 1.0f;
    public float DistanceTravelled;

    [Header("Track switching")]
    public float PositionOffset;
    public float RotationOffset;

    void Start()
    {
        if (Path == null) GetPath();

        Locomotion(); // Position player on path right away
    }
    void GetPath()
    {
        if (PathSystem != null & Path is null)
            Path = PathSystem.path;
        else
            Debug.Log("Locomotion: Path system was not attached!");

        if (Path != null) return;

        // If the path is still not found:
        Debug.LogWarningFormat("Locomotion: Path is null! - Finding \"Path\" GameObject...");
        var path = GameObject.Find("Path").GetComponent<PathCreator>().path;

        if (path == null)
            Debug.LogError("Locomotion: Path not found! Locomotion fallback to straight path!");
        else
            Debug.Log("Locomotion: Path found!");

        Path = path;
    }

    // Locomotion
    Quaternion rotVelocity; // Temporary value holding current rotation velocity (ref)

    /// <summary>
    /// Moves the player along the path for a given distance. <br/>
    /// If no path exists, the player is moved to the distance without taking any world contour into account.
    /// </summary>
    public void Locomotion(float distance = 0f)
    {
        if (!RhythmicGame.IsTunnelMode) // Regular mode
        {
            if (Path is null)
                transform.position = new Vector3(PositionOffset, 0, distance);
            else
            {
                Vector3 localRight = Path.GetNormalAtDistance(distance);
                Vector3 targetPos = Path.GetPointAtDistance(distance) + (localRight * PositionOffset);
                transform.position = targetPos;

                Quaternion currentRot = Interpolatable.rotation;
                Quaternion targetRot = Path.GetRotationAtDistance(distance) * Quaternion.Euler(0, 0, 90);
                Interpolatable.rotation = QuaternionUtil.SmoothDamp(currentRot, targetRot, ref rotVelocity, EasingStrength);
                Contents.rotation = targetRot;

                transform.eulerAngles = new Vector3(0, 0, RotationOffset);
            }
        }
        else // Tunnel mode
        {
            Debug.LogError("Locomotion: Tunnel mode not yet implemented!");
            return;
        }
    }

    [Header("Testing properties")]
    public bool IsPlaying; // TEMP

    float step;
    void Update()
    {
        if (SongController.IsPlaying || IsPlaying)
        {
            step = (4f * SongController.secInzPos) * Time.unscaledDeltaTime * SongController.songSpeed;

            if (SongController.Enabled)
                DistanceTravelled = Mathf.MoveTowards(DistanceTravelled, float.MaxValue, step);
            else
                DistanceTravelled += 4f * Time.deltaTime;

            Locomotion(DistanceTravelled);
        }
    }
}