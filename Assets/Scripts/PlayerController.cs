using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    public TracksController TracksController { get { return TracksController.Instance; } } // might not be needed?
    public CatcherController CatcherController { get { return CatcherController.Instance; } }

    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI MissText;

    public TextMeshProUGUI SubbeatCounterText;
    public TextMeshProUGUI MeasureCounterText;
    public TextMeshProUGUI NextNoteText;

    // Props
    //public float zPos { get { return transform.position.z; } }

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

        // Wire up catcher events
        CatcherController.OnCatch += CatcherController_OnCatch;

        if (!AmplitudeSongController.Instance.Enabled)
            return;

        while (TracksController == null || TracksController.Tracks[0].trackMeasures.Count < 3)
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
    public async void LoseStreak()
    {
        // find next notes again
        CatcherController.IsSuccessfullyCatching = false;
        CatcherController.FindNextMeasuresNotes();

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
    public event EventHandler<int> OnTrackSwitched;
    public void SwitchToTrack(int id, bool force = false)
    {
        //if (id == TracksController.CurrentTrackID) { Debug.Log("PLAYER/SwitchToTrack(): Already on this track!"); return; }

        if (TracksController.Tracks.Count < 1)
        {
            Debug.LogWarning("PLAYER/SwitchToTrack(): No tracks are available");
            return;
        }

        if (id > TracksController.Tracks.Count - 1) { Debug.LogWarningFormat("PLAYER/SwitchToTrack(): Trying to switch to non-existent track {0} / {1}", id, TracksController.Tracks.Count - 1); return; }
        else if (id < 0) { Debug.LogWarningFormat("PLAYER/SwitchToTrack(): Trying to switch to non-existent track {0} / {1}", id, 0); return; }

        // find the track by ID
        Track track = null;

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
        // TODO: animate the player model to the position
        // TODO: improve this
        Vector3 pos = new Vector3(track.transform.position.x,
            transform.position.y, transform.position.z);

        transform.position = pos;

        // let stuff know of the switch
        OnTrackSwitched?.Invoke(null, track.ID.Value);
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
        if (Input.GetKeyDown(InputManager.Player.TrackSwitching[0]))
            SwitchToTrack(TracksController.CurrentTrackID - 1, Input.GetKey(KeyCode.LeftShift));
        else if (Input.GetKeyDown(InputManager.Player.TrackSwitching[1]))
            SwitchToTrack(TracksController.CurrentTrackID + 1, Input.GetKey(KeyCode.LeftShift));
        /*
        if (Input.GetKeyDown(KeyCode.A))
            SwitchToTrack(TracksController.CurrentTrackID - 1, Input.GetKey(KeyCode.LeftShift));
        else if (Input.GetKeyDown(KeyCode.D))
            SwitchToTrack(TracksController.CurrentTrackID + 1, Input.GetKey(KeyCode.LeftShift));
        */

        // RESTART
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene("Loading", LoadSceneMode.Single);

        // TEMP / DEBUG
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

    async void DoFailTest()
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
