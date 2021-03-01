//#define ALWAYS_UPDATE
#undef ALWAYS_UPDATE

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum HDirection { Left = 0, Right = 1 }
public enum TrackSwitchForce { None = 0, IgnoreSeeking = 1, Force = 2 }

public class AmpPlayerTrackSwitching : MonoBehaviour
{
    public static AmpPlayerTrackSwitching Instance;

    [Header("Common")]
    public AmpPlayer Player;
    public Tunnel Tunnel { get { return Tunnel.Instance; } }
    public SongController SongController { get { return SongController.Instance; } }
    public TracksController TracksController { get { return TracksController.Instance; } }
    public AmpPlayerLocomotion Locomotion;

    [Header("Properties")]
    public int StartTrackID = 0; // -1 is clear focus, < -2 does nothing

    public bool IsAnimating;
    public float PositionEasingStrength = 10f;
    public float RotationEasingStrength = 10f;
    public float Duration = 1f;
    [Range(0, 1)]
    public float AnimationProgress;

    /// Functionality

    void Awake() => Instance = this;
    void Start() => StartCoroutine(WaitForStart());  // Automatically switch to a start track ID, once we are loaded
    IEnumerator WaitForStart()
    {
        if (!SongController.IsEnabled) yield break;

        while (!TracksController.IsLoaded) yield return null;
        SwitchToTrack(StartTrackID, true);
    }

    /// <summary>
    /// Calls SwitchToTrack() towards the direction specified.
    /// </summary>
    /// <param name="direction">The direction of the switching</param>
    /// <param name="force">Whether to force the track switching. Force will switch to miscellaneous and disabled tracks as well.</param>
    public void SwitchTowardsDirection(HDirection direction, TrackSwitchForce force = TrackSwitchForce.None)
    {
        if (Keyboard.current.shiftKey.isPressed) // TODO: not needed once modifiers are working in Input System
            force = TrackSwitchForce.IgnoreSeeking;

        if (RhythmicGame.DebugPlayerTrackSwitchEvents)
            Debug.LogFormat("Track switching: Switching towards direction: {0} | Force mode: {1}", direction.ToString(), force.ToString());

        int id = TracksController.CurrentRealTrackID;

        if (force > 0) // Forceful
        {
            // Handle edges, roll over in Tunnel mode or when wrapping!
            if (!RhythmicGame.IsTunnelMode) // Regular mode
                id = Mathf.Clamp(id += (direction == HDirection.Left) ? -1 : 1,
                    0, TracksController.Tracks.Count - 1);
            else // Tunnel mode
            {
                id += (direction == HDirection.Left) ? -1 : 1;
                if (id == TracksController.Tracks.Count) id = 0;
                else if (id == -1) id = TracksController.Tracks.Count - 1;
            }
        }
        else
        {
            // While seeking is unimplemented, redirect to forceful switching
            SwitchTowardsDirection(direction, true);
            return;
        }
        //else if (RhythmicGame.TrackSeekingEnabled) // Track seeking
        //{
        //    // to be added
        //}

        // Switch to the destination track!
        SwitchToTrack(id);
    }
    public void SwitchTowardsDirection(HDirection direction, bool force) => SwitchTowardsDirection(direction, force ? TrackSwitchForce.IgnoreSeeking : TrackSwitchForce.None);

    public void SwitchToTrack(int ID, bool force = false)
    {
        if (ID < -1) return;
        if (ID == TracksController.CurrentRealTrackID) return;

        // Get the track for the ID
        AmpTrack track = TracksController.Tracks[ID];
        if (track is null) Debug.LogErrorFormat("Track switching: Invalid track ID: {0}", ID);

        if (RhythmicGame.DebugPlayerTrackSwitchEvents) Debug.LogFormat("Track switching: Switching to track: {0} [{1}] [force: {2}]", ID, track.TrackName != "" ? track.TrackName : track.name, force);

        // Forbid FREESTYLE tracks based on policy
        if (track.Instrument == AmpTrack.InstrumentType.FREESTYLE)
        {
            if (!force && !RhythmicGame.PlayableFreestyleTracks)
            {
                Debug.LogErrorFormat("Track switching: Track ID {0} is a FREESTYLE track. Playable freestyle tracks are turned off or not a forceful enough switch!");
                return;
            }
        }
        else if (track.Instrument == AmpTrack.InstrumentType.bg_click & !force) return;

        IsAnimating = true;

        // Propagate track switching to tracks controller!
        TracksController.SwitchToTrack(track);

        // Camera work
        // Grab Tunnel transform information about track
        Vector3[] trans = Tunnel.GetTransformForTrackID(ID);

        // Begin animation
        TrackSwitchAnimation(trans);
    }

    // Target transforms from Tunnel
    Vector3 targetPos;
    Vector3 targetRot;

    // Velocity storage for SmoothDamp()
    Vector3 pos_vel;
    Vector3 rot_vel;

    public string debug;

    /// <summary>
    /// Prepares the track switching animation values. <br/>
    /// Also transforms the camera to properly 'wrap around' FreQ mode rotations.
    /// </summary>
    void TrackSwitchAnimation(Vector3[] trans)
    {
        targetPos = trans[0];

        // When rotating beyond 180, we want a smooth transition to the inverse angles instead. 
        if (targetRot.z > 180 && trans[1].z < 180)
        {
            Vector3 target = Locomotion.RotationOffset;
            target.z = -(360 - target.z);
            Locomotion.SetTunnelRotationImmediate(target);
        }
        else if (targetRot.z < 180 && trans[1].z > 180)
        {
            Vector3 target = Locomotion.RotationOffset;
            target.z = (360 + target.z);
            Locomotion.SetTunnelRotationImmediate(target);
        }

        targetRot = trans[1];

        if (RhythmicGame.DebugPlayerCameraAnimEvents)
            Debug.LogFormat("Track switching camera debug: Tunnel information: ID: {0} [pos]: {1} | [rot]: {2}", TracksController.CurrentTrackID, targetPos, targetRot);
        Locomotion.TunnelRotation = targetRot;
    }

    private void Update() // TODO: LateUpdate?
    {
        // TEMP (?)
#if ALWAYS_UPDATE
        if (!SongController.IsPlaying || !Locomotion.IsPlaying) // TEMP?: update while not playing for testing purposes
            Locomotion.Locomotion(Locomotion.DistanceTravelled);
#endif

        if (!SongController.IsPlaying && !Locomotion.IsPlaying) return;

        Locomotion.PositionOffset = Vector3.SmoothDamp(Locomotion.PositionOffset, targetPos, ref pos_vel, 1f, 100f, PositionEasingStrength * Time.deltaTime);
        Locomotion.RotationOffset = Vector3.SmoothDamp(Locomotion.RotationOffset, RhythmicGame.IsTunnelMode ? targetRot : Vector3.zero,
                                                       ref rot_vel, 1f, 100f, RotationEasingStrength * Time.deltaTime);
    }
}