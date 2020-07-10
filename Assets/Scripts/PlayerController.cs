using Assets.Scripts.Amplitude;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public AmplitudeSongController amp_ctrl { get { return GameObject.Find("AMPController").GetComponent<AmplitudeSongController>(); } }
    public TracksController TracksController;
    public AmplitudeTracksController AMP_TracksController { get; set; }
    List<Track> trackList;
    Transform catcherTransform;
    AudioSource src;

    public AudioClip streak_lose;
    public AudioClip catcher_empty;
    public AudioClip catcher_miss;

    public GameObject MissText;
    public GameObject ScoreText;

    /// <summary>
    /// The offset for starting, in "seconds"
    /// Amplitude usually uses 4, sometimes 5
    /// </summary>
    public float StartPosOffset = 4;

    /// <summary>
    /// Player Z position offset
    /// </summary>
    public float PosOffset = 0f;

    /// <summary>
    /// the Y position (elevation) of the camera
    /// </summary>
    public float YPosition = 2.05f;

    /// <summary>
    /// The track to start with.
    /// </summary>
    public Track StartTrack;

    bool IsTrackActive = false; // TODO: temp
    async void Start()
    {
        // Get tracks controller
        if (TracksController == null) // if the script is not assigned
        {
            var controller = GameObject.Find("TRACKS");
            if (RhythmicGame.GameType == RhythmicGame._GameType.AMPLITUDE)
                TracksController = controller.GetComponent<AmplitudeTracksController>();
            else
                TracksController = controller.GetComponent<TracksController>();
        }

        if (RhythmicGame.GameType == RhythmicGame._GameType.AMPLITUDE)
            AMP_TracksController = (AmplitudeTracksController)TracksController;

        trackList = TracksController.trackList;

        //float startTrackXPos = 0f;
        // Get starting track
        if (StartTrack == null)
            StartTrack = TracksController.GetDefaultTrack();

        //StartPosOffset -= (60f / 110f);

        // Position player!
        //transform.position = new Vector3(0, YPosition, -StartPosOffset);

        // Push player backwards instead so that we can set initial player position in editor
        // TODO: automate this so it isn't predetermined!
        transform.Translate(Vector3.back * StartPosOffset); // negative

        currentTrack = StartTrack;
        SwitchToTrack(StartTrack);

        // Create catchers!
        CreateCatchers();

        //catcherTransform = GameObject.Find("CATCHER").transform;

        Debug.LogFormat("PLAYER: created");

        src = GetComponent<AudioSource>();
        catcher_empty = (AudioClip)Resources.Load("Sounds/catcher_empty");
        catcher_miss = (AudioClip)Resources.Load("Sounds/catcher_miss");
        streak_lose = (AudioClip)Resources.Load("Sounds/streak_lose");

        // TODO: detect whether a track is active - this is TEMP for now
        await System.Threading.Tasks.Task.Delay(10000);
        IsTrackActive = true;
    }

    public Catcher[] catchers = new Catcher[3];
    void CreateCatchers()
    {
        for (int i = 0; i < 3; i++) // create 3 catchers!
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //obj.transform.parent = gameObject.transform;

            obj.GetComponent<MeshRenderer>().material = (Material)Resources.Load("Materials/CatcherMaterial", typeof(Material));
            obj.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Destroy(obj.GetComponent<BoxCollider>());

            float xPos = TrackLane.GetLocalXPosFromLaneType((TrackLane.LaneType)i);

            obj.name = string.Format("CATCHER_{0}", (TrackLane.LaneType)i);
            obj.transform.localScale = new Vector3(0.4f, 0.01f, 0.4f);
            obj.transform.parent = gameObject.transform;
            obj.transform.localPosition = new Vector3(xPos, -1.95f, 2.5f);

            Catcher catcher = obj.AddComponent<Catcher>();
            catcher.OnCatch += Catcher_OnCatch;

            catchers[i] = catcher;
        }
    }

    public int[] TrackCatchCounter = new int[6];
    public Note ExpectedNote { get; set; }
    public int Score;
    private void Catcher_OnCatch(object sender, Catcher.CatchEventResult e)
    {
        // On Success / Powerup:
        switch (e.catchresult)
        {
            default:
                break;

            case Catcher.CatchResult.Success: // if successfully caught a Note
            case Catcher.CatchResult.Powerup:
            {
                Score++;
                ScoreText.GetComponent<TextMeshProUGUI>().text = Score.ToString();

                int notetrackID = e.note.noteTrack.ID.Value;
                TrackCatchCounter[notetrackID] += 1;

                break;
            }
            case Catcher.CatchResult.Failure: // if we pressed the wrong button
            {
                src.PlayOneShot(catcher_miss);
                DeclareNoteMiss(e.note, Catcher.NoteMissType.Mispress);

                break;
            }
            case Catcher.CatchResult.Empty: // if we pressed on an empty space
            {
                src.PlayOneShot(currentTrack.IsTrackActive ? catcher_miss : catcher_empty);
                if (currentTrack.IsTrackFocused & currentTrack.IsTrackActive) // if a track is focused / being played, declare miss
                    DeclareNoteMiss(e.note, Catcher.NoteMissType.EmptyMispress);

                break;
            }
            case Catcher.CatchResult.Unknown:
                break;
        }
    }

    public bool shouldReactToNoteMiss { get; set; } = false;
    public async void DeclareNoteMiss(Note note = null, Catcher.NoteMissType? misstype = null)
    {
        if (!shouldReactToNoteMiss)
            return;

        src.PlayOneShot(streak_lose);
        shouldReactToNoteMiss = false;

        if (note != null)
        {
            int notetrackID = note.noteTrack.ID.Value;
            TrackCatchCounter[notetrackID] = 0;
        }

        MissText.SetActive(true);
        await Task.Delay(3000);
        MissText.SetActive(false);
    }

    public Track currentTrack;
    public void SwitchToTrack(Track targetTrack)
    {
        // position the player
        PositionPlayer(targetTrack.gameObject.transform.position.x, targetTrack.gameObject.transform.position.y);

        if (RhythmicGame.GameType == RhythmicGame._GameType.AMPLITUDE)
        {
            AmplitudeTracksController ctrl = (AmplitudeTracksController)TracksController;
            ctrl.UpdateTracksVolume(targetTrack);
            ctrl.ActiveTrack = targetTrack;

            TrackCatchCounter[currentTrack.ID.Value] = 0;


            currentTrack.gameObject.GetComponent<TrackEdgeLightController>().IsTrackFocused = false;
            currentTrack.IsTrackFocused = false;
            currentTrack = targetTrack;
            currentTrack.gameObject.GetComponent<TrackEdgeLightController>().IsTrackFocused = true;
            targetTrack.IsTrackFocused = true;
        }
    }
    public void SwitchToTrack(int id)
    {
        if (id > trackList.Count - 1)
            Debug.LogWarning("PLAYER/SwitchToTrack(): trying to go above track count!");

        if (id < 0)
            Debug.LogWarning("PLAYER/SwitchToTrack(): trying to go below track count!");

        SwitchToTrack(trackList[id]);
    }

    public void PositionPlayer(float xPos, float yPos)
    {
        transform.position = new Vector3(xPos, yPos + YPosition, transform.position.z);
    }

    // Game loop

    // TODO: mouse flick controls
    // TODO: major code cleanup / improvements / explanations

    KeyCode[] keycodes = new KeyCode[] { KeyCode.LeftArrow, KeyCode.UpArrow, KeyCode.RightArrow };
    bool m_pressingButton = false;
    KeyCode pressedKey = KeyCode.None;
    public bool IsPlayerMoving = false;
    public float movementSpeed = 4f;
    //bool mouseFlicked = false;
    //public float FlickSensitivity = 10f;

    float prevBeatPos = 8;
    bool twomeasures = false;
    void Update()
    {
        // PLAYER MOVEMENT
        if (IsPlayerMoving)
        {
            float zPos = ((amp_ctrl.songPosition * movementSpeed) + (AMP_TracksController.bpm / 60f)) * (1f + AMP_TracksController.fudgeFactor);
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, zPos - (PosOffset) - (StartPosOffset));

            if (RhythmicGame.DebugPlayerMovementEvents)
                Debug.LogFormat("songPosition: {0} | songPositionInBeats: {1}", amp_ctrl.songPosition, amp_ctrl.songPositionInBeats);

            /* EXPERIMENTAL METRONOME CODE
            if (Time.time >= nextTime)
            {
                nextTime += interval;
                //Debug.LogFormat("Time: {0} | Position: {1}", nextTime, gameObject.transform.position.z);
                //src.PlayOneShot(catcher_empty);
            }
            */
        }

        // MEASURE CHECKING
        if (IsPlayerMoving)
        {
            if (float.Parse(amp_ctrl.songPositionInBeats.ToString().Substring(0, amp_ctrl.songPositionInBeats.ToString().IndexOf(','))) - prevBeatPos == 0 & IsPlayerMoving)
            {
                prevBeatPos = prevBeatPos + 4;
                twomeasures = true;
            }
            else
                twomeasures = false;

            Debug.LogFormat("prevBeatPos: {0} | twomeasures: {1} | songPosInBeats (int): {2} TrackCatchCounter: {3}", prevBeatPos, twomeasures, amp_ctrl.songPositionInBeats, TrackCatchCounter[currentTrack.ID.Value]);

            if (TrackCatchCounter[currentTrack.ID.Value] == 0)
                twomeasures = false;

            if (TrackCatchCounter[currentTrack.ID.Value] != 0 & twomeasures & currentTrack.IsTrackActive)
            {
                var track = (AmplitudeTrack)currentTrack;
                track.DisableForMeasures(2);
                TrackCatchCounter[currentTrack.ID.Value] = 0;
            }

            twomeasures = false;
        }


        // TEMP START MOVEMENT
        if (Input.GetKeyDown(KeyCode.Return) & !IsPlayerMoving)
        {
            Destroy(GameObject.Find("Intro Text"));
            amp_ctrl.PlayMusic();
            IsPlayerMoving = true;
        }


        // INPUT
        #region CATCHER
        foreach (KeyCode key in keycodes)
        {
            if (Input.GetKeyDown(key))
            {
                if (pressedKey == key)
                {
                    m_pressingButton = true;
                    continue;
                }
                else
                {
                    pressedKey = key;
                    m_pressingButton = false;
                    break;
                }
            }
        }

        if (!m_pressingButton & Input.GetKey(pressedKey))
        {
            Catcher targetCatcher = catchers[(int)Catcher.KeyCodeToLane(pressedKey)];
            targetCatcher.PerformCatch(pressedKey);
            m_pressingButton = true;

        }
        if (!Input.GetKey(pressedKey))
            pressedKey = KeyCode.None;
        #endregion

        #region TRACK SWITCHING
        /* EXPERIMENTAL MOUSE FLICK CODE
        float mouseX = Input.GetAxis("Mouse X");
        if (!mouseFlicked)
        {
            if (mouseX < -FlickSensitivity) // negative value is to the left
                SwitchToTrack(CurrentTrack.ID.Value - 1);
            else if (mouseX > FlickSensitivity)
                SwitchToTrack(CurrentTrack.ID.Value + 1);

            mouseFlicked = true;
        }

        if (mouseX == 0f & mouseFlicked)
            mouseFlicked = false;
        else
            mouseFlicked = true;

        Debug.Log(mouseX + " | " + mouseFlicked);
        */

        if (Input.GetKeyUp(KeyCode.A))
            SwitchToTrack(currentTrack.ID.Value - 1);
        if (Input.GetKeyUp(KeyCode.D))
            SwitchToTrack(currentTrack.ID.Value + 1);
        #endregion
    }

    // TEMP TIMESCALE CODE
    private void LateUpdate()
    {
        if (Input.GetKey(KeyCode.Keypad2))
        {
            if (Time.timeScale > 0.1f)
            {
                Time.timeScale -= 0.1f;
                foreach (AudioSource src in amp_ctrl.GetComponents<AudioSource>())
                {
                    src.pitch -= 0.1f;
                }
            }
        }

        if (Input.GetKey(KeyCode.Keypad8))
        {
            if (Time.timeScale < 5f)
            {
                Time.timeScale += 0.1f;
                foreach (AudioSource src in amp_ctrl.GetComponents<AudioSource>())
                {
                    src.pitch += 0.1f;
                }
            }
        }

        if (Input.GetKey(KeyCode.Keypad5))
        {
            Time.timeScale = 1f;

            foreach (AudioSource src in amp_ctrl.GetComponents<AudioSource>())
            {
                src.pitch = 1f;
            }
        }
    }
}