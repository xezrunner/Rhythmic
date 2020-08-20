using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

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

    // Props
    //public float zPos { get { return transform.position.z; } }

    public float CameraPullbackOffset = -5.5f;
    public float StartZOffset = 4f; // countin
    public float ZOffset = 0f; // (DEBUG) additional position offset

    public bool IsSongPlaying = false;
    public float PlayerSpeed = 8f;

    public int Score = 0;
    public int Multiplier = 1;

    #region AudioSource and AudioClips
    AudioSource src { get { return GetComponent<AudioSource>(); } }
    AudioClip catcher_miss { get { return (AudioClip)Resources.Load("Sounds/catcher_miss"); } }
    AudioClip catcher_empty { get { return (AudioClip)Resources.Load("Sounds/catcher_empty"); } }
    AudioClip streak_lose { get { return (AudioClip)Resources.Load("Sounds/streak_lose"); } }
    #endregion

    // Awake & Start
    void Awake()
    {
        // this Instance
        Instance = this;
    }
    public async virtual void Start()
    {
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
        MovePlayer(0, true);

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
                streakCounter++;

                //e.note.noteTrack.IsTrackBeingCaptured = true;

                break;
            }
            case Catcher.CatchResult.Miss: // if we pressed the wrong button or we ignored
            {
                src.PlayOneShot(catcher_miss);
                DeclareMiss(e.note, e.noteMissType);

                break;
            }
            case Catcher.CatchResult.Empty: // if we pressed on an empty space
            {
                // if the track that's being played right now has an active measure (?)
                if (!TracksController.CurrentMeasure.IsMeasureEmptyOrCaptured)
                {
                    src.PlayOneShot(catcher_miss);
                    DeclareMiss(e.note, Catcher.NoteMissType.Mispress);
                }
                else
                    src.PlayOneShot(catcher_empty);

                break;
            }
            case Catcher.CatchResult.Inactive:
            {

                break;
            }
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
                // Disable all current measures
                TracksController.DisableCurrentMeasures();

                break;
            }
        }

        if (misstype != Catcher.NoteMissType.EmptyMispress)
            LoseStreak();
    }

    int streakCounter = 0;
    bool canLoseStreak = true;
    public bool EnableStreaks = true;
    public async void LoseStreak()
    {
        // find next notes again
        CatcherController.IsSuccessfullyCatching = false;
        CatcherController.FindNextMeasuresNotes();

        if (!EnableStreaks)
            return;
        if (streakCounter < 1 || !canLoseStreak)
            return;

        TracksController.SetAllTracksCapturingState(false);

        streakCounter = 0;
        SetScore(0);

        src.PlayOneShot(streak_lose);

        canLoseStreak = false;

        MissText.gameObject.SetActive(true);
        await Task.Delay(1500);
        MissText.gameObject.SetActive(false);
        canLoseStreak = true;
    }

    // PLAYER POSITIONING / TRACK SWITCHING

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
    public IEnumerator DoMovePlayerAnim(Vector3[] target)
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
        if (!RhythmicGame.IsTunnelMode) // in tunnel mode, don't change position!!
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

        while (Move_Progress < 1f)
            yield return null;

        // movement finished, stop animating
        IsMoving = false;
    }
    // This moves the camera according to the track switching animation!
    void MovePlayerUpdate()
    {
        Vector3 pos = Vector3.Lerp(Move_CamOffset[0], Move_CamTarget[0], Move_Progress);
        Vector3 rot = Vector3.Lerp(Move_CamOffset[1], Move_CamTarget[1], Move_Progress);

        if (!RhythmicGame.IsTunnelMode)
            CameraTunnelOffsetHelper.position = new Vector3(pos.x, pos.y, Camera.position.z);
        Camera.eulerAngles = rot;
    }

    public event EventHandler<Track> OnTrackSwitched;
    public void MovePlayer(Vector3 position = new Vector3(), Vector3 rotation = new Vector3())
    {
        Vector3 finalPos = new Vector3(position.x, position.y, transform.position.z); // ignore Z!
        Vector3[] target = new Vector3[] { finalPos, rotation };

        StartCoroutine(DoMovePlayerAnim(target));
    }
    public void MovePlayer(Track track) { MovePlayer(track.RealID); }
    public void MovePlayer(int id, bool ignoreSeek = false)
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
        Track track;
        if (RhythmicGame.TrackSeekEmpty & TracksController.CurrentMeasure.measureNum >= 4 & !ignoreSeek) // seek
        {
            if (TracksController.Tracks[id].trackMeasures[TracksController.CurrentMeasure.measureNum + 1].IsMeasureEmpty)
            { MovePlayer(id > TracksController.CurrentTrackID ? id + 1 : id - 1); return; }
            else
                track = TracksController.GetTrackByID(id);
        }
        else
            track = TracksController.GetTrackByID(id);

        if (track == null)
        { Debug.LogErrorFormat("PLAYER/SwitchToTrack(): Could not switch to track {0}", id); return; }

        // let stuff know of the switch
        OnTrackSwitched?.Invoke(null, track);

        // move player!
        MovePlayer(track.transform.position, track.transform.eulerAngles);
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

    // Score / streak system
    public void AddScore(int score = 1)
    {
        Score += score * Multiplier;
        ScoreText.text = Score.ToString();
    }
    public void SetScore(int score = 0)
    {
        Score = score;
        ScoreText.text = Score.ToString();
    }

    // MAIN LOOP
    public virtual async void Update()
    {
        // TRACK SWITCHING

        if (IsMoving)
            MovePlayerUpdate();

        if (Input.GetKeyDown(InputManager.Player.TrackSwitching[0]))
            MovePlayer(TracksController.CurrentTrackID - 1, Input.GetKey(KeyCode.LeftShift));
        else if (Input.GetKeyDown(InputManager.Player.TrackSwitching[1]))
            MovePlayer(TracksController.CurrentTrackID + 1, Input.GetKey(KeyCode.LeftShift));
        else if (Input.GetKeyDown(KeyCode.Q))
            MovePlayer(0, true);

        // RESTART
        if (Input.GetKeyDown(KeyCode.R))
            Restart();

        // TEMP / DEBUG

        // Toggle tunnel mode
        if (Input.GetKeyDown(KeyCode.F))
        {
            RhythmicGame.IsTunnelMode = !RhythmicGame.IsTunnelMode;
            ScoreText.text = string.Format("Tunnel {0}\nRestart!", RhythmicGame.IsTunnelMode ? "ON" : "OFF");
        }

        // FPS Lock
        if (Input.GetKeyDown(KeyCode.F1)) { RhythmicGame.SetFramerate(60); ScoreText.text = "60 FPS"; }
        else if (Input.GetKeyDown(KeyCode.F2)) { RhythmicGame.SetFramerate(0); ScoreText.text = "No FPS Lock"; }

        // Track capturing debug
        if (Input.GetKeyDown(KeyCode.H)) // current track, 5
            TracksController.CurrentTrack.CaptureMeasuresRange(GetCurrentMeasure().measureNum, 5);

        else if (Input.GetKeyDown(KeyCode.Keypad5)) // 5
            foreach (Track track in TracksController.CurrentTrackSet)
                track.CaptureMeasuresRange(GetCurrentMeasure().measureNum, 5);

        else if (Input.GetKeyDown(KeyCode.Keypad6)) // all!
            foreach (Track track in TracksController.CurrentTrackSet)
                track.CaptureMeasures(GetCurrentMeasure().measureNum, track.trackMeasures.Count - 1);

        // Track restoration (buggy!)
        if (Input.GetKeyDown(KeyCode.Keypad7))
        {
            foreach (Track track in TracksController.Tracks)
            {
                foreach (Measure measure in track.trackMeasures)
                {
                    if (!measure.IsMeasureCaptured)
                        continue;

                    measure.IsMeasureCaptured = false;
                    measure.IsMeasureEmpty = false;
                    measure.IsMeasureActive = true;
                    measure.IsMeasureCapturing = true;
                    measure.CaptureLength = 0f;
                    await Task.Delay(1);
                    measure.IsMeasureCapturing = false;
                }
                foreach (Note note in track.trackNotes)
                {
                    note.IsNoteCaptured = false;
                    note.IsNoteEnabled = true;
                }
            }
        }

        // Debug move forward
        if (Input.GetKeyDown(KeyCode.Keypad8))
            transform.Translate(Vector3.forward * 62.9f);
        else if (Input.GetKeyDown(KeyCode.Keypad9))
            foreach (AudioSource src in AmplitudeSongController.Instance.audiosrcList)
                src.time += 2f;

        // Timescale
        if (Input.GetKeyDown(KeyCode.Keypad8) & Input.GetKey(KeyCode.LeftControl)) // up
        {
            if (Time.timeScale < 1f)
                SetTimescale(Time.timeScale + 0.1f);
            else
                SetTimescale(1f);
        }
        if (Input.GetKeyDown(KeyCode.Keypad2) & Input.GetKey(KeyCode.LeftControl)) // down
            if (Time.timeScale > 0.1f)
                SetTimescale(Time.timeScale - 0.1f);
        if (Input.GetKeyDown(KeyCode.Keypad1)) // one
            SetTimescale(1f);
        if (Input.GetKeyDown(KeyCode.Keypad0)) // progressive slowmo test (tut)
            DoFailTest();

        // If the game is not Rhythmic, ignore everything below
        if (RhythmicGame.GameType != RhythmicGame._GameType.RHYTHMIC)
            return;
    }

    public virtual void BeginPlay() { }
    public void Restart() { SceneManager.LoadScene("Loading", LoadSceneMode.Single); }

    public async void DoFailTest()
    {
        int msCounter = 50;

        for (float i = 1f; i > 0f; i -= 0.1f)
        {
            await Task.Delay(msCounter);
            SetTimescale(i);
            msCounter -= 5;
        }
    }
    void SetTimescale(float speed)
    {
        Time.timeScale = speed;
        foreach (AudioSource src in AmplitudeSongController.Instance.audiosrcList)
            src.pitch = speed;
    }
}
