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
    SongController SongController { get { return SongController.Instance; } }

    public GameObject section_debug;
    public GameObject section_controllerinput;
    public TextMeshProUGUI inputlagText;
    public TextMeshProUGUI framerateText;

    private void Start()
    {

    }

    public bool isDebugOn = true;

    public void AMP_ChangeSong(string value)
    {
        SongController.songName = value;
        //Player.ScoreText.text = string.Format("Song changed: {0} - restart!", value);
    }

#if UNITY_ANDROID
    int android_songcounter = 0;
#endif

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

#if UNITY_ANDROID
        // Test code for changing songs on an Android device
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
#endif

        if (Gamepad.current != null && Gamepad.current.dpad.down.wasPressedThisFrame)
        {
            var world = GameObject.Find("WORLD_TUT");
            var worldCamera = GameObject.Find("MainCamera");

            world.SetActive(!world.activeInHierarchy); worldCamera.SetActive(!world.activeInHierarchy);
        }

        // Sequence & notes refreshing
        if (Input.GetKeyDown(KeyCode.T))
        {
            TracksController.Instance.RefreshSequences();
            TracksController.Instance.RefreshTargetNotes();
            Debug.LogWarning("Debug: Refreshing with no track specified!");
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            TracksController.Instance.RefreshSequences(TracksController.Instance.CurrentTrack);
            TracksController.Instance.RefreshTargetNotes(TracksController.Instance.CurrentTrack);
            Debug.LogWarning("Debug: Refreshing with current track specified!");
        }

        // RESTART
        if (Input.GetKeyDown(KeyCode.R))
            RhythmicGame.Restart();
        if (Input.GetKeyDown(KeyCode.Escape))
            SongController.TogglePause();

        // TEMP / DEBUG

        // Resolution
        if (Input.GetKeyDown(KeyCode.F11) & Input.GetKey(KeyCode.LeftControl))
        {
            RhythmicGame.PreferredResolution = new Vector2(1280, 720);
            RhythmicGame.SetResolution(RhythmicGame.PreferredResolution);
        }

        // FPS Lock
        if (Input.GetKeyDown(KeyCode.F1)) { RhythmicGame.SetFramerate(60); }
        else if (Input.GetKeyDown(KeyCode.F2)) { RhythmicGame.SetFramerate(144); }
        else if (Input.GetKeyDown(KeyCode.F3)) { RhythmicGame.SetFramerate(200); }
        else if (Input.GetKeyDown(KeyCode.F4)) { RhythmicGame.SetFramerate(0); }

        // Toggle tunnel mode
        if (Input.GetKeyDown(KeyCode.F))
        {
            RhythmicGame.IsTunnelMode = !RhythmicGame.IsTunnelMode;
            //Player.ScoreText.text = string.Format("Tunnel {0} - restart!", RhythmicGame.IsTunnelMode ? "ON" : "OFF");
        }

        // Lag compensation
        if (Input.GetKey(KeyCode.KeypadPlus))
        {
            RhythmicGame.SetAVCalibrationOffset(RhythmicGame.AVCalibrationOffsetMs + RhythmicGame.AVCalibrationStepMs);
            inputlagText.text = string.Format("Player offset (ms): {0}", RhythmicGame.AVCalibrationOffsetMs);
        }
        else if (Input.GetKey(KeyCode.KeypadMinus))
        {
            RhythmicGame.SetAVCalibrationOffset(RhythmicGame.AVCalibrationOffsetMs - RhythmicGame.AVCalibrationStepMs);
            inputlagText.text = string.Format("Player offset (ms): {0}", RhythmicGame.AVCalibrationOffsetMs);
        }

        // Track capturing debug
        if (Input.GetKeyDown(KeyCode.H)) // current track, 5
            TracksController.Instance.CaptureMeasureAmount(Mathf.FloorToInt(Clock.Instance.bar), 7, 0);

        else if (Input.GetKeyDown(KeyCode.Keypad5)) // 5
            TracksController.Instance.CaptureMeasureAmount(Mathf.FloorToInt(Clock.Instance.bar), 7, TracksController.Instance.Tracks);

        else if (Input.GetKeyDown(KeyCode.Keypad6)) // all!
        { }

        // Track restoration (buggy!)
        if (Input.GetKeyDown(KeyCode.Keypad7))
        { }

        if (Input.GetKeyDown(KeyCode.Keypad9))
            AmpPlayerLocomotion.Instance.DistanceTravelled += 100f;

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
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
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

}
