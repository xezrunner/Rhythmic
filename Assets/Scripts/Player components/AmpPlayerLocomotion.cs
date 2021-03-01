using PathCreation;
using UnityEngine;

public class AmpPlayerLocomotion : MonoBehaviour
{
    public static AmpPlayerLocomotion Instance;

    [Header("Common")]
    public AmpPlayer Player;
    public PathCreator PathCreator;
    public VertexPath Path;
    public Tunnel Tunnel { get { return Tunnel.Instance; } }
    public SongController SongController { get { return SongController.Instance; } }
    public TracksController TracksController { get { return TracksController.Instance; } }

    [Header("Camera and containers")]
    public Camera MainCamera;
    public Transform Interpolatable;
    public Transform Interpolatable_TunnelRotation;
    public Transform NonInterpolatable;

    [Header("Contents")]
    public Transform CatcherVisuals;

    [Header("Properties")]
    public bool SmoothEnabled = true;
    public float SmoothDuration = 1.0f;
    public float TunnelSmoothDuration = 1.0f;
    public float Speed = 4f;
    public float Step;
    public float DistanceTravelled;
    public float HorizonLength;
    public float CameraPullback = 12.8f;
    public float CameraElevation = 7.53f;

    [Header("Track switching")]
    public Vector3 PositionOffset;
    public Vector3 RotationOffset;
    public Vector3 TunnelRotation;

    private void Awake() => Instance = this;

    void Start()
    {
        if (Path == null) GetPath();
        if (!SongController.IsEnabled) return;

        // Position player to tunnel
        transform.position = !RhythmicGame.IsTunnelMode ? Tunnel.center : new Vector3(Tunnel.center.x, Tunnel.center.y - Tunnel.radius);
        MainCamera.transform.position = new Vector3(Tunnel.center.x, Tunnel.center.y + CameraElevation, -CameraPullback);
        //MainCamera.transform.localPosition -= Tunnel.center;
        // Set catcher visuals to bottom of tunnel, offset by 0.01f (up)
        CatcherVisuals.position = new Vector3(0, 0.01f, 0);

        DistanceTravelled = SongController.StartDistance;

        Locomotion(DistanceTravelled, true); // Position & rotate player on path right away
    }
    void GetPath()
    {
        if (PathCreator) Path = PathCreator.path; // Inspector override
        else Path = PathTools.Path;
    }

    // Locomotion
    Quaternion rotVelocity; // Temporary value holding current rotation velocity (ref)
    Quaternion tunnelrotVelocity; // Temporary value holding current rotation velocity (ref)

    public Vector3 offset;

    public void SetTunnelRotationImmediate(Vector3 target)
    {
        Interpolatable_TunnelRotation.localRotation = Quaternion.Euler(target);
        RotationOffset = target;
    }

    /// <summary>
    /// Moves the player along the path for a given distance. <br/>
    /// If no path exists, the player is moved to the distance without taking any world contour into account.
    /// </summary>
    public void Locomotion(float distance = 0f, bool instant = false)
    {
        // Set the horizon length (used by sections to clip the materials)
        HorizonLength = DistanceTravelled - RhythmicGame.HorizonMeasuresOffset + // TODO: the individual track streaming is visible, so we temporarily offset it
            (RhythmicGame.HorizonMeasures * SongController.measureLengthInzPos);

        Vector3 targetPos = PathTools.GetPositionOnPath(Path, distance, (!RhythmicGame.IsTunnelMode) ? PositionOffset : Vector3.zero); // no X movement in tunnel mode
        transform.position = targetPos;

        Quaternion pathRot = PathTools.GetRotationOnPath(Path, distance, offset);
        Quaternion tunnelRot = RhythmicGame.IsTunnelMode ? Quaternion.Euler(TunnelRotation) : Quaternion.identity;
        Quaternion totalRot = PathTools.GetRotationOnPath(Path, distance, RhythmicGame.IsTunnelMode ? TunnelRotation : Vector3.zero + offset);

        if (instant) // Don't do smoothing
        {
            NonInterpolatable.localRotation = Interpolatable.localRotation = totalRot;
            Interpolatable_TunnelRotation.localRotation = tunnelRot;
        }
        else
        {
            NonInterpolatable.localRotation = totalRot;
            Interpolatable.localRotation = QuaternionUtil.SmoothDamp(Interpolatable.localRotation, RhythmicGame.IsTunnelMode ? pathRot : totalRot, ref rotVelocity, SmoothDuration);
            // Different smoothing for tunnel rotation:
            //Interpolatable_TunnelRotation.localRotation = QuaternionUtil.SmoothDamp(Interpolatable_TunnelRotation.localRotation, tunnelRot, ref tunnelrotVelocity, TunnelSmoothDuration);
            Interpolatable_TunnelRotation.localRotation = Quaternion.Euler(RotationOffset);
        }
    }

    [Header("Testing properties")]
    public bool IsPlaying; // TEMP

    public float LiveCaptDist;
    void Update()
    {
        if (IsPlaying || SongController.IsPlaying)
        {
            if (!SongController.IsPlaying && IsPlaying) { }
            //DistanceTravelled += 4f * Time.deltaTime;
            else if (SongController.IsEnabled)
            {
                Step = (Speed * SongController.posInSec * Time.unscaledDeltaTime * SongController.songTimeScale);
                DistanceTravelled = Mathf.MoveTowards(DistanceTravelled, float.MaxValue, Step);
            }

            Locomotion(DistanceTravelled, !SmoothEnabled);

            if (SongController.IsSongOver) return;

            // Live note capture glow thing
            foreach (AmpTrack t in TracksController.Tracks)
            {
                if (t == TracksController.CurrentTrack && (!t.CurrentMeasure.IsEmpty & !t.CurrentMeasure.IsCaptured)) continue;
                foreach (AmpTrackSection s in t.Measures)
                {
                    if (s is null) continue;
                    foreach (AmpNote n in s.Notes)
                    {
                        if ((int)n.Distance == (int)DistanceTravelled + LiveCaptDist & n.IsCaptured)
                            n.CaptureNote(NoteCaptureFX.DotLightEffect);
                    }
                }
            }
        }
    }
}