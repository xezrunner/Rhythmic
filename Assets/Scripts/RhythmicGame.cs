using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// The game supports playing Amplitude songs. By using AMPLITUDE, the game will use a different
/// logic compared to the default RHYTHMIC song format. <br/>
/// Some features found in RHYTHMIC may not work / be disabled in AMPLITUDE mode.
/// </summary>
public enum GameLogic { AMPLITUDE, RHYTHMIC }
public enum GameMode { Metagame, Gameplay, Practice, Editor, Charting, Debugging }

public static class RhythmicGame
{
    public static void SetFramerate(int fps, int vsync = 0)
    {
        QualitySettings.vSyncCount = vsync;
        Application.targetFrameRate = fps;
    }
    public static void Restart() { SetTimescale(1f); Player.Instance.IsPlaying = false; SongController.Instance.IsPlaying = false; SceneManager.LoadScene("Loading", LoadSceneMode.Single); }
    public static void SetTimescale(float speed)
    {
        Time.timeScale = speed;
        SongController.Instance.SetSongSpeed(speed);
    }
    public static void SetResolution(Vector2 resolution) { Screen.SetResolution((int)resolution.x, (int)resolution.y, FullScreenMode.ExclusiveFullScreen); }
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

    public static bool IsLoading = true;
    public static Vector2 PreferredResolution = new Vector2(1920, 1080);

    // A/V calibration props | milliseconds
    public static float AVCalibrationOffsetMs = 0f;
    public static float AVCalibrationStepMs = 16.67f;

    // Gameplay props
    public static float SlopMs = 100f;
    public static float DebounceWindowMs = 85f;
    public static int HorizonMeasures = 15; // How many measures should we be able to see ahead of us

    public static bool IsTunnelMode = false; // Whether to use tunnel gameplay mode
    public static bool TunnelTrackDuplication = true; // Whether to duplicate tracks when using tunnel mode
    static int _tunnelTrackDuplicationNum = 3; // How many tracks should be there when we duplicate in tunnel mode
    public static int TunnelTrackDuplicationNum // Automatically returns 1 when tunnel mode or track duplication is off
    {
        get { return (IsTunnelMode & TunnelTrackDuplication) ? _tunnelTrackDuplicationNum : 1; }
        set { _tunnelTrackDuplicationNum = value; }
    }

    public static bool TrackSeekingEnabled = true; // Whether to skip empty tracks when switching tracks
    public static int TrackCaptureLength = 7; // How many measures to capture when you clear a sequence

    public static bool PlayableFreestyleTracks = false; // FreQuency-style freestyle tracks
    public static bool CapturedNoteLightup = true; // Whether notes that are being captured should light up
    public static bool CapturedNoteInteractiveLightup = true; // Whether inactive notes should light up as we pass by them
    public static bool CapturedNoteBPMPulsation = true; // Whether inactive notes should pulse with the BPM

    public static float TrackWidth = 3.6f; // 2.36f // 3.6755f
    public static float TrackHeight = 0.62f; // 0.4f

    public enum GameDifficulty { Beginner = 0, Intermediate = 1, Advanced = 2, Expert = 3, Super = 4 }
    public static GameDifficulty Difficulty = GameDifficulty.Expert;

    public static float[] DifficultyFudgeFactors = new float[] { 1f, 1f, 0.93f, 0.8f, 0.8f };

    // Event debug
    public static bool DebugTrackCreationEvents = true;
    public static bool DebugTrackCapturingEvents = false;
    public static bool DebugTrackCapturingEase = false;

    public static bool DebugPlayerCameraAnimEvents = false;
    public static bool DebugPlayerTrackSwitchEvents = true;
    public static bool DebugTrackSeekEvents = true;

    public static bool DebugNextNoteCheckEvents = true;

    // Draw debug
    public static bool DebugDrawLanes = false;
    public static bool DebugCatcherCasting = true;
}
