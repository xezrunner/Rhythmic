using PathCreation;
using System;
using UnityEngine;

public class AmpPlayerLocomotion : MonoBehaviour
{
    public static AmpPlayerLocomotion Instance;

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
    public Transform NonInterpolatable;

    [Header("Properties")]
    public float SmoothDuration = 1.0f;
    public float DistanceTravelled;
    public float HorizonLength;

    [Header("Track switching")]
    public Vector3 PositionOffset;
    public Vector3 RotationOffset;
    public Vector3 TunnelRotation;

    private void Awake() => Instance = this;

    void Start()
    {
        if (Path == null) GetPath();
        transform.position = Tunnel.center / 2;
        if (RhythmicGame.IsTunnelMode)
            MainCamera.transform.localPosition -= Tunnel.center;

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

    public Vector3 offset;

    public GameObject LengthPlane;

    /// <summary>
    /// Moves the player along the path for a given distance. <br/>
    /// If no path exists, the player is moved to the distance without taking any world contour into account.
    /// </summary>
    public void Locomotion(float distance = 0f)
    {
        // Set the horizon length (used by sections to clip the materials)
        HorizonLength = DistanceTravelled - RhythmicGame.HorizonMeasuresOffset + // TODO: the individual track streaming is visible, so we temporarily offset it
            (RhythmicGame.HorizonMeasures * SongController.measureLengthInzPos);

        if (Path is null || distance < 0f)
        {
            transform.position = PathTools.GetPositionOnPath(Path, 0, PositionOffset) + new Vector3(0, 0, distance);

            //Quaternion targetRot = Path.GetRotationAtDistance(distance) * Quaternion.Euler(0, 0, 90) * Quaternion.Euler(tunnelRotation_smooth);
            Quaternion targetRot = Quaternion.identity;
            Interpolatable.localRotation = QuaternionUtil.SmoothDamp(Interpolatable.localRotation, targetRot, ref rotVelocity, SmoothDuration);
            NonInterpolatable.localRotation = targetRot;
        }
        else
        {
            Vector3 targetPos;

            if (!RhythmicGame.IsTunnelMode)
                targetPos = PathTools.GetPositionOnPath(Path, distance, PositionOffset);
            else
                targetPos = PathTools.GetPositionOnPath(Path, distance);

            transform.position = targetPos;

            //Quaternion targetRot = Path.GetRotationAtDistance(distance) * Quaternion.Euler(0, 0, 90) * Quaternion.Euler(tunnelRotation_smooth);
            Quaternion targetRot = PathTools.GetRotationOnPath(Path, distance, TunnelRotation + offset);
            Interpolatable.localRotation = QuaternionUtil.SmoothDamp(Interpolatable.localRotation, targetRot, ref rotVelocity, SmoothDuration);
            NonInterpolatable.localRotation = targetRot;
        }
    }

    [Header("Testing properties")]
    public bool IsPlaying; // TEMP

    public float LiveCaptDist;
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

            foreach (AmpTrack t in TracksController.Tracks)
            {
                foreach (AmpTrackSection s in t.Measures)
                {
                    if (s is null) continue;
                    foreach (AmpNote n in s.Notes)
                    {
                        if ((int)n.Distance == (int)DistanceTravelled + LiveCaptDist & n.IsCaptured)
                            n.CaptureNote(true, true);
                    }
                }
            }
        }
    }
}