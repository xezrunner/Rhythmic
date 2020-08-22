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

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    public TracksController TracksController { get { return TracksController.Instance; } }
    public CatcherController CatcherController { get { return CatcherController.Instance; } }
    Tunnel Tunnel { get { return Tunnel.Instance; } }

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

    public Animation move_anim;

    public AudioSource src;

    public List<AudioClip> audioClips = new List<AudioClip>();
    public AudioClip GetAudioClip(string name)
    {
        var clip = audioClips.Find(x => x.name == name);
        if (clip == null)
            clip = (AudioClip)Resources.Load("Sounds/" + name);

        return clip;
    }

    // Props

    public float CameraPullbackOffset = -5.5f;
    public float StartZOffset = 4f; // countin
    public float ZOffset = 0f; // (DEBUG) additional position offset

    public bool IsSongPlaying = false;
    public float PlayerSpeed = 8f;

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
        try
        { InputUser.PerformPairingWithDevice(Keyboard.current, InputUser.all[0]); }
        catch { }

        // Push back player by the Start ZOffset
        transform.Translate(Vector3.back * StartZOffset);
        PlayerCameraTransform.position = new Vector3(PlayerCameraTransform.position.x, PlayerCameraTransform.position.y, CameraPullbackOffset);

        // Wire up catcher events
        CatcherController.OnCatch += CatcherController_OnCatch;

        if (!AmplitudeSongController.Instance.Enabled)
            return;

        while (TracksController == null || TracksController.enabled & TracksController.Tracks[0].trackMeasures.Count < 3)
            await Task.Delay(500);

        // Offset player tunnel helper with tunnel center Y
        transform.position = Tunnel.center;
        TunnelOffsetHelper.transform.localPosition = -Tunnel.center;
        CameraTunnelOffsetHelper.transform.localPosition = -Tunnel.center;

        // TODO: we should switch to the track when the game starts
        SwitchToTrack(0, true);

        //StartCoroutine(Load("SnowMountains"));
    }

    IEnumerator Load(string levelName)
    {
        //AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
        Debug.Log("Loading progress: " + asyncOperation.progress);//should get 0 here, right?

        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone)
        {
            Debug.Log("Loading... " + (asyncOperation.progress * 100) + "%");
            if (asyncOperation.progress >= 0.9f)
                asyncOperation.allowSceneActivation = true;

            yield return null;
        }
    }

    // Score / streak system
    public bool IsMultiplierEnabled = true;
    public int Score = 0;
    [Range(1, 8)]
    public int Multiplier = 1;

    public void AddScore(int score = 1)
    {
        Score += score * (!IsMultiplierEnabled ? 1 : Mathf.Clamp(Multiplier, 1, 4)); // TODO: is Multiply powerup engaged
        ScoreText.text = Score.ToString();
    }
    public void SetScore(int score = 0)
    {
        Score = score;
        ScoreText.text = Score.ToString();
    }

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

    bool canLoseStreak = true;
    public bool EnableStreaks = true;
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

    // Player movement / track switching
    #region Player movement & track switching
    [Range(0, 1f)]
    public float Move_Progress = 0f; // progress of camera animation
    public bool IsMoving { get; set; } = false; // controls player camera update function

    Vector3[] Move_CamOffset; // this is the LOCAL pos/rot where the camera will start animating from
                              // should be the negative difference between the target GLOBAL rotation and the current GLOBAL rotation (before switching!)
                              // for ROT: it gets overriden to 360 - current GLOBAL rotation when switching from track 0 to last track

    Vector3[] Move_CamTarget; // this is the target LOCAL pos/rot where the camera will animate to
                              // should be 0,0,0 in most cases
                              // for ROT: it gets overriden to 360 when we're switching from the last track to track 0

    // Calculates and offsets camera, handles camera animation state
    // TODO: force position changing during FREESTYLE!
    public IEnumerator DoMovePlayerAnim(Vector3[] target, bool force = false)
    {
        Vector3 position = target[0];
        Vector3 rotation = target[1];

        // reset global camera offset values
        Move_CamOffset = new Vector3[2];
        Move_CamTarget = target;

        // set offset and target for camera LOCAL pos & rot
        Move_CamOffset[0] = CameraTunnelOffsetHelper.position; // position - leave Z at 0!
        Move_CamOffset[1] = Camera.eulerAngles; // rotation

        // handle inverse rotations
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

        if (RhythmicGame.DebugPlayerCameraAnimEvents)
            Debug.LogFormat("CamOffset POS: {0} | TargetCam POS: {1} | CamOffset ROT: {2} | CamTarget ROT: {3}",
                            new Vector2(Move_CamOffset[0].x, Move_CamOffset[0].y), new Vector2(Move_CamTarget[0].x, Move_CamTarget[0].y),
                            Move_CamOffset[1].z, Move_CamTarget[1].z);

        // position & rotate player immediately!
        // offset camera so it's at the previous pos & rot
        if (force || !RhythmicGame.IsTunnelMode) // in tunnel mode, don't change position!!
        {
            // In regular mode, we want to change the GLOBAL position of the tunnel helpers
            TunnelOffsetHelper.position = position;
            CameraTunnelOffsetHelper.position = Move_CamOffset[0];
        }
        transform.eulerAngles = rotation;
        Camera.eulerAngles = Move_CamOffset[1];

        // play and wait for animation
        Move_Progress = 0f;
        IsMoving = true;
        move_anim.Stop(); move_anim.Play();
        forcedPlayerMove = force;

        while (Move_Progress < 1f)
            yield return null;

        // movement finished, stop animating
        IsMoving = false;
    }
    bool forcedPlayerMove;
    // This moves the camera according to the track switching animation!
    void MovePlayerUpdate()
    {
        Vector3 pos = Vector3.Lerp(Move_CamOffset[0], Move_CamTarget[0], Move_Progress);
        Vector3 rot = Vector3.Lerp(Move_CamOffset[1], Move_CamTarget[1], Move_Progress);

        if (forcedPlayerMove || !RhythmicGame.IsTunnelMode)
            CameraTunnelOffsetHelper.position = new Vector3(pos.x, pos.y, Camera.position.z);
        Camera.eulerAngles = rot;
    }

    public event EventHandler<Track> OnTrackSwitched;
    public void MovePlayer(Vector3 position = new Vector3(), Vector3 rotation = new Vector3(), bool force = false)
    {
        Vector3 finalPos = new Vector3(position.x, position.y, transform.position.z); // ignore Z!
        Vector3[] target = new Vector3[] { finalPos, rotation };

        StartCoroutine(DoMovePlayerAnim(target, force));
    }
    public void SwitchToTrack(Track track) { SwitchToTrack(track.RealID); }
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

        // find the track by ID & seek if tunnel
        // TODO: improve seeking!
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
                if (availableMeasuresCounter > 1)
                    break;
                if (!t.trackMeasures[CatcherController.CurrentMeasureID].IsMeasureEmptyOrCaptured)
                    availableMeasuresCounter++;
            }
            if (availableMeasuresCounter > 1 & seekTryCounter < TracksController.Tracks.Count & measuresToCheck[0].IsMeasureEmptyOrCaptured & measuresToCheck[1].IsMeasureEmptyOrCaptured)
            {
                seekTryCounter++;
                try
                { SwitchToTrack(id > TracksController.CurrentTrackID ? id + 1 : id - 1);  return; }
                catch (Exception ex)
                { Debug.LogError(ex.Message); }
            }
            else
                track = TracksController.GetTrackByID(id);
        }
        else
            track = TracksController.GetTrackByID(id);

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

    // MAIN LOOP
    int prevSubbeat = 0;
    public virtual void Update()
    {
        // TRACK SWITCHING

        if (IsMoving)
            MovePlayerUpdate();

        if (prevSubbeat != CatcherController.CurrentBeatID)
        {
            AmplitudeSongController.Instance.BeatVibration();
            prevSubbeat = CatcherController.CurrentBeatID;
        }

        if (Input.GetKeyDown(KeyCode.Q))
            SwitchToTrack(0, true);

        // Debug move forward
        if (Input.GetKeyDown(KeyCode.Keypad8))
            transform.Translate(Vector3.forward * 62.9f);
        else if (Input.GetKeyDown(KeyCode.Keypad9) || (Gamepad.current != null && Gamepad.current.dpad.up.wasPressedThisFrame))
            foreach (AudioSource src in AmplitudeSongController.Instance.audiosrcList)
                src.time += 2f;

        // If the game is not Rhythmic, ignore everything below
        if (RhythmicGame.GameType != RhythmicGame._GameType.RHYTHMIC)
            return;
    }

    public virtual void BeginPlay() { }

    public void BeginPlay(CallbackContext context)
    {
        if (context.performed) BeginPlay();
    }
}
