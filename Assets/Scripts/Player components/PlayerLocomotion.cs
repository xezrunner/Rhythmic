using PathCreation;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLocomotion : MonoBehaviour
{
    public static PlayerLocomotion Instance;

    [Header("Common")]
    public Player Player;
    public PathCreator PathCreator;
    public VertexPath Path;
    public Tunnel Tunnel { get { return Tunnel.Instance; } }
    public GenericSongController SongController { get { return GenericSongController.Instance; } }
    public TracksController TracksController { get { return TracksController.Instance; } }

    [Header("Camera and containers")]
    public Camera MainCamera;
    public Transform Interpolatable;
    public Transform Interpolatable_TunnelRotation;
    public Transform NonInterpolatable;
    public Transform PlayerVisualPoint;

    Vector3 normalCameraPos;
    Vector3 closeCameraPos;
    Quaternion normalRotation;
    Quaternion closeRotation;

    [Header("Contents")]
    public Transform CatcherVisuals;

    [Header("Properties")]
    public bool SmoothEnabled = true;
    public float SmoothDuration = 1.0f;
    public float TunnelSmoothDuration = 1.0f;
    public float Speed = 4f;
    public float Step;
    public float DistanceTravelled;
    public float DistanceTravelled_SmoothFactor = 0.1f; // NOTE: 0.05f for accuracy!
    public float HorizonLength;
    public float CameraPullback = 12.8f;
    public float CameraElevation = 7.53f;
    public float PlayerVisualPoint_Pullback;

    [Header("Freestyle")]
    public bool IsFreestyle = false;

    [Header("Track switching")]
    public Vector3 PositionOffset;
    public Vector3 RotationOffset;
    public Vector3 TunnelRotation;

    private void Awake() => Instance = this;

    void Start()
    {
        DebugConsole.RegisterCommand("freestyle", () => { IsFreestyle = !IsFreestyle; Logger.LogConsole("new state: %", IsFreestyle); });
    }
    void GetPath()
    {
        if (PathCreator) Path = PathCreator.path; // Inspector override
        else Path = PathTools.Path;
    }

    // Locomotion
    Quaternion rotVelocity; // Temporary value holding current rotation velocity (ref)

    public Vector3 offset;
    
    public void SetTunnelRotationImmediate(Vector3 target)
    {
        Interpolatable_TunnelRotation.localRotation = Quaternion.Euler(target);
        RotationOffset = target;
    }
    
    //NOTE: call this in SongController when starting for the first time (?) OR first time start control here
    public void StartLocomotion()
    {
        if (Path == null) GetPath();
        if (!SongController.is_enabled) return;
        
        // Position player to tunnel
        transform.position = !RhythmicGame.IsTunnelMode ? Tunnel.center : new Vector3(Tunnel.center.x, Tunnel.center.y - Tunnel.radius);
        
        MainCamera.transform.position = new Vector3(Tunnel.center.x, Tunnel.center.y + CameraElevation, -CameraPullback);
        
        normalCameraPos = MainCamera.transform.localPosition;
        normalRotation = MainCamera.transform.localRotation;
        closeCameraPos = normalCameraPos + (Vector3.down * 5f) + (Vector3.forward * 8.5f);
        closeRotation = Quaternion.Euler(13f, 0, 0);
        
        //MainCamera.transform.localPosition -= Tunnel.center;
        // Set catcher visuals to bottom of tunnel, offset by 0.01f (up)
        CatcherVisuals.position = new Vector3(0, 0.01f, 0);
        
        DistanceTravelled = SongController.start_distance;
        
        Locomotion(DistanceTravelled, true); // Position & rotate player on path right away
    }
    /// <summary>
    /// Moves the player along the path for a given distance. <br/>
    /// If no path exists, the player is moved to the distance without taking any world contour into account.
    /// </summary>
    public void Locomotion(float distance = 0f, bool no_smooth = false)
    {
        // Set the horizon length (used by sections to clip the materials)
        HorizonLength = DistanceTravelled + (RhythmicGame.HorizonMeasures * SongController.bar_length_pos) -
            RhythmicGame.HorizonMeasuresOffset; // TODO: the individual track streaming is visible, so we temporarily offset it
        
        // Get & set the target position on the path - this isn't smoothened:
        Vector3 targetPos = PathTools.GetPositionOnPath(Path, distance, (!RhythmicGame.IsTunnelMode) ? PositionOffset : Vector3.zero); // no X movement in tunnel mode
        transform.position = targetPos;
        
        // Calculate rotation on the path:
        Quaternion pathRot = PathTools.GetRotationOnPath(Path, distance, offset);
        Quaternion tunnelRot = RhythmicGame.IsTunnelMode ? Quaternion.Euler(TunnelRotation) : Quaternion.identity;
        Quaternion totalRot = PathTools.GetRotationOnPath(Path, distance, RhythmicGame.IsTunnelMode ? TunnelRotation : Vector3.zero + offset);
        
        NonInterpolatable.localRotation = totalRot;
        Interpolatable.localRotation = no_smooth ? totalRot : QuaternionUtil.SmoothDamp(Interpolatable.localRotation, RhythmicGame.IsTunnelMode ? pathRot : totalRot, ref rotVelocity, SmoothDuration);
        
        // Different smoothing for tunnel rotation:
        //Interpolatable_TunnelRotation.localRotation = QuaternionUtil.SmoothDamp(Interpolatable_TunnelRotation.localRotation, tunnelRot, ref tunnelrotVelocity, TunnelSmoothDuration);
        Interpolatable_TunnelRotation.localRotation = no_smooth ? tunnelRot : Quaternion.Euler(RotationOffset);
    }
    
    [Header("Testing properties")]
    public bool _IsPlaying; // TEMP
    
    // TODO: This should be handled in a much cleaner and better way! (config values!)
    static bool cameraClose = false;
    Vector3 cam_posref;
    Quaternion cam_rotref;

    public float LiveCaptDist;
    
    Vector3 freestyle_visualpoint_pos_ref;
    public float freestyle_mouse_dampen = 20f;

    float dist_ref;
    void Update()
    {
        /// TODO TODO TODO: Improve this!!!

        if (_IsPlaying || SongController.is_playing)
        {
            if (SongController.is_enabled && SongController.is_playing)
            {
                Step = (Speed * SongController.pos_in_sec * Time.unscaledDeltaTime) * SongController.song_time_scale;
                //DistanceTravelled = Mathf.MoveTowards(DistanceTravelled, float.MaxValue, Step);
                DistanceTravelled = Mathf.SmoothDamp(DistanceTravelled, SongController.song_info.time_units.SecToPos(SongController.song_position), ref dist_ref, DistanceTravelled_SmoothFactor, 10000f);
            }
            
            // Camera pullback toggle:
            if (false && !IsFreestyle)
            {
                // TODO: Keymap!
                if (PlayerInputHandler.IsActive && Keyboard.current.nKey.wasPressedThisFrame || (Gamepad.current != null && Gamepad.current.rightStickButton.wasPressedThisFrame))
                    cameraClose = !cameraClose;
            }
            
            Locomotion(DistanceTravelled, !SmoothEnabled);

            // Camera pos & rot:
            {
                MainCamera.transform.localPosition = Vector3.SmoothDamp(MainCamera.transform.localPosition, cameraClose ? closeCameraPos : normalCameraPos, ref cam_posref, 0.2f);

                Vector3 freestyle_dir = (PlayerVisualPoint.localPosition - MainCamera.transform.localPosition);
                Quaternion cam_rot_target = QuaternionUtil.SmoothDamp(MainCamera.transform.localRotation, IsFreestyle ? Quaternion.LookRotation(freestyle_dir) : (cameraClose ? closeRotation : normalRotation), ref cam_rotref, 0.35f);
                MainCamera.transform.localRotation = cam_rot_target;
            }

            // Player visual point control (with mouse):
            {
                //MainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());

                Vector3 pos;
                if (IsFreestyle) /*pos = (RhythmicGame.IsTunnelMode) ? Tunnel.center : new Vector3(0, -Tunnel.diameter, 0);*/
                {
                    Vector3 mouse_pos = (new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, MainCamera.nearClipPlane));
                    //pos = MainCamera.ScreenToWorldPoint(mouse_pos);
                    pos = new Vector3(0, RhythmicGame.Resolution.y / 2, 0) / freestyle_mouse_dampen + new Vector3(mouse_pos.x - RhythmicGame.Resolution.x / 2, mouse_pos.y - RhythmicGame.Resolution.y / 2) / freestyle_mouse_dampen;
                }
                else pos = new Vector3(0, 0, PlayerVisualPoint_Pullback);

                //if (IsFreestyle) Logger.Log("mouse pos: % | pos: %", Mouse.current.position.ReadValue(), pos);

                PlayerVisualPoint.localPosition = Vector3.SmoothDamp(PlayerVisualPoint.localPosition, pos, ref freestyle_visualpoint_pos_ref, 0.2f);
            }

            if (SongController.is_song_over) return;

            // Live note capture glow thing
            // TODO: Improve performance, move to a better place (?)
            foreach (Track t in TracksController.Tracks)
            {
                if (t == TracksController.CurrentTrack && (!t.CurrentMeasure.IsEmpty & !t.CurrentMeasure.IsCaptured)) continue;
                foreach (Measure s in t.Measures)
                {
                    if (s is null) continue;
                    foreach (Note n in s.Notes)
                    {
                        if ((int)n.Distance == (int)DistanceTravelled + LiveCaptDist & n.IsCaptured)
                            n.CaptureNote(NoteCaptureFX.DotLightEffect);
                    }
                }
            }
        }
    }
}