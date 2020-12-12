using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum HDirection { Left = 0, Right = 1 }
public enum TrackSwitchForce { None = 0, IgnoreSeeking = 1, Force = 2 }

public class AmpPlayerTrackSwitching : MonoBehaviour
{
    [Header("Common")]
    public AmpPlayer Player;
    public Tunnel Tunnel { get { return Tunnel.Instance; } }
    public SongController SongController { get { return SongController.Instance; } }
    public AmpTrackController TracksController { get { return AmpTrackController.Instance; } }
    public AmpPlayerLocomotion Locomotion;

    [Header("Properties")]
    public int StartTrackID = 0; // -1 is clear focus, < -2 does nothing

    public bool IsAnimating;
    public float EasingStrength = 10f;
    public float Duration = 1f;
    [Range(0, 1)]
    public float AnimationProgress;

    /// Functionality

    void Start() => SwitchToTrack(StartTrackID, true); // Automatically switch to a start track ID

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

        int id = TracksController.CurrentTrackID;

        if (force > 0) // Forceful
            id = Mathf.Clamp(id += (direction == HDirection.Left) ? -1 : 1,
                0, TracksController.Tracks.Count - 1);
        else if (RhythmicGame.TrackSeekingEnabled) // Track seeking
        {
            // to be added
        }

        // Switch to the destination track!
        SwitchToTrack(id);
    }
    public void SwitchTowardsDirection(HDirection direction, bool force) => SwitchTowardsDirection(direction, force ? TrackSwitchForce.IgnoreSeeking : TrackSwitchForce.None);

    public void SwitchToTrack(int ID, bool force = false)
    {
        if (ID < -1) return;
        if (ID == TracksController.CurrentTrackID) return;

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
    float pos_vel = 0f;
    float rot_vel = 0f;

    /// <summary>
    /// Prepares the track switching animation values. <br/>
    /// Also transforms the camera to properly 'wrap around' FreQ mode rotations.
    /// </summary>
    void TrackSwitchAnimation(Vector3[] trans)
    {
        // Extract Tunnel trans for easy access
        // TODO: change to just floats?
        targetPos = trans[0];
        targetRot = trans[1];

        if (RhythmicGame.DebugPlayerCameraAnimEvents)
            Debug.LogFormat("Track switching camera debug: Tunnel information: ID: {0} [pos]: {1} | [rot]: {2}", TracksController.CurrentTrackID, targetPos, targetRot);

        // Handle inverse rotations

        // When rotating beyond 180, we want a smooth transition to the inverse angles instead. 
        // We utilize the fact that negative angles are the same as 360 - (-angle) | (example:  -60 = 330) and vice-versa.
        if (!RhythmicGame.IsTunnelMode)
        {
            // LEFT
            // If the target is on the right part of the tunnel & the difference between target and camera is 180
            if ((targetRot.z > transform.eulerAngles.z) & (targetRot.z - transform.eulerAngles.z > 180))
                targetRot.z = -(360 - targetRot.z); // change to 0 - target

            // RIGHT
            // If the target is on the left part of the tunnel & the difference between camera and target is 180
            else if ((targetRot.z < transform.eulerAngles.z) & (transform.eulerAngles.z - targetRot.z > 180)) // RIGHT
                transform.eulerAngles = new Vector3(0, 0, -(360 - transform.eulerAngles.z)); // change to 360 + target
        }
    }
    private void Update() // TODO: LateUpdate?
    {
        Locomotion.PositionOffset = Mathf.SmoothDamp(Locomotion.PositionOffset, targetPos.x, ref pos_vel, 1f, 100f, EasingStrength * Time.deltaTime);
        Locomotion.RotationOffset = Mathf.SmoothDamp(Locomotion.RotationOffset, targetRot.z, ref rot_vel, 1f, 100f, EasingStrength * Time.deltaTime);

        if (!SongController.IsPlaying || !Locomotion.IsPlaying) // TEMP?: update while not playing for testing purposes
            Locomotion.Locomotion(Locomotion.DistanceTravelled);
    }
}