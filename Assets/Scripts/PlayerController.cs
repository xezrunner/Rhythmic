using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    // TODO: BPM calculations for offset!
    /// <summary>
    /// the offset for starting, in "seconds"
    /// Amplitude usually uses 4, sometimes 5
    /// </summary>
    public float StartPosOffset = 4;

    /// <summary>
    /// the Y position of the camera
    /// </summary>
    public float YPosition = 2.05f;

    /// <summary>
    /// The track to start with.
    /// </summary>
    public Track StartTrack;
    public bool IsPlayerMoving = false;
    public float movementSpeed = 5f;
    public TracksController TracksController;
    List<Track> trackList;
    Catcher[] catchers = new Catcher[3];

    Transform catcherTransform;
    AudioSource src;

    bool IsTrackActive = false;

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

        trackList = TracksController.trackList;

        //float startTrackXPos = 0f;
        // Get starting track
        if (StartTrack == null)
            StartTrack = TracksController.GetDefaultTrack();

        StartPosOffset -= (60f / 110f);

        // Position player!
        transform.position = new Vector3(0, YPosition, -StartPosOffset);
        SwitchToTrack(StartTrack);

        // Create catchers!
        CreateCatchers();

        //catcherTransform = GameObject.Find("CATCHER").transform;

        Debug.LogFormat("PLAYER: created");

        src = GetComponent<AudioSource>();
        catcher_empty = (AudioClip)Resources.Load("Sounds/catcher_empty");
        catcher_miss = (AudioClip)Resources.Load("Sounds/catcher_miss");

        // TODO: detect whether a track is active - this is TEMP for now
        await System.Threading.Tasks.Task.Delay(10000);

        IsTrackActive = true;

        var amp_trackctrl = (AmplitudeTracksController)TracksController;
        src = amp_trackctrl.src_bgclick;
    }

    void CreateCatchers()
    {
        for (int i = 0; i < 3; i++) // create 3 catchers!
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.transform.parent = gameObject.transform;

            obj.GetComponent<MeshRenderer>().material = (Material)Resources.Load("Materials/CatcherMaterial", typeof(Material));
            Destroy(obj.GetComponent<BoxCollider>());

            float xPos = TrackLane.GetLocalXPosFromLaneType((TrackLane.LaneType)i);

            obj.name = string.Format("CATCHER_{0}", (TrackLane.LaneType)i);
            obj.transform.localPosition = new Vector3(xPos, -1.4f, 2.5f);
            obj.transform.localScale = new Vector3(1f, 0.01f, 1f);

            obj.AddComponent<Catcher>();

            catchers[i] = obj.GetComponent<Catcher>();
        }
    }

    public Track CurrentTrack;
    public void SwitchToTrack(Track track)
    {
        // get the position of the track
        float xPos = track.gameObject.transform.position.x; // get the track position on the X axis

        // position the player
        PositionPlayerOnX(xPos);

        CurrentTrack = track;
        if (RhythmicGame.GameType == RhythmicGame._GameType.AMPLITUDE)
        {
            AmplitudeTracksController ctrl = (AmplitudeTracksController)TracksController;
            ctrl.UpdateTracksVolume(track);
        }
    }
    public void SwitchToTrack(int id)
    {
        if (id > trackList.Count - 1)
            throw new Exception("PLAYER/SwitchToTrack(): exceeded track count!");

        if (id < 0)
            throw new Exception("PLAYER/SwitchToTrack(): trying to go below track count!");

        SwitchToTrack(trackList[id]);
    }

    public void PositionPlayerOnX(float xPos)
    {
        transform.position = new Vector3(xPos, transform.position.y, transform.position.z);
    }

    // TODO: mouse flick controls
    /*
    bool mouseFlicked = false;
    public float FlickSensitivity = 10f;
    */
    KeyCode[] keycodes = new KeyCode[] { KeyCode.LeftArrow, KeyCode.UpArrow, KeyCode.RightArrow };
    bool m_pressingButton = false;
    KeyCode pressedKey = KeyCode.None;
    public AudioClip catcher_empty;
    public AudioClip catcher_miss;

    void Update()
    {
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

        // 
        if (!m_pressingButton & Input.GetKey(pressedKey))
        {
            /*
            Catcher catcher = catchers[(int)(KeyCodeToLane(pressedKey))];
            catcher.PerformCatch(pressedKey);
            */

            bool successfulCatch = false;
            foreach (Catcher catcher in catchers)
            {
                if (catcher.PerformCatch(pressedKey))
                {
                    successfulCatch = true;
                    break;
                }
            }

            if (!successfulCatch)
                src.PlayOneShot(IsTrackActive ? catcher_miss : catcher_empty);

            m_pressingButton = true;
        }

        if (!Input.GetKey(pressedKey))
        {
            //m_pressingButton = false;
            pressedKey = KeyCode.None;
        }

        /*
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
            SwitchToTrack(CurrentTrack.ID.Value - 1);
        if (Input.GetKeyUp(KeyCode.D))
            SwitchToTrack(CurrentTrack.ID.Value + 1);

        //if (Input.GetKeyUp(KeyCode.DownArrow))
        //    transform.position += Vector3.back;
    }

    private void LateUpdate()
    {

    }
    private void FixedUpdate()
    {
        if (IsPlayerMoving)
        {
            Vector3 movement = Vector3.forward * Time.deltaTime * movementSpeed; //* (110f / 60f)
            gameObject.transform.Translate(movement);
        }

        if (Input.GetKey(KeyCode.Keypad2))
        {
            if (Time.timeScale > 0.1f)
            {
                Time.timeScale -= 0.1f;
                foreach (AudioSource src in GameObject.Find("TRACKS").GetComponents<AudioSource>())
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
                foreach (AudioSource src in GameObject.Find("TRACKS").GetComponents<AudioSource>())
                {
                    src.pitch += 0.1f;
                }
            }
        }

        if (Input.GetKey(KeyCode.Keypad5))
        {
            Time.timeScale = 1f;

            foreach (AudioSource src in GameObject.Find("TRACKS").GetComponents<AudioSource>())
            {
                src.pitch = 1f;
            }
        }
    }

    KeyCode LaneToKeyCode(TrackLane.LaneType lane)
    {
        switch (lane)
        {
            case TrackLane.LaneType.Left:
                return KeyCode.LeftArrow;
            case TrackLane.LaneType.Center:
                return KeyCode.UpArrow;
            case TrackLane.LaneType.Right:
                return KeyCode.RightArrow;

            default:
                return KeyCode.None;
        }
    }

    TrackLane.LaneType KeyCodeToLane(KeyCode key)
    {
        switch (key)
        {
            default:
                return TrackLane.LaneType.Center;

            case KeyCode.LeftArrow:
                return TrackLane.LaneType.Left;
            case KeyCode.UpArrow:
                return TrackLane.LaneType.Center;
            case KeyCode.RightArrow:
                return TrackLane.LaneType.Right;
        }
    }
}