using System;
using System.Collections;
using System.Threading.Tasks;
using System.Transactions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    public AmplitudeTracksController TracksController { get { return (AmplitudeTracksController)AmplitudeTracksController.Instance; } } // might not be needed?
    public CatcherController CatcherController { get { return CatcherController.Instance; } }

    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI MissText;

    public TextMeshProUGUI SubbeatCounterText;
    public TextMeshProUGUI MeasureCounterText;
    public TextMeshProUGUI NextNoteText;

    public Camera PlayerCamera;
    public Transform PlayerCameraTransform;
    public Transform CameraContainer;

    public Animation trackswitch_anim;

    // Props
    //public float zPos { get { return transform.position.z; } }

    public float CameraPullbackOffset = -5.5f;
    public float StartZOffset = 4f; // countin
    public float ZOffset = 0f; // (DEBUG) additional position offset

    public bool IsPlayerMoving = false;
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
    public async void Start()
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

    // Track switching

    public bool IsTrackSwitching = false;
    public float TrackSwitch_Progress = 0f;
    int TrackSwitch_PrevTrackID;
    Vector3 TrackSwitch_PrevCam;
    Vector3 TrackSwitch_TargetCam;
    public IEnumerator TrackSwitchAnim(Vector3 target)
    {
        TrackSwitch_Progress = 0f; // set anim progress to 0

        if (!RhythmicGame.IsTunnelMode) // store prev camera position
            TrackSwitch_PrevCam = new Vector3(-(target.x - CameraContainer.position.x),
                CameraContainer.localPosition.y, CameraContainer.localPosition.z);
        else
            TrackSwitch_PrevCam = new Vector3(CameraContainer.localEulerAngles.x, CameraContainer.localEulerAngles.y,
                -(target.z - CameraContainer.eulerAngles.z));

        // move rest of the player immediately
        if (!RhythmicGame.IsTunnelMode)
            transform.position = target;
        else
            transform.eulerAngles = target;

        // move back player camera to previous pos
        if (!RhythmicGame.IsTunnelMode)
            CameraContainer.localPosition = TrackSwitch_PrevCam;
        else // inverse rotations
        {
            if (TrackSwitch_PrevTrackID == TracksController.Tracks.Count - 1)
                TrackSwitch_TargetCam = new Vector3(0, 0, 360);
            else if (TrackSwitch_PrevTrackID == 0 & TracksController.CurrentTrackID == TracksController.Tracks.Count - 1)
            {
                TrackSwitch_PrevCam = new Vector3(0, 0, TracksController.rotZ);
                TrackSwitch_TargetCam = new Vector3(0, 0, 0);
            }
            else
                TrackSwitch_TargetCam = Vector3.zero;
            CameraContainer.localEulerAngles = TrackSwitch_PrevCam;
        }

        // play track switching animation
        IsTrackSwitching = true;
        trackswitch_anim.Stop();
        trackswitch_anim.Play();

        while (TrackSwitch_Progress < 1f)
            yield return null;

        IsTrackSwitching = false;
    }

    // This moves the camera according to the track switching animation!
    void TrackSwitchUpdate()
    {
        Vector3 prevpos = TrackSwitch_PrevCam;
        Vector3 targetpos = TrackSwitch_TargetCam; // Target is usually 0 but gets overriden to 360 when doing an inverse rotation

        Vector3 final = Vector3.Lerp(prevpos, targetpos, TrackSwitch_Progress);

        if (!RhythmicGame.IsTunnelMode)
            CameraContainer.transform.localPosition = final;
        else
            CameraContainer.transform.localEulerAngles = final;
    }

    public event EventHandler<int> OnTrackSwitched;

    public void SwitchToTrack(int id, bool force = false)
    {
        if (TracksController.Tracks.Count < 1)
        {
            Debug.LogWarning("PLAYER/SwitchToTrack(): No tracks are available");
            return;
        }

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

        // find the track by ID
        Track track;
        if (RhythmicGame.TrackSeekEmpty & TracksController.CurrentMeasure.measureNum >= 4 & !force) // seek
        {
            if (TracksController.Tracks[id].trackMeasures[TracksController.CurrentMeasure.measureNum + 1].IsMeasureEmpty)
            {
                SwitchToTrack(id > TracksController.CurrentTrackID ? id + 1 : id - 1);
                return;
            }
            else
                track = TracksController.GetTrackByID(id);
        }
        else
            track = TracksController.GetTrackByID(id);

        if (track == null)
        {
            Debug.LogErrorFormat("PLAYER/SwitchToTrack(): Could not switch to track {0}", id);
            return;
        }

        // position the player

        // set target pos / rot
        Vector3 target;
        if (!RhythmicGame.IsTunnelMode)
            target = new Vector3(track.transform.position.x, transform.position.y, transform.position.z);
        else
            target = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, id * TracksController.rotZ);

        TrackSwitch_PrevTrackID = TracksController.CurrentTrackID;

        // let stuff know of the switch
        OnTrackSwitched?.Invoke(null, track.ID.Value);

        // start animation
        StartCoroutine(TrackSwitchAnim(target));

        /* OLD IMMEDIATE (NON-ANIM) SWITCHING CODE
        if (!RhythmicGame.IsTunnelMode)
        {
            Vector3 pos = new Vector3(track.transform.position.x,
            transform.position.y, transform.position.z);

            transform.position = pos;
        }
        else
            transform.localEulerAngles = new Vector3(0, 0, id * TracksController.rotZ);
        */
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
    public async void Update()
    {
        // TRACK SWITCHING

        if (IsTrackSwitching)
            TrackSwitchUpdate();

        if (Input.GetKeyDown(InputManager.Player.TrackSwitching[0]))
            SwitchToTrack(TracksController.CurrentTrackID - 1, Input.GetKey(KeyCode.LeftShift));
        else if (Input.GetKeyDown(InputManager.Player.TrackSwitching[1]))
            SwitchToTrack(TracksController.CurrentTrackID + 1, Input.GetKey(KeyCode.LeftShift));

        // RESTART
        if (Input.GetKeyDown(KeyCode.R))
            Restart();

        // TEMP / DEBUG
        if (Input.GetKeyDown(KeyCode.F))
        {
            RhythmicGame.IsTunnelMode = !RhythmicGame.IsTunnelMode;
            ScoreText.text = string.Format("Tunnel {0}\nRestart!", RhythmicGame.IsTunnelMode ? "ON" : "OFF");
        }

        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            foreach (Track track in TracksController.Tracks)
                track.CaptureMeasuresRange(GetCurrentMeasure().measureNum, 5);
        }

        if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            foreach (Track track in TracksController.Tracks)
                track.CaptureMeasures(GetCurrentMeasure().measureNum, track.trackMeasures.Count - 1);
        }

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

        if (Input.GetKeyDown(KeyCode.H))
        {
            TracksController.CurrentTrack.CaptureMeasuresRange(GetCurrentMeasure().measureNum, 5);
        }

        if (Input.GetKeyDown(KeyCode.Keypad8))
            transform.Translate(Vector3.forward * 63.2f);

        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            if (Time.timeScale > 0.1f)
                Time.timeScale -= 0.1f;
            foreach (AudioSource src in AmplitudeSongController.Instance.audiosrcList)
                src.pitch -= 0.1f;
        }

        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            Time.timeScale = 1f;
            foreach (AudioSource src in AmplitudeSongController.Instance.audiosrcList)
                src.pitch = 1f;
        }

        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            DoFailTest();
        }

        if (Input.GetKeyDown(KeyCode.Keypad9))
        {
            foreach (AudioSource src in AmplitudeSongController.Instance.audiosrcList)
                src.time += 2f;
        }

        // If the game is Rhythmic
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
            SetSpeed(i);
            msCounter -= 5;
        }
    }
    void SetSpeed(float speed)
    {
        Time.timeScale = speed;
        foreach (AudioSource src in AmplitudeSongController.Instance.audiosrcList)
            src.pitch = speed;
    }
}
