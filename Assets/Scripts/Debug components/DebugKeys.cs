using UnityEngine;
using UnityEngine.InputSystem;

[DebugComponent(DebugComponentFlag.DebugKeys, DebugComponentType.Component)]
public partial class DebugKeys : DebugComponent
{
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
                DebugUI.SwitchToComponent();

            if (Input.GetKeyDown(KeyCode.Alpha8))
                DebugUI.SwitchToComponent(typeof(SelectionComponentTest));
            if (Input.GetKeyDown(KeyCode.Alpha9))
                DebugUI.SwitchToComponent(typeof(DebugStats));
        }

        // ConsoleServer test
        if (Input.GetKeyDown(KeyCode.J))
            ConsoleServer.Write("Hi!!!");

        // AMP songs debug
        HandleSongSwitching();

        // World stuff
        if (Gamepad.current != null && Gamepad.current.dpad.down.wasPressedThisFrame)
            DEBUG_DisableWorld();

        if (Input.GetKeyDown(KeyCode.M))
            DEBUG_ToggleRenderingPath();

        // Sequence & notes refreshing
        if (Input.GetKeyDown(KeyCode.Z))
            DEBUG_RefreshSequencesNotes(true);
        if (Input.GetKeyDown(KeyCode.T))
            DEBUG_RefreshSequencesNotes(false);

        // RESTART
        // TODO: Move to gameplay input!
        if (Input.GetKeyDown(KeyCode.R))
            RhythmicGame.Restart();
        if (Input.GetKeyDown(KeyCode.Escape))
            SongController.PlayPause();

        // Resolution
        if (!Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.F11) & Input.GetKey(KeyCode.LeftControl) & Input.GetKey(KeyCode.LeftShift))
                DEBUG_SetPreferredResolution(new Vector2(1280, 720));
            if (Input.GetKeyDown(KeyCode.F12) & Input.GetKey(KeyCode.LeftControl) & Input.GetKey(KeyCode.LeftShift))
                DEBUG_SetPreferredResolution(new Vector2(1920, 1080));
        }

        // FPS Lock
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F1)) DEBUG_SetFramerateLock(10);
        else if (Input.GetKeyDown(KeyCode.F1)) DEBUG_SetFramerateLock(60);
        else if (Input.GetKeyDown(KeyCode.F2)) DEBUG_SetFramerateLock(120);
        else if (Input.GetKeyDown(KeyCode.F3)) DEBUG_SetFramerateLock(200);
        else if (Input.GetKeyDown(KeyCode.F4)) DEBUG_SetFramerateLock(0);

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
            DEBUG_CaptureMeasureAmount(null, SongController.songLengthInMeasures, 0);

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
}