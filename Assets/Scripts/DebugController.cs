using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DebugController : MonoBehaviour
{
    Player Player { get { return Player.Instance; } }
    AmplitudeSongController amp_ctrl { get { return AmplitudeSongController.Instance; } }
    TracksController TracksController { get { return TracksController.Instance; } }

    public GameObject section_debug;
    public GameObject section_controllerinput;
    public TextMeshProUGUI inputlagText;
    public TextMeshProUGUI framerateText;

    private void Start()
    {
        inputlagText.text = string.Format("Player offset (zPos): {0}", Player.ZOffset);
    }

    public bool isDebugOn = true;

    public void AMP_ChangeSong(string value)
    {
        AmplitudeSongController.songName = value;
        Player.ScoreText.text = string.Format("Song changed: {0} - restart!", value);
    }

    int android_songcounter = 0;
    private void LateUpdate()
    {
        // AMP songs debug
        if (Input.GetKeyDown(KeyCode.Alpha0))
            AMP_ChangeSong("tut0");
        else if (Input.GetKey(KeyCode.Alpha1))
            AMP_ChangeSong("perfectbrain");
        else if (Input.GetKey(KeyCode.Alpha2))
            AMP_ChangeSong("dreamer");
        else if (Input.GetKey(KeyCode.Alpha3))
            AMP_ChangeSong("dalatecht");

        if (Touchscreen.current != null)
        {
            if (Touchscreen.current.press.wasPressedThisFrame)
            {
                if (android_songcounter == 4) android_songcounter = 0;
                switch (android_songcounter)
                {
                    case 0:
                        AMP_ChangeSong("tut0"); break;
                    case 1:
                        AMP_ChangeSong("perfectbrain"); break;
                    case 2:
                        AMP_ChangeSong("dreamer"); break;
                    case 3:
                        AMP_ChangeSong("dalatecht"); break;
                }
                android_songcounter++;
            }
        }

        if (Gamepad.current != null && Gamepad.current.dpad.down.wasPressedThisFrame)
        {
            var world = GameObject.Find("WORLD_TUT");
            var worldCamera = GameObject.Find("MainCamera");

            world.SetActive(!world.activeInHierarchy); worldCamera.SetActive(!world.activeInHierarchy);
        }

        // RESTART
        if (Input.GetKeyDown(KeyCode.R))
            RhythmicGame.Restart();
        if (Input.GetKeyDown(KeyCode.Escape))
            Player.TogglePause();

        // TEMP / DEBUG

        // Resolution
        if (Input.GetKeyDown(KeyCode.F11) & Input.GetKey(KeyCode.LeftControl))
        {
            RhythmicGame.PreferredResolution = new Vector2(1280, 720);
            RhythmicGame.SetResolution(RhythmicGame.PreferredResolution);
        }

        // FPS Lock
        if (Input.GetKeyDown(KeyCode.F1)) { RhythmicGame.SetFramerate(60); Player.ScoreText.text = "60 FPS"; }
        else if (Input.GetKeyDown(KeyCode.F2)) { RhythmicGame.SetFramerate(144); Player.ScoreText.text = "144 FPS"; }
        else if (Input.GetKeyDown(KeyCode.F3)) { RhythmicGame.SetFramerate(200); Player.ScoreText.text = "200 FPS"; }

        // Toggle tunnel mode
        if (Input.GetKeyDown(KeyCode.F))
        {
            RhythmicGame.IsTunnelMode = !RhythmicGame.IsTunnelMode;
            Player.ScoreText.text = string.Format("Tunnel {0} - restart!", RhythmicGame.IsTunnelMode ? "ON" : "OFF");
        }

        // Lag compensation
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            Player.ZOffset += 0.1f;
            Player.transform.Translate(Vector3.forward * (Player.PlayerSpeed * amp_ctrl.secPerBeat / amp_ctrl.TunnelSpeedAccountation) * 0.1f); // offset the player as well!
            inputlagText.text = string.Format("Player offset (zPos): {0}", Player.ZOffset);
        }
        else if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            Player.ZOffset -= 0.1f;
            Player.transform.Translate(Vector3.back * (Player.PlayerSpeed * amp_ctrl.secPerBeat / amp_ctrl.TunnelSpeedAccountation) * 0.1f); // offset the player as well!
            inputlagText.text = string.Format("Player offset (zPos): {0}", Player.ZOffset);
        }

        // Track capturing debug
        if (Input.GetKeyDown(KeyCode.H)) // current track, 5
            Player.DeployPowerup();

        else if (Input.GetKeyDown(KeyCode.Keypad5)) // 5
            foreach (Track track in TracksController.CurrentTrackSet)
                track.CaptureMeasuresRange(Player.GetCurrentMeasure().measureNum, 5);

        else if (Input.GetKeyDown(KeyCode.Keypad6)) // all!
            foreach (Track track in TracksController.CurrentTrackSet)
                track.CaptureMeasures(Player.GetCurrentMeasure().measureNum, track.trackMeasures.Count - 1);

        // Track restoration (buggy!)
        if (Input.GetKeyDown(KeyCode.Keypad7))
            Debug_RestoreTracks();

        // Timescale
        if (Input.GetKeyDown(KeyCode.Keypad8) & Input.GetKey(KeyCode.LeftControl)) // up
        {
            if (Time.timeScale < 1f)
                RhythmicGame.SetTimescale(Time.timeScale + 0.1f);
            else
                RhythmicGame.SetTimescale(1f);
        }
        if (Input.GetKeyDown(KeyCode.Keypad2) & Input.GetKey(KeyCode.LeftControl)) // down
            if (Time.timeScale > 0.1f)
                RhythmicGame.SetTimescale(Time.timeScale - 0.1f);
        if (Input.GetKeyDown(KeyCode.Keypad1)) // one
            RhythmicGame.SetTimescale(1f);
        if (Input.GetKeyDown(KeyCode.Keypad0)) // progressive slowmo test (tut)
            DoFailTest();

        if (Input.GetKeyDown(KeyCode.Keypad9) & Input.GetKey(KeyCode.LeftAlt))
            Player.MovePlayer(new Vector2(0, -Tunnel.Instance.radius), force: true);

        // Debug UI

        if (Input.GetKeyDown(KeyCode.F3))
        {
            isDebugOn = !isDebugOn;
            section_debug.SetActive(isDebugOn);
        }

        if (!isDebugOn)
            return;
    }

    float deltaTime;
    void Update()
    {
        if (!isDebugOn)
            return;

        // update framerate debug
        if (Time.timeScale == 0f)
            return;
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        framerateText.text = string.Format("Framerate: {0} FPS", Mathf.Ceil(fps).ToString());
    }

    public async void DoFailTest()
    {
        int msCounter = 50;

        for (float i = 1f; i > 0f; i -= 0.1f)
        {
            await Task.Delay(msCounter);
            RhythmicGame.SetTimescale(i);
            msCounter -= 5;
        }
    }
    async void Debug_RestoreTracks()
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

}
