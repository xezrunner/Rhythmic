using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static UnityEngine.InputSystem.InputAction;

public class Player : MonoBehaviour
{
    // TODO: Global dynamic haptics controller!
    // TODO: Input system wrapper!

    public static Player Instance;
    public SongController SongController { get { return SongController.Instance; } }
    public TracksController TracksController { get { return TracksController.Instance; } }
    public CatcherController CatcherController { get { return CatcherController.Instance; } }
    Tunnel Tunnel { get { return Tunnel.Instance; } }

    #region Object instances set in editor
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI MissText;

    public TextMeshProUGUI SubbeatCounterText;
    public TextMeshProUGUI MeasureCounterText;
    public TextMeshProUGUI NextNoteText;

    public Transform TunnelOffsetHelper;
    public Transform CameraTunnelOffsetHelper;
    public Transform Camera;

    public Camera PlayerCamera;
    public Transform PlayerCameraTransform;
    public GameObject StartCamera;

    public Animation move_anim;

    public AudioSource src;
    #endregion

    public List<AudioClip> audioClips = new List<AudioClip>();
    // TODO: do NOT dynamically load AudioClips! Use the Editor instead!
    public AudioClip GetAudioClip(string name)
    {
        var clip = audioClips.Find(x => x.name == name);
        if (clip == null)
            clip = (AudioClip)Resources.Load("Sounds/" + name);

        return clip;
    }

    // Props
    public float CameraPullbackOffset = -5.5f;
    public float StartZOffset = 0f;
    public static float ZOffset = 0f; // Lag compensation offset

    public bool IsPlaying; // Controlled by the SongController!
    public float PlayerSpeed = 4f;

    // Awake & Start
    void Awake()
    {
        // this Instance
        Instance = this;
    }
    public async virtual void Start()
    {
        // Disable mouse support
        UnityEngine.Cursor.visible = false;

        // BUG: Unity's new Input System won't automatically pair the Keyboard if included in Supported Devices section.
        // WORKAROUND: we manually pair it to Player 0 on startup and ignore if it's already paired
        try
        { InputUser.PerformPairingWithDevice(Keyboard.current, InputUser.all[0]); }
        catch (Exception ex)
        { Debug.LogWarningFormat("PLAYER: Keyboard pairing not required or failed.\n{0}", ex.Message); }

        // If song controller is disabled, do not continue
        if (!AmplitudeSongController.Instance.Enabled)
            return;

        // Wait for the tracks to be loaded
        //while (TracksController == null || TracksController.enabled & TracksController.Tracks[0].trackMeasures.Count < 3)
        while (RhythmicGame.IsLoading)
            await Task.Delay(500);

        // Set song movement step | TODO: revise!!!
        SongMovementStep = (PlayerSpeed * SongController.secInzPos) * Time.unscaledDeltaTime * SongController.songSpeed;

        // Push back player by the Start ZOffset
        transform.Translate(Vector3.back * StartZOffset);
        PlayerCameraTransform.position = new Vector3(PlayerCameraTransform.position.x, PlayerCameraTransform.position.y, CameraPullbackOffset);

        // Push back player by the AV calibration value
        //UpdateAVCalibrationOffset();

        // Wire up catcher events
        CatcherController.OnCatch += CatcherController_OnCatch;

        // Move player to Tunnel center and offset its content so they're at the intended place
        transform.position = Tunnel.center;
        TunnelOffsetHelper.transform.localPosition = -Tunnel.center;
        CameraTunnelOffsetHelper.transform.localPosition = -Tunnel.center;

        // TODO: we should switch to the first track when the game starts, not here!
        SwitchToTrack(0, true);
    }

    // Score / streak system
    // TODO: updating UI text is messy
    #region Score / streak system
    public int Score = 0;
    [Range(1, 8)]

    // TODO: clarify ambigious naming! [ambiguity: Multiply powerup / score multiplier] [ideas: ScoreMultiplier, ScoreMult (?)]
    // TODO: might even just be based on the streak counter?
    public bool IsMultiplierEnabled = true;
    public int Multiplier = 1;

    // Add score based on streak counter
    public void AddScore(int score = 1)
    {
        Score += score * (!IsMultiplierEnabled ? 1 : Mathf.Clamp(Multiplier, 1, 4)); // TODO: is Multiply powerup engaged
        ScoreText.text = Score.ToString();
    }
    // Set the score to an arbitrary value, ignoring the streaking system
    public void SetScore(int score = 0)
    {
        Score = score;
        ScoreText.text = Score.ToString();
    }

    // Controls streak-ability. If this is false, the streak counter will not be increased by successful catches.
    public bool EnableStreaks = true;

    // Streak losing
    // TOOD: improve!
    bool canLoseStreak = true;
    public async void LoseStreak()
    {
        // find next notes again
        CatcherController.IsSuccessfullyCatching = false;
        CatcherController.FindNextMeasuresNotes();

        if (!EnableStreaks)
            return;
        if (Multiplier == 1 || !canLoseStreak)
            return;

        TracksController.SetAllTracksCapturingState(false);
        Multiplier = 1;

        src.PlayOneShot(GetAudioClip("streak_lose"));

        canLoseStreak = false;

        MissText.gameObject.SetActive(true);
        await Task.Delay(1500);
        MissText.gameObject.SetActive(false);
        canLoseStreak = true;
    }
    #endregion

    // Catcher
    void CatcherController_OnCatch(object sender, CatcherController.CatchEventArgs e)
    {
        // On Success / Powerup:
        switch (e.catchresult)
        {
            default:
                break;
            case Catcher.CatchResult.Success: // if successfully caught a Note
            case Catcher.CatchResult.Powerup:
            {
                AddScore();
                break;
            }
            case Catcher.CatchResult.Miss: // if we pressed the wrong button or we ignored
            {
                src.PlayOneShot(GetAudioClip("catcher_miss")); DeclareMiss(e.note, e.noteMissType);
                break;
            }
            case Catcher.CatchResult.Empty: // if we pressed on an empty space
            {
                // if the track that's being played right now has an active measure (?)
                if (!TracksController.CurrentMeasure.IsMeasureEmptyOrCaptured)
                {
                    src.PlayOneShot(GetAudioClip("catcher_miss"));
                    DeclareMiss(e.note, Catcher.NoteMissType.Mispress);
                }
                else
                    src.PlayOneShot(GetAudioClip("catcher_empty"));
                break;
            }
            case Catcher.CatchResult.Inactive:
            case Catcher.CatchResult.Unknown:
                break;
        }
    }

    // TODO: cleanup, perhaps move the entire tracks failing thing into TracksController by passing along the note? Maybe this isn't even neccessary?
    // UPDATE: stuff that tackles track stuff should defo move to TracksController
    public void DeclareMiss(Note note = null, Catcher.NoteMissType? misstype = null)
    {
        switch (misstype)
        {
            default:
            case Catcher.NoteMissType.EmptyMispress:
                break;
            case Catcher.NoteMissType.EmptyIgnore:
            case Catcher.NoteMissType.Mispress:
            case Catcher.NoteMissType.Ignore:
            {
                TracksController.DisableCurrentMeasures(); LoseStreak();
                if (note != null) AmplitudeSongController.Instance.AdjustTrackVolume(note.noteTrack.ID, 0f);
                break;
            }
        }
    }

    // Player movement / track switching
    // NOTE: mentions of 'rotation' actually refer to eulerAngles.
    #region Player movement & track switching
    [Range(0, 1f)]
    public float Move_Progress = 0f; // progress of camera animation
    public bool IsMoving { get; set; } = false; // controls player camera update function

    Vector3[] Move_CamOffset; // this is the LOCAL pos/rot where the camera will be offset to and start animating from
                              // should be the negative difference between the target GLOBAL rotation and the current GLOBAL rotation (before switching!)
                              // for ROT: it gets overriden to -(360 - offset angle) when rotating to the RIGHT and the target is beyond 180 degrees (upside-down)

    Vector3[] Move_CamTarget; // this is the target LOCAL pos/rot where the camera will animate to.
                              // usually animates back to the default 0,0,0, or:
                              // for ROT: it gets overriden to -(360 - target angle) when rotating to the LEFT and the target is beyond 180 degrees (upside-down)

    // Calculates & offsets camera, animates camera and handles its animation update control state
    // TODO: force position changing during FREESTYLE & tunnel mode!
    public IEnumerator DoMovePlayerAnim(Vector3[] target, bool force = false)
    {
        Vector3 position = target[0];
        Vector3 rotation = target[1];

        // reset global camera offset values
        Move_CamOffset = new Vector3[2];
        Move_CamTarget = target;

        // set offset and target for camera LOCAL pos & rot
        Move_CamOffset[0] = CameraTunnelOffsetHelper.position; // offset is current GLOBAL position of the camera
        Move_CamOffset[1] = Camera.eulerAngles; // rotation is current GLOBAL rotation of the camera

        // Handle inverse rotations

        // When rotating beyond 180, we want a smooth transition to the inverse angles instead. 
        // We utilize the fact that negative angles are the same as 360 - (-angle) | (example:  -60 = 330) and vice-versa.
        // This way, the camera can transition without having to rotate all the way and then some to arrive at its destination.
        if (RhythmicGame.IsTunnelMode)
        {
            // LEFT
            // If the target is on the right part of the tunnel & the difference between target and camera is 180
            if ((rotation.z > Camera.eulerAngles.z) & (rotation.z - Camera.eulerAngles.z > 180))
                Move_CamTarget[1].z = -(360 - rotation.z); // change to 0 - target

            // RIGHT
            // If the target is on the left part of the tunnel & the difference between camera and target is 180
            else if ((rotation.z < Camera.eulerAngles.z) & (Camera.eulerAngles.z - rotation.z > 180)) // RIGHT
                Move_CamOffset[1] = new Vector3(0, 0, -(360 - Camera.eulerAngles.z)); // change to 360 + target
        }

        // Print debug information
        if (RhythmicGame.DebugPlayerCameraAnimEvents)
            Debug.LogFormat("CamOffset POS: {0} | TargetCam POS: {1} | CamOffset ROT: {2} | CamTarget ROT: {3}",
                            new Vector2(Move_CamOffset[0].x, Move_CamOffset[0].y), new Vector2(Move_CamTarget[0].x, Move_CamTarget[0].y),
                            Move_CamOffset[1].z, Move_CamTarget[1].z);

        // position & rotate player immediately!
        // offset camera so it's at the previous pos & rot!
        if (force || !RhythmicGame.IsTunnelMode) // in tunnel mode, don't change position unless forced!
        {
            // In regular mode, we want to change the GLOBAL position of the tunnel helpers when moving the player!
            // We want to keep the actual player Transform centered to the tunnel at all times.
            TunnelOffsetHelper.position = position;
            CameraTunnelOffsetHelper.position = Move_CamOffset[0];
        }
        transform.eulerAngles = rotation;
        Camera.eulerAngles = Move_CamOffset[1];

        // play and wait for animation
        Move_Progress = 0f;
        IsMoving = true;
        move_anim.Stop(); move_anim.Play();
        forcedPlayerMove = force; // TODO: change naming!

        while (Move_Progress < 1f)
            yield return null;

        // stop movement animation update control
        IsMoving = false;
    }

    // Applies movements to the camera according to the track switching animation - called in Update()
    bool forcedPlayerMove;
    void MovePlayerUpdate()
    {
        Vector3 pos = Vector3.Lerp(Move_CamOffset[0], Move_CamTarget[0], Move_Progress);
        Vector3 rot = Vector3.Lerp(Move_CamOffset[1], Move_CamTarget[1], Move_Progress);

        if (forcedPlayerMove || !RhythmicGame.IsTunnelMode)
            CameraTunnelOffsetHelper.position = new Vector3(pos.x, pos.y, Camera.position.z);
        Camera.eulerAngles = rot;
    }

    // Moves player to specific POS & ROT coordinates (Z is ignored)
    public void MovePlayer(Vector3 position = new Vector3(), Vector3 rotation = new Vector3(), bool force = false)
    {
        Vector3 finalPos = new Vector3(position.x, position.y, transform.position.z); // ignore Z!
        Vector3[] target = new Vector3[] { finalPos, rotation };

        StartCoroutine(DoMovePlayerAnim(target, force));
    }

    public event EventHandler<Track> OnTrackSwitched;
    public void SwitchToTrack(Track track) { SwitchToTrack(track.RealID); }

    // Switches to a specific Track realID and handles seeking
    // TODO: FIX SEEKING! (possibly without a recursive approach)
    int seekTryCounter = 0;
    public void SwitchToTrack(int id, bool force = false)
    {
        if (TracksController.Tracks.Count < 1)
        { Debug.LogWarning("PLAYER/SwitchToTrack(): No tracks are available"); return; }

        if (!RhythmicGame.IsTunnelMode)
        {
            if (id > TracksController.Tracks.Count - 1) { Debug.LogWarningFormat("PLAYER/SwitchToTrack(): Trying to switch to non-existent track {0} / {1}", id, TracksController.Tracks.Count - 1); return; }
            else if (id < 0) { Debug.LogWarningFormat("PLAYER/SwitchToTrack(): Trying to switch to non-existent track {0} / {1}", id, 0); return; }
        }
        else
        {
            if (id > TracksController.Tracks.Count - 1)
                id = 0;
            else if (id < 0)
                id = TracksController.Tracks.Count - 1;
        }

        // find the track by ID & seek
        // TODO: PLAYTEST CODE - improve seeking for main dev!
        Track track = null;
        if (RhythmicGame.TrackSeekEmpty & TracksController.CurrentMeasure.measureNum >= 4 & !force) // seek
        {
            Measure[] measuresToCheck = new Measure[3];
            measuresToCheck[0] = TracksController.Tracks[id].trackMeasures[CatcherController.CurrentMeasureID];
            measuresToCheck[1] = TracksController.Tracks[id].trackMeasures[CatcherController.CurrentMeasureID + 1];
            //measuresToCheck[2] = TracksController.Tracks[id].trackMeasures[CatcherController.CurrentMeasureID + 2];

            //if (TracksController.Tracks[id].trackMeasures[TracksController.CurrentMeasure.measureNum + 1].IsMeasureEmpty)
            int availableMeasuresCounter = 0;
            foreach (Track t in TracksController.Tracks)
            {
                if (availableMeasuresCounter >= 2)
                    break;
                if (!t.trackMeasures[CatcherController.CurrentMeasureID].IsMeasureEmptyOrCaptured)
                    availableMeasuresCounter++;
            }
            if (availableMeasuresCounter >= 2 & seekTryCounter < TracksController.Tracks.Count & measuresToCheck[0].IsMeasureEmptyOrCaptured & measuresToCheck[1].IsMeasureEmptyOrCaptured)
            {
                seekTryCounter++;
                try
                { SwitchToTrack(id > TracksController.CurrentTrackID ? id + 1 : id - 1); return; }
                catch (Exception ex)
                { Debug.LogError(ex.Message); }
            }
            else
                track = TracksController.GetTrackByID(id);
        }
        else
            track = TracksController.GetTrackByID(id);

        seekTryCounter = 0;

        if (track == null)
        { Debug.LogErrorFormat("PLAYER/SwitchToTrack(): Could not switch to track {0}", id); return; }

        seekTryCounter = 0;

        // let stuff know of the switch
        OnTrackSwitched?.Invoke(null, track);

        // move player!
        MovePlayer(track.transform.position, track.transform.eulerAngles);
    }

    public void SwitchTrackLeft(CallbackContext context) { if (context.performed) SwitchToTrack(TracksController.CurrentTrackID - 1); }
    public void SwitchTrackRight(CallbackContext context) { if (context.performed) SwitchToTrack(TracksController.CurrentTrackID + 1); }
    public void SwitchTrackLeftForce(CallbackContext context) { if (context.performed) SwitchToTrack(TracksController.CurrentTrackID - 1, true); }
    public void SwitchTrackRightForce(CallbackContext context) { if (context.performed) SwitchToTrack(TracksController.CurrentTrackID + 1, true); }
    #endregion

    // Powerups | TODO: implementation!
    // TODO: separate classes for powerups

    // PLAYTEST Cleanse
    public bool IsCleanseDeployed = false;
    public Vector3 cleanseHaptics; // large, small, duration
    public async void DeployPowerup()
    {
        if (IsCleanseDeployed)
            return;

        IsCleanseDeployed = true;

        TracksController.CurrentTrack.CaptureTrack();
        src.PlayOneShot(GetAudioClip("cleanse_deploy"));

        if (InputManager.IsController)
        {
            Gamepad.current.SetMotorSpeeds(cleanseHaptics.x, cleanseHaptics.x);
            await Task.Delay(TimeSpan.FromMilliseconds(cleanseHaptics.z));
            Gamepad.current.SetMotorSpeeds(0f, 0f);
        }

        await Task.Delay(2000);

        IsCleanseDeployed = false;
    }

    // Measures & subbeats
    public Measure GetCurrentMeasure()
    {
        return TracksController.CurrentTrack.GetMeasureForZPos(transform.position.z);
    }
    public MeasureSubBeat GetCurrentSubbeat()
    {
        return GetCurrentMeasure().GetSubbeatForZpos(transform.position.z);
    }

    // Song movement
    public void UpdateAVCalibrationOffset()
    {
        float offset = RhythmicGame.AVCalibrationOffsetMs;
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + (offset * SongController.msInzPos));
        ZOffset = offset;
    }

    // Offsets the player by given zPos
    public void OffsetPlayer(float offset) => transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + offset);

    public float SongMovementStep;
    void SongMovementUpdate()
    {
        float step = (PlayerSpeed * SongController.secInzPos) * Time.unscaledDeltaTime * SongController.songSpeed;
        Vector3 currentPoint = transform.position;
        Vector3 targetPoint = new Vector3(transform.position.x, transform.position.y, SongController.songLengthInzPos + SongController.SecTozPos(10f));
        transform.position = Vector3.MoveTowards(currentPoint, targetPoint, step);
    }

    // MAIN LOOP
    int prevSubbeat = 0;
    public virtual void Update()
    {
        // Literal player movement | track switching & Freestyle
        if (IsMoving)
            MovePlayerUpdate();

        // Song player movement
        if (IsPlaying)
            SongMovementUpdate();

        // TODO: PLAYTEST CODE - temp controller haptics to the beat until there isn't a global haptics management system!
        if (prevSubbeat != CatcherController.CurrentBeatID) { SongController.BeatVibration(); prevSubbeat = CatcherController.CurrentBeatID; }

        // Debug switch to 0th track
        if (Input.GetKeyDown(KeyCode.Q))
            SwitchToTrack(0, true);
        // Debug switch to last track
        if (Input.GetKeyDown(KeyCode.P))
            SwitchToTrack(TracksController.Tracks.Count - 1, true);

        // Debug move forward
        if (Input.GetKeyDown(KeyCode.Keypad9) || (Gamepad.current != null && Gamepad.current.dpad.up.wasPressedThisFrame))
            SongController.OffsetSong(2f);

        // If the game is not Rhythmic, ignore everything below
        if (RhythmicGame.GameType != RhythmicGame._GameType.RHYTHMIC)
            return;

        // RHYTHMIC PLAYER BEHAVIOR HERE
    }

    public virtual void BeginPlay() { }

    // TODO: PLAYTEST CODE - temp pause & resume solution
    // Improve this for stability & performance reasons!
    public bool IsPaused;
    public void TogglePause()
    {
        RhythmicGame.SetTimescale(Time.timeScale != 0 ? 0f : 1f);
        IsPlaying = !IsPlaying;
        IsPaused = !IsPaused;
    }

    public void BeginPlay(CallbackContext context)
    {
        if (context.performed)
        {
            if (AmplitudeSongController.Instance.songPosition < 0.1f)
                BeginPlay();
            else
                TogglePause();
        }
    }
}
