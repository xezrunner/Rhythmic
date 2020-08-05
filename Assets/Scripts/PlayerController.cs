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

        await Task.Delay(300);

        // TODO: we should switch to the track when the game starts
        SwitchToTrack(0);

        TracksController.MeasureCaptureFinished += TracksController_MeasureCaptureFinished;

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

                e.note.noteTrack.IsTrackBeingCaptured = true;

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
                if (TracksController.GetIsCurrentMeasureActive)
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
            {
                // Disable all notes in the note's measures
                note.noteTrack.IsTrackBeingCaptured = false;
                TracksController.DisableCurrentMeasures();

                break;
            }
            case Catcher.NoteMissType.Mispress:
            {
                TracksController.CurrentTrack.IsTrackBeingCaptured = false;
                TracksController.DisableCurrentMeasures();

                break;
            }
            case Catcher.NoteMissType.Ignore:
            {
                note.noteTrack.IsTrackBeingCaptured = false;
                TracksController.DisableCurrentMeasures();

                break;
            }

        }

        if (misstype != Catcher.NoteMissType.EmptyMispress)
            LoseStreak();
    }

    int streakCounter = 0;

    public async void LoseStreak()
    {
        // find next notes again
        //CatcherController.ShouldHit.Clear();
        CatcherController.ShouldHit = null;
        CatcherController.FindNextMeasureNotes();

        if (streakCounter < 1)
            return;

        streakCounter = 0;

        SetScore(0);

        src.PlayOneShot(streak_lose);

        MissText.gameObject.SetActive(true);
        await Task.Delay(1500);
        MissText.gameObject.SetActive(false);
    }

    // Track switching
    public event EventHandler<int> OnTrackSwitched;
    public void SwitchToTrack(int id)
    {
        if (TracksController.Tracks.Count < 1)
        {
            Debug.LogWarning("PLAYER/SwitchToTrack(): No tracks are available");
            return;
        }

        // find the track by ID
        Track track;
        try
        {
            track = TracksController.GetTrackByID(id);
        }
        catch
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
        OnTrackSwitched?.Invoke(null, id);
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
    public void Update()
    {
        // TRACK SWITCHING
        if (Input.GetKeyDown(KeyCode.A))
            SwitchToTrack(TracksController.CurrentTrackID - 1);
        else if (Input.GetKeyDown(KeyCode.D))
            SwitchToTrack(TracksController.CurrentTrackID + 1);

        // RESTART
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.UnloadSceneAsync("DevScene", UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
            SceneManager.LoadSceneAsync("Loading", LoadSceneMode.Single);
        }

        // TEMP / DEBUG
        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            foreach (Note note in TracksController.CurrentTrack.trackNotes)
                note.CaptureNote();

            foreach (Measure measure in TracksController.CurrentTrack.trackActiveMeasures)
                measure.IsMeasureActive = false;
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            //TracksController.CurrentTrack.BeginCapture(4, 10);
            TracksController.CurrentMeasure.CaptureMeasure();
        }

        if (Input.GetKeyDown(KeyCode.Keypad8))
            transform.Translate(Vector3.forward * 60);

        // If the game is Rhythmic
        if (RhythmicGame.GameType != RhythmicGame._GameType.RHYTHMIC)
            return;
    }

    int counter = 0;
    private void TracksController_MeasureCaptureFinished(object sender, int e)
    {
        if (counter < 7)
        {
            TracksController.CurrentTrack.trackMeasures[e + 1].CaptureMeasure();
            counter++;
        }
        else
            counter = 0;
    }
}
