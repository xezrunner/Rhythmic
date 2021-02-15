using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugController : MonoBehaviour
{
    public static DebugController Instance;

    SongController SongController { get { return SongController.Instance; } }

    public GameObject section_debug;
    public GameObject section_controllerinput;

    public TextMeshProUGUI inputlagText;
    public TextMeshProUGUI framerateText;
    public TextMeshProUGUI debugText;
    public TextMeshProUGUI debugLineText;

    void Awake()
    {
        Instance = this;
        GameState.CreateGameState();
    }

    private void Start()
    {
        // TODO TODO TODO!!! This causes a Unity bug where the editor locks up when the server is not properly closed & disposed!
        //ConsoleServer.StartConsoleServer();

        if (RhythmicGame.FastStreamingLevel.HasFlag(FastStreamingLevel.Tracks))
            AddToDebugLine("Warning! RhythmicGame.FastStreamingLevel has Tracks flag! This is slow!");
    }

    int bananasCounter = -1;

    // TODO: improve this! Add Logger compatibility!
    public static void AddToDebugLine(string text) => Instance._AddToDebugLine(text);
    void _AddToDebugLine(string text)
    {
        if (debugLineText.text.Length == 0) { debugLineText.text = text; return; }
        string s = debugLineText.text;

        int charCount = 0;
        int newlineCount = 0;
        for (int i = 0; i < s.Length; i++, charCount++)
        {
            if (s[i] == '\n')
                newlineCount++;
        }

        if (charCount == 0) // assume that we have one line without a newline
            charCount = s.Length;

        s = s.Insert(charCount, '\n' + text);
        newlineCount++;

        if (newlineCount >= 4) // line cleanup! max 4 lines!
            s = s.Remove(0, s.IndexOf('\n') + 1);

        debugLineText.text = s;
    }

    public bool isDebugOn = true;
    bool isDebugPrintOn = true;

    void UpdateMainDebugText()
    {
        if (!SongController.IsEnabled)
        {
            debugText.text = "SongController Enabled: False";
            return;
        }

        int trackCount = TracksController.Instance.Tracks.Count;
        string trackNames = "";
        TracksController.Instance.Tracks.ForEach(t => trackNames += $"{t.TrackName}  ");

        string s = /*$"World: DevScene\n" +
                   $"Room path: /rooms/_u_trans_/dev/dev_scene.drm [SceneToRoom]\n" +
                   $"SongController Enabled: {SongController.IsEnabled}\n\n" +*/

                   $"Song name: {SongController.songName}\n" +
                   $"Song BPM: {SongController.songBpm}  Song scale: {SongController.songFudgeFactor.ToString("0.00")}\n\n" +

                   $"Tracks: {trackNames}({trackCount})\n\n" +

                   $"SlopMs: {SongController.SlopMs}  SlopPos: {SongController.SlopPos}\n\n" +

                   /*
                   $"Timscale: [world: {Time.timeScale.ToString("0.00")}]  [song: {SongController.songTimeScale.ToString("0.00")}]\n" +
                   $"Clock seconds: {Clock.Instance.seconds}\n" +
                   $"Clock bar: {(int)Clock.Instance.bar}\n" +
                   $"Clock beat: {(int)Clock.Instance.beat % 8} ({(int)Clock.Instance.beat})\n" +
                   $"Locomotion distance: {AmpPlayerLocomotion.Instance.DistanceTravelled}\n\n" +
                   */

                   //$"LightManager: null | LightGroups:  (0)";
                   "";

        if (!isDebugPrintOn)
            s += "\n\nDEBUG PRINT FREEZE";

        debugText.text = s;
    }

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
        // Debug control
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F3))
        {
            isDebugOn = !isDebugOn;
            section_debug.SetActive(isDebugOn);
        }

        if (!isDebugOn)
            return;

        // ----- DEBUG LOOP ----- //

        if (Input.GetKeyDown(KeyCode.L))
            AddToDebugLine($"bananas! {bananasCounter++}");

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F4)) // freeze printing
        {
            isDebugPrintOn = !isDebugPrintOn;
            UpdateMainDebugText();
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F4)) // disable debug print UI
        {
            isDebugPrintOn = !isDebugPrintOn;
            section_debug.SetActive(isDebugPrintOn);
        }

        if (isDebugPrintOn)
            UpdateMainDebugText();

        if (Keyboard.current.jKey.wasPressedThisFrame)
            ConsoleServer.Write("Hi!!!");

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
            var world = GameObject.Find("World");
            var worldCamera = GameObject.Find("WorldCamera");

            world.SetActive(!world.activeInHierarchy); worldCamera.SetActive(!world.activeInHierarchy);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            if (Camera.main.renderingPath == RenderingPath.Forward)
                Camera.main.renderingPath = RenderingPath.DeferredShading;
            else
                Camera.main.renderingPath = RenderingPath.Forward;
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
        if (Input.GetKeyDown(KeyCode.F12) & Input.GetKey(KeyCode.LeftControl) & Input.GetKey(KeyCode.LeftShift))
        {
            RhythmicGame.PreferredResolution = new Vector2(1920, 1080);
            RhythmicGame.SetResolution(RhythmicGame.PreferredResolution);
        }

        // FPS Lock
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F1)) { RhythmicGame.SetFramerate(10); }
        else if (Input.GetKeyDown(KeyCode.F1)) { RhythmicGame.SetFramerate(60); }
        else if (Input.GetKeyDown(KeyCode.F2)) { RhythmicGame.SetFramerate(120); }
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
            TracksController.Instance.CaptureMeasureAmount(Mathf.FloorToInt(Clock.Instance.bar), RhythmicGame.TrackCaptureLength, TracksController.Instance.CurrentTrackID);

        else if (Input.GetKeyDown(KeyCode.Keypad5)) // 5
            TracksController.Instance.CaptureMeasureAmount(Mathf.FloorToInt(Clock.Instance.bar), RhythmicGame.TrackCaptureLength, TracksController.Instance.Tracks);

        else if (Input.GetKeyDown(KeyCode.Keypad6)) // all!
        { }

        // Track restoration (buggy!)
        if (Input.GetKeyDown(KeyCode.Keypad7))
        { }

        // Debug move forward
        //if (Input.GetKeyDown(KeyCode.Keypad9))
        //{
        //    AmpPlayerLocomotion.Instance.IsPlaying = true;
        //    AmpPlayerLocomotion.Instance.DistanceTravelled += 140;
        //    await Task.Delay(100);
        //    AmpPlayerLocomotion.Instance.IsPlaying = false;
        //}

        if (!Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Keypad8))
        {
            float offset = 2f;
            AmpPlayerLocomotion.Instance.DistanceTravelled += offset * SongController.posInSec;
            SongController.Instance.OffsetSong(offset);
            Clock.Instance.seconds += offset;
        }

        if (!Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Keypad2))
        {
            float offset = 2f;
            AmpPlayerLocomotion.Instance.DistanceTravelled -= offset * SongController.posInSec;
            SongController.Instance.OffsetSong(-offset);
            Clock.Instance.seconds -= offset;
        }

        if (Input.GetKeyDown(KeyCode.Q))
            AmpPlayerTrackSwitching.Instance.SwitchToTrack(0, true);
        else if (Input.GetKeyDown(KeyCode.P))
            AmpPlayerTrackSwitching.Instance.SwitchToTrack(TracksController.Instance.Tracks.Count - 1, true);

        // Timescale
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Keypad8)) // up
        {
            if (Time.timeScale < 1f)
                RhythmicGame.SetTimescale(Time.timeScale + 0.1f);
            else
                RhythmicGame.SetTimescale(1f);
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Keypad2)) // down
            if (Time.timeScale > 0.1f)
                RhythmicGame.SetTimescale(Time.timeScale - 0.1f);
        if (Input.GetKeyDown(KeyCode.Keypad1)) // one
            RhythmicGame.SetTimescale(1f);
        if (Input.GetKeyDown(KeyCode.Keypad0)) // progressive slowmo test (tut)
            DoFailTest();
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
