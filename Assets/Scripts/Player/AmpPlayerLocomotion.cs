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
    public TracksController TracksController { get { return TracksController.Instance; } }

    [Header("Camera and containers")]
    public Camera MainCamera;
    public Transform Interpolatable;
    public Transform NonInterpolatable;

    [Header("Contents")]
    public Transform CatcherVisuals;

    [Header("Properties")]
    public float SmoothDuration = 1.0f;
    public float Speed = 4f;
    public float Step;
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

        // Position player to tunnel
        transform.position = Tunnel.center / 2;
        MainCamera.transform.localPosition -= Tunnel.center;
        // Set catcher visuals to bottom of tunnel, offset by 0.01f (up)
        CatcherVisuals.position = new Vector3(0, Tunnel.radius / 2 + 0.01f, 0);

        DistanceTravelled = SongController.StartPosition;

        Locomotion(DistanceTravelled, true); // Position & rotate player on path right away
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
    public void Locomotion(float distance = 0f, bool instant = false)
    {
        // Set the horizon length (used by sections to clip the materials)
        HorizonLength = DistanceTravelled - RhythmicGame.HorizonMeasuresOffset + // TODO: the individual track streaming is visible, so we temporarily offset it
            (RhythmicGame.HorizonMeasures * SongController.measureLengthInzPos);

        if (Path is null || distance < 0f) // NEGATIVE POSITION
        {
            Quaternion targetRot = PathTools.GetRotationOnPath(Path, 0);
            transform.position = PathTools.GetPositionOnPath(Path, 0, PositionOffset) + (targetRot * new Vector3(0, 0, distance));

            if (instant)
            { Interpolatable.localRotation = NonInterpolatable.localRotation = targetRot; return; }

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

            Quaternion targetRot = PathTools.GetRotationOnPath(Path, distance, TunnelRotation + offset);

            if (instant)
            { Interpolatable.localRotation = NonInterpolatable.localRotation = targetRot; return; }

            Interpolatable.localRotation = QuaternionUtil.SmoothDamp(Interpolatable.localRotation, targetRot, ref rotVelocity, SmoothDuration);
            NonInterpolatable.localRotation = targetRot;

        }
    }

    [Header("Testing properties")]
    public bool IsPlaying; // TEMP

    public float LiveCaptDist;
    void Update()
    {
        if (SongController.IsPlaying || IsPlaying)
        {
            Step = (Speed * SongController.posInSec * Time.unscaledDeltaTime * SongController.songSpeed);

            if (SongController.Enabled)
                DistanceTravelled = Mathf.MoveTowards(DistanceTravelled, float.MaxValue, Step);
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