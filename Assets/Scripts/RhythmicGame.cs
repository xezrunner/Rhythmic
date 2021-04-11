using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The game supports playing Amplitude songs. By using AMPLITUDE, the game will use a different
/// logic compared to the default RHYTHMIC song format. <br/>
/// Some features found in RHYTHMIC may not work / be disabled in AMPLITUDE mode.
/// </summary>
public enum GameLogic { AMPLITUDE, RHYTHMIC }
public enum GameMode { Metagame, Gameplay, Practice, Editor, Charting, Debugging }
public enum GameMatchType { Singleplayer = 0, LocalMultiplayer = 1, OnlineMultiplayer = 2 }

// TODO: Some props here could be moved to their respective class!
public static class RhythmicGame
{
    public static GameState GameState;

    // TODO: World & loading systems!
    public static string StartWorld = "DevScene";

    // TODO: These should persist between loads!
    public static void SetFramerate(int fps, int vsync = 0)
    {
        QualitySettings.vSyncCount = vsync;
        Application.targetFrameRate = fps;
    }
    public static void Restart()
    {
        SetTimescale(1f);
        SceneManager.CreateScene("temp");
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        SceneManager.LoadScene("Loading", LoadSceneMode.Single);
    }
    public static void SetTimescale(float speed)
    {
        Time.timeScale = speed;
        if (SongController.Instance) SongController.Instance.SetSongSpeed(speed);
    }
    public static void SetResolution(Vector2 resolution) { Screen.SetResolution((int)resolution.x, (int)resolution.y, FullScreenMode.FullScreenWindow); }
    public static void SetAVCalibrationOffset(float offsetMs) { AVCalibrationOffsetMs = offsetMs; /*Player.Instance.UpdateAVCalibrationOffset();*/ }

    public static bool IsDeveloper = true; // Enable dev buttons
    public static bool IsInteractiveDeveloper = true; // Enable debug UI features & stats
    public static bool IsInternal = true; // Enable internal tools

    static GameLogic _gameLogic = GameLogic.AMPLITUDE;
    /// <summary>
    /// The current game type. Refer to <see cref="global::GameLogic"/> for more info.
    /// </summary>
    public static GameLogic GameLogic
    {
        get { return _gameLogic; }
        set
        {
            _gameLogic = value;
            Debug.LogFormat("GAME: Game type changed: {0}", GameLogic.ToString());
        }
    }
    public static string GameTypeToFriendlyName()
    {
        if (GameLogic == GameLogic.AMPLITUDE) return "Amplitude";
        else if (GameLogic == GameLogic.RHYTHMIC) return "Rhythmic";
        else return "";
    }

    public static GameMode GameMode = GameMode.Gameplay;
    public static GameMatchType GameMatchTpye = GameMatchType.Singleplayer;

    public static bool IsLoading = true;
    public static Vector2 Resolution { get { return new Vector2(Screen.currentResolution.width, Screen.currentResolution.height); } }
    public static Vector2 PreferredResolution = new Vector2(1920, 1080);
    public static int LowestFramerate = 60; // 30

    // Debug system
    public static DebugComponentFlag DebugControllerFlags = DebugComponentFlag.Uninitialized;
    public static bool AutoLoadDebugStats = false;

    // A/V calibration props | milliseconds
    public static float AVCalibrationOffsetMs = 0f;
    public static float AVCalibrationStepMs = 16.67f;

    // Gameplay props
    public static float SlopMs = 300f; // 100f
    public static float DebounceWindowMs = 85f;

    public static int HorizonMeasures = 6; // How many measures should we be able to see ahead of us
    public static float HorizonMeasuresOffset = 15f; // How many units to offset (backwards) from the horizon when clipping

    // --- Track streaming --- //
    public static bool StreamAllMeasuresOnStart = false;
    public static bool FastStreaming = false;
    static FastStreamingLevel _fastStreamingLevel = FastStreamingLevel.MeasuresAndNotes;

    public static FastStreamingLevel FastStreamingLevel
    {
        get
        {
            if (FastStreaming) return _fastStreamingLevel;
            else return FastStreamingLevel.None;
        }
    }

    public static bool IsTunnelMode = false; // Whether to use tunnel gameplay mode
    public static bool TunnelTrackDuplication = true; // Whether to duplicate tracks when using tunnel mode
    public static bool IsTunnelTrackDuplication { get { return (IsTunnelMode && TunnelTrackDuplication); } }
    static int _tunnelTrackDuplicationNum = 3; // How many times should track sets exist in Tunnel mode
    public static int TunnelTrackDuplicationNum // Automatically returns 1 when tunnel mode or track duplication is off
    {
        get { return (IsTunnelMode & TunnelTrackDuplication) ? _tunnelTrackDuplicationNum : 1; }
        set { _tunnelTrackDuplicationNum = value; }
    }
    public static int TunnelTrackDuplicationMult { get { return _tunnelTrackDuplicationNum - 1; } }

    public static bool TrackSeekingEnabled = true; // Whether to skip empty tracks when switching tracks

    public static int TrackCaptureLength = 12; // How many measures to capture when you clear a sequence

    public static bool GlobalEdgeLightsCaptureClipping = false; // Whether global edge lights should clip along with the capture effects
    public static bool CapturedNoteLightup = true; // Whether notes that are being captured should light up
    public static bool CapturedNoteInteractiveLightup = true; // Whether inactive notes should light up as we pass by them
    public static bool CapturedNoteBPMPulsation = true; // Whether inactive notes should pulse with the BPM

    public static int SequenceAmount = 2; // How many measures is a sequence? (How many measures do we have to play in a track)

    public static bool PlayableFreestyleTracks = false; // FreQuency-style freestyle tracks
    public static float TrackWidth = 3.6f; // 2.36f // 3.6755f
    public static float TrackHeight = 0.62f; // 0.4f

    public enum GameDifficulty { Beginner = 0, Intermediate = 1, Advanced = 2, Expert = 3, Super = 4 }
    public static GameDifficulty Difficulty = GameDifficulty.Expert;

    public static float[] DifficultyFudgeFactors = new float[] { 1f, 1f, 0.93f, 0.8f, 0.8f };

    // Tech props
    public static bool AutoFindPathFallback = true; // Whether to attempt finding an object named 'Path' in case a world/song path was never specified.

    public static bool EnableTrackVisualClipping = true; // Controls all visual track clipping effects in the game.
    public static bool DisableTrackLengthClipping = false; // Controls the horizon clipping effect

    /* ----- DEBUG props ----- */

    // Event debug
    public static bool DebugTrackCapturingEvents = false;
    public static bool DebugTrackCapturingEase = false;

    public static bool DebugPlayerCameraAnimEvents = false;
    public static bool DebugPlayerTrackSwitchEvents = false;
    public static bool DebugPlayerTrackSeekEvents = true;

    public static bool DebugCatchResultEvents = false;
    public static bool DebugCatcherSlopEvents = false;
    public static bool DebugTargetNoteRefreshEvents = false;
    public static bool DebugSequenceRefreshEvents = false;

    // Draw debug
    public static bool DebugDrawTunnelGizmos = false;
    public static bool DebugDrawWorldLights = true;
}
