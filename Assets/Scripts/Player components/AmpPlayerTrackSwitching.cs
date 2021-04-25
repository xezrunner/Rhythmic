using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum HDirection { Left = 0, Right = 1 }
public enum TrackSwitchForce { None = 0, IgnoreSeeking = 1, Force = 2 }

public class AmpPlayerTrackSwitching : MonoBehaviour
{
    public static AmpPlayerTrackSwitching Instance;

    [Header("Common")]
    public AmpPlayer Player;
    Clock Clock { get { return Clock.Instance; } }
    Tunnel Tunnel { get { return Tunnel.Instance; } }
    SongController SongController { get { return SongController.Instance; } }
    TracksController TracksController { get { return TracksController.Instance; } }
    public AmpPlayerLocomotion Locomotion;

    [Header("Properties")]
    public int StartTrackID = 0; // -1 is clear focus, < -2 does nothing

    public bool IsAnimating;
    public float PositionEasingStrength = 10f;
    public float RotationEasingStrength = 10f;
    public float Duration = 1f;
    public float Duration_Damp = 0.8f;
    [Range(0, 1)]
    public float AnimationProgress;

    /// Functionality

    void Awake() => Instance = this;
    void Start() => StartCoroutine(WaitForStart());  // Automatically switch to a start track ID, once we are loaded
    // TODO: proper loading waiting!
    IEnumerator WaitForStart()
    {
        if (!SongController.IsEnabled) yield break;

        while (!TracksController || !TracksController.IsLoaded) yield return null;
        SwitchToTrack(StartTrackID, true);
    }

    public int Seek_FindTrackID(int current_id, int target_id, HDirection direction)
    {
        if (Clock.Fbar < SongController.songCountIn) return target_id; // COUNTIN: do not do seeking at countin!

        int tracks_count = TracksController.Tracks_Count;
        int i_dir = (direction == HDirection.Left) ? -1 : 1;
        int bar_to_check = Clock.Fbar + 1;
        string debug_s = "";

        // Edge-cases:
        {
            // Check how many tracks are captured:
            int captured_count = 0;
            for (int i = 0; i < tracks_count; i++)
            {
                AmpTrack t = TracksController.Tracks[i]; if (!t) continue;
                AmpTrackSection m = t.Measures[bar_to_check];
                if (m.IsCaptured || m.IsEmpty /*|| !m.IsEnabled*/) ++captured_count;
            }

            if (captured_count == tracks_count) return target_id; // If all tracks are captured, return the target_id.
            else if (captured_count == tracks_count - 1) // If we have only one track remaining, find the one with the closest start sequence:
            {
                int target = -1, t_id = 0;
                for (int i = target_id; ; i += i_dir)
                {
                    if (i <= -1 || i >= tracks_count) break;

                    AmpTrack t = TracksController.Tracks[i]; if (!t) continue;
                    if (t.Sequences.Count == 0) continue;
                    AmpTrackSection m = t.Sequences[0]; if (!m) continue;

                    // Check the distance:
                    if (target == -1 || m.ID < target) { target = m.ID; t_id = i; }
                }
                if (target != -1)
                {
                    if (RhythmicGame.DebugPlayerTrackSeekEvents) Logger.Log($"Seeking: from one track - track with closest seq: {t_id}");
                    return t_id;
                }
            }
        }

        if (RhythmicGame.DebugPlayerTrackSeekEvents) debug_s += $"Starting at: {target_id}" + $" from: {current_id} ".AddColor(Colors.Unimportant);
        for (int t_id = target_id, i = 0; ; t_id += i_dir, i += i_dir)
        {
            if (t_id <= -1 || t_id >= tracks_count) break;

            // ----- Seeking checks: ----- //
            AmpTrack t = TracksController.Tracks[t_id]; if (!t) continue;
            {
                if (t.Sequences.Count == 0) continue; // No sequences in given track - ignore!
                if (t.Measures.Count < bar_to_check) continue; // There are less measures than the bar we are trying to check on - ignore!

                AmpTrackSection m = t.Measures[bar_to_check]; if (!m) continue;
                if (!m.IsEmpty && !m.IsCaptured && m.IsEnabled)
                {
                    if (RhythmicGame.DebugPlayerTrackSeekEvents) Logger.LogMethod(debug_s + $":: (skipped {i}) | landed at {t_id} (SUCCESS)".AddColor(Colors.Network));
                    return t_id;
                }
            }
        }

        if (RhythmicGame.DebugPlayerTrackSeekEvents) Logger.Log("Seeking: " + debug_s + $":: staying at {current_id} (FAILURE)".AddColor(Colors.Error));
        return current_id;
    }

    /// <summary>Calls SwitchToTrack() towards the direction specified.</summary>
    /// <param name="direction">The direction of the switching</param>
    /// <param name="force">Whether to force the track switching. Force will switch to miscellaneous and disabled tracks as well.</param>
    public void SwitchTowardsDirection(HDirection direction, TrackSwitchForce force = TrackSwitchForce.None)
    {
        if (Keyboard.current.shiftKey.isPressed) // TODO: not needed once modifiers are working in Input System
            force = TrackSwitchForce.IgnoreSeeking;

        if (RhythmicGame.DebugPlayerTrackSwitchEvents)
            Debug.LogFormat("Track switching: Switching towards direction: {0} | Force mode: {1}", direction.ToString(), force.ToString());

        int current_id = TracksController.CurrentRealTrackID;
        int target_id = TracksController.CurrentRealTrackID;

        // Move target_id towards / to target:
        if (!RhythmicGame.IsTunnelMode)
            target_id = Mathf.Clamp(target_id + (direction == HDirection.Left ? -1 : 1), 0, TracksController.Tracks_Count - 1);
        else // Tunnel mode - rollover!
        {
            target_id += (direction == HDirection.Left) ? -1 : 1;
            if (target_id == TracksController.Tracks_Count) target_id = 0;
            else if (target_id == -1) target_id = TracksController.Tracks_Count - 1;
        }

        if (RhythmicGame.TrackSeekingEnabled)
            target_id = Seek_FindTrackID(current_id, target_id, direction);

#if false
        if (force > 0) // Forceful switch
        {
            // Handle edges, roll over in Tunnel mode or when wrapping!
            if (!RhythmicGame.IsTunnelMode) // Regular mode
                target_id = Mathf.Clamp(target_id + (direction == HDirection.Left ? -1 : 1), 0, TracksController.Tracks_Count - 1);
            else // Tunnel mode
            {
                target_id += (direction == HDirection.Left) ? -1 : 1;
                if (target_id == TracksController.Tracks_Count) target_id = 0;
                else if (target_id == -1) target_id = TracksController.Tracks_Count - 1;
            }
        }
        else if (RhythmicGame.TrackSeekingEnabled) // Track seeking
        {
            // tba ...
        }
        else
        {
            // While seeking is unimplemented, redirect to forceful switching
            SwitchTowardsDirection(direction, true);
            return;
        }
#endif

        // Switch to the destination track!
        SwitchToTrack(target_id);
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

        Duration = (Mathf.Abs(TracksController.CurrentRealTrackID - ID)) / ((Mathf.Abs(TracksController.CurrentRealTrackID - ID) * Duration_Damp));
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
        if (!SongController.IsPlaying && !Locomotion.IsPlaying) return;

        Locomotion.PositionOffset = Vector3.SmoothDamp(Locomotion.PositionOffset, targetPos, ref pos_vel, Duration, 100f, PositionEasingStrength * Time.deltaTime);
        Locomotion.RotationOffset = Vector3.SmoothDamp(Locomotion.RotationOffset, RhythmicGame.IsTunnelMode ? targetRot : Vector3.zero,
                                                       ref rot_vel, Duration, 100f, RotationEasingStrength * Time.deltaTime);
    }
}