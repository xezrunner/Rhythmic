using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static DebugFunctionality;

[DebugComponent(DebugComponentFlag.DebugKeys, DebugComponentType.Component)]
public partial class DebugKeys : DebugComponent
{
    public static TracksController TracksController { get { return TracksController.Instance; } }
    public static DebugUI DebugUI { get { return DebugUI.Instance; } }


    public static RefDebugComInstance Instance;

    public bool IsEnabled = true;

    void Awake() => Instance = new RefDebugComInstance(this);

    // Main loop
    void Update()
    {
        if (!IsEnabled)
            return;

        if (Input.GetKeyDown(KeyCode.KeypadDivide))
        {
            AmpTrack t = TracksController.CurrentTrack;
            string s = $"Sequences [{t.TrackName}]: ";
            foreach (var m in t.Sequences)
                s += $"({m.ID}); ";
            s += $"  [total: {t.Sequences.Count}]";
            Logger.Log(s);
        }

        // DebugUI:
        if (DebugUI.Instance)
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha7)) // toggle debug line
                DebugUI.IsDebugLineOn = !DebugUI.IsDebugLineOn;
            else if (Input.GetKeyDown(KeyCode.Alpha7)) // empty com
            {
                DebugUI.SwitchToComponent();
                DebugStats stats = (DebugStats)DebugStats.Instance.Component;
                stats.StatsMode = StatsMode.None;
            }

            if (Input.GetKeyDown(KeyCode.Alpha8))
                DebugUI.SwitchToComponent(typeof(SelectionComponentTest));
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                DebugStats stats = (DebugStats)DebugStats.Instance.Component;
                stats.StatsMode = StatsMode.Long;
            }
        }

        // ConsoleServer test
        if (Input.GetKeyDown(KeyCode.J))
            ConsoleServer.Write("Hi!!!");

        // AMP songs debug
        //HandleSongSwitching(); // Now handled in Debug menu!

        // World stuff
        //if (Gamepad.current != null && Gamepad.current.dpad.down.wasPressedThisFrame)
        //    DEBUG_DisableWorld();

        if (Input.GetKeyDown(KeyCode.M))
            DEBUG_ToggleRenderingPath();

        // Sequence & notes refreshing
        //if (Input.GetKeyDown(KeyCode.Z))
        //    DEBUG_RefreshSequencesNotes(true);
        if (Input.GetKeyDown(KeyCode.T))
            DEBUG_RefreshSequencesNotes(false);

        // RESTART
        // TODO: Move to gameplay input!
        if (Input.GetKeyDown(KeyCode.R))
            RhythmicGame.Restart();
        if (Input.GetKeyDown(KeyCode.Escape))
            SongController.Instance.PlayPause();

        // Resolution
        if (!Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.F11) & Input.GetKey(KeyCode.LeftControl) & Input.GetKey(KeyCode.LeftShift))
                DEBUG_SetPreferredResolution(new Vector2(1280, 720));
            if (Input.GetKeyDown(KeyCode.F12) & Input.GetKey(KeyCode.LeftControl) & Input.GetKey(KeyCode.LeftShift))
                DEBUG_SetPreferredResolution(new Vector2(1920, 1080));
        }

        // FPS Lock
        if (Keyboard.current.shiftKey.isPressed && Keyboard.current.f5Key.wasPressedThisFrame) DEBUG_SetFramerateLock(0, vsync: true);
        if (Keyboard.current.ctrlKey.isPressed)
        {
            //if (Keyboard.current.ctrlKey.isPressed && Keyboard.current.f1Key.wasPressedThisFrame) DEBUG_SetFramerateLock(10);
            if (Keyboard.current.f1Key.wasPressedThisFrame) DEBUG_SetFramerateLock(60);
            else if (Keyboard.current.f2Key.wasPressedThisFrame) DEBUG_SetFramerateLock(120);
            else if (Keyboard.current.f3Key.wasPressedThisFrame) DEBUG_SetFramerateLock(200);
            else if (Keyboard.current.f4Key.wasPressedThisFrame) DEBUG_SetFramerateLock(0);
        }

        // Toggle tunnel mode
        if (Input.GetKeyDown(KeyCode.F))
            DEBUG_ToggleTunnelMode();

        // Streamer delay test
        if (Keyboard.current.leftCtrlKey.isPressed)
        {
            bool shouldLog = true;

            if (Keyboard.current.numpadPlusKey.wasPressedThisFrame)
                TrackStreamer.Instance.DestroyDelay += 0.1f;
            else if (Keyboard.current.numpadMinusKey.wasPressedThisFrame)
                TrackStreamer.Instance.DestroyDelay -= 0.1f;
            else
                shouldLog = false;

            if (shouldLog)
                Logger.Log($"DestroyDelay: {TrackStreamer.Instance.DestroyDelay.ToString().AddColor(Colors.Application)}", "TrackStreamer", false);
        }

        // Lag compensation
        if (!Keyboard.current.leftCtrlKey.isPressed)
        {
            if (Input.GetKey(KeyCode.KeypadPlus))
                RhythmicGame.SetAVCalibrationOffset(RhythmicGame.AVCalibrationOffsetMs + RhythmicGame.AVCalibrationStepMs);
            else if (Input.GetKey(KeyCode.KeypadMinus))
                RhythmicGame.SetAVCalibrationOffset(RhythmicGame.AVCalibrationOffsetMs - RhythmicGame.AVCalibrationStepMs);
        }

        // Track capturing debug
        if (Input.GetKeyDown(KeyCode.H)) // current track, 5
            DEBUG_CaptureMeasureAmount(TracksController.CurrentTrack, RhythmicGame.TrackCaptureLength);

        else if (Input.GetKeyDown(KeyCode.Keypad5)) // 5
            DEBUG_CaptureMeasureAmount(null, RhythmicGame.TrackCaptureLength);

        else if (Input.GetKeyDown(KeyCode.Keypad6)) // all!
            DEBUG_CaptureMeasureAmount(null, SongController.Instance.songLengthInMeasures);

        // Track restoration (buggy!)
        if (Input.GetKeyDown(KeyCode.Keypad7))
            DEBUG_RestoreCapturedTracks();

        // Song offsetting
        HandleSongOffsetting();

        // Enable IsPlaying property in Locomotion
        if (Keyboard.current.shiftKey.isPressed && Keyboard.current.numpadMinusKey.wasPressedThisFrame)
        {
            AmpPlayerLocomotion.Instance.IsPlaying = !AmpPlayerLocomotion.Instance.IsPlaying;
            Logger.Log($"DEBUG: Locomotion IsPlaying: {AmpPlayerLocomotion.Instance.IsPlaying}");
        }

        // Quick track switching
        if (Input.GetKeyDown(KeyCode.Q)) // First track (0)
            AmpPlayerTrackSwitching.Instance.SwitchToTrack(0, true);
        else if (Input.GetKeyDown(KeyCode.P)) // Last track
            AmpPlayerTrackSwitching.Instance.SwitchToTrack(TracksController.Instance.Tracks.Length - 1, true);

        // Timescale
        DEBUG_HandleTimescale();

        if (Input.GetKeyDown(KeyCode.Keypad0)) // progressive slowmo test (tut)
            DoFailTest();
    }

    public IEnumerator DoFailTest_Coroutine()
    {
        float elapsed_ms = 0f;
        float ms = 50;

        while (ms > 0)
        {
            RhythmicGame.SetTimescale(ms / 50);
            elapsed_ms += Time.unscaledDeltaTime * 1000;
            if (elapsed_ms > ms) { elapsed_ms = 0; ms -= 5; }

            yield return null;
        }
    }

    public void DoFailTest() => StartCoroutine(DoFailTest_Coroutine());
}