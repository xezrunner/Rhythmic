using UnityEngine;
using UnityEngine.InputSystem;

[DebugComponent(DebugComponentFlag.DebugKeys, DebugComponentType.Component)]
public partial class DebugKeys : DebugComponent
{
    public static DebugKeys Instance;

    public bool IsEnabled = true;

    void Awake() => Instance = this;

    // Main loop
    void Update()
    {
        if (!IsEnabled)
            return;

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
        else if (Input.GetKeyDown(KeyCode.Z))
            DEBUG_RefreshSequencesNotes(true);
        if (Input.GetKeyDown(KeyCode.T))
            DEBUG_RefreshSequencesNotes(false);

        // RESTART
        // TODO: Move to gameplay input!
        if (Input.GetKeyDown(KeyCode.R))
            RhythmicGame.Restart();
        if (Input.GetKeyDown(KeyCode.Escape))
            SongController.TogglePause();

        // Resolution
        if (Input.GetKeyDown(KeyCode.F11) & Input.GetKey(KeyCode.LeftControl))
            DEBUG_SetPreferredResolution(new Vector2(1280, 720));
        if (Input.GetKeyDown(KeyCode.F12) & Input.GetKey(KeyCode.LeftControl) & Input.GetKey(KeyCode.LeftShift))
            DEBUG_SetPreferredResolution(new Vector2(1920, 1080));

        // FPS Lock
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F1)) DEBUG_SetFramerateLock(10);
            else if (Input.GetKeyDown(KeyCode.F1)) DEBUG_SetFramerateLock(60);
            else if (Input.GetKeyDown(KeyCode.F2)) DEBUG_SetFramerateLock(120);
            else if (Input.GetKeyDown(KeyCode.F3)) DEBUG_SetFramerateLock(200);
            else if (Input.GetKeyDown(KeyCode.F4)) DEBUG_SetFramerateLock(0);
        }

        // Toggle tunnel mode
        if (Input.GetKeyDown(KeyCode.F))
            DEBUG_ToggleTunnelMode();

        // Lag compensation
        if (Input.GetKey(KeyCode.KeypadPlus))
            RhythmicGame.SetAVCalibrationOffset(RhythmicGame.AVCalibrationOffsetMs + RhythmicGame.AVCalibrationStepMs);
        else if (Input.GetKey(KeyCode.KeypadMinus))
            RhythmicGame.SetAVCalibrationOffset(RhythmicGame.AVCalibrationOffsetMs - RhythmicGame.AVCalibrationStepMs);

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

        if (!Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Keypad8))
            DEBUG_OffsetSong(2f);

        if (!Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Keypad2))
            DEBUG_OffsetSong(2f);

        // Quick track switching
        if (Input.GetKeyDown(KeyCode.Q)) // First track (0)
            AmpPlayerTrackSwitching.Instance.SwitchToTrack(0, true);
        else if (Input.GetKeyDown(KeyCode.P)) // Last track
            AmpPlayerTrackSwitching.Instance.SwitchToTrack(TracksController.Instance.Tracks.Count - 1, true);

        // Timescale
        DEBUG_HandleTimescale();

        if (Input.GetKeyDown(KeyCode.Keypad0)) // progressive slowmo test (tut)
            DoFailTest();
    }
}