using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class RhythmicGame : MonoBehaviour
{
    private void Start()
    {
        Debug.LogFormat("GAME [init]: Game type is {0}", GameType.ToString());
    }
    private void Awake()
    {
#if UNITY_ANDROID
        SetFramerate(60);
#elif UNITY_STANDALONE
        SetFramerate(200); 
#endif
    }

    public static void SetFramerate(int fps, int vsync = 0)
    {
        QualitySettings.vSyncCount = vsync;
        Application.targetFrameRate = fps;
    }
    public static void Restart() { SetTimescale(1f); AmplitudeSongController.IsSongPlaying = false; SceneManager.LoadScene("Loading", LoadSceneMode.Single); }
    public static void SetTimescale(float speed)
    {
        Time.timeScale = speed;
        AmplitudeSongController.Instance.AdjustSongSpeed(speed);
    }
    public static void SetResolution(Vector2 resolution) { Screen.SetResolution((int)resolution.x, (int)resolution.y, FullScreenMode.ExclusiveFullScreen); }

    /// <summary>
    /// The game supports playing Amplitude songs. By using AMPLITUDE, the game will use a different
    /// logic compared to the default RHYTHMIC song format.
    /// </summary>
    public enum _GameType { AMPLITUDE, RHYTHMIC }

    static _GameType m_GameType = _GameType.AMPLITUDE;
    /// <summary>
    /// The current game type. Refer to <see cref="_GameType"/> for more info.
    /// </summary>
    public static _GameType GameType
    {
        get { return m_GameType; }
        set
        {
            m_GameType = value;
            Debug.LogFormat("GAME: Game type changed: {0}", GameType.ToString());
        }
    }

    public static bool IsLoading = true;

    public static Vector2 PreferredResolution = new Vector2(1920, 1080);

    // Gameplay props
    public static bool IsTunnelMode = false; // Whether to use tunnel gameplay mode
    public static bool TunnelTrackDuplication = true; // Whether to duplicate tracks when using tunnel mode
    static int _tunnelTrackDuplicationNum = 3; // How many times to duplicate each track
    public static int TunnelTrackDuplicationNum // Automatically returns 1 when tunnel mode or track duplication is off
    {
        get { return (IsTunnelMode & TunnelTrackDuplication) ? _tunnelTrackDuplicationNum : 1; }
        set { _tunnelTrackDuplicationNum = value; }
    }

    public static bool TrackSeekEmpty = true; // Whether to skip empty tracks when switching tracks
    public static int TrackCaptureLength = 7; // How many measures to capture when you clear a sequence

    public static bool PlayableFreestyleTracks = false;

    public static float TrackWidth = 2.36f;

    public enum GameDifficulty { Beginner = 0, Intermediate = 1, Advanced = 2, Expert = 3, Super = 4 }
    public static GameDifficulty Difficulty = GameDifficulty.Expert;

    public static float[] DifficultyFudgeFactors = new float[] { 1f, 1f, 0.93f, 0.8f, 0.8f };

    // Event debug
    public static bool DebugTrackCreationEvents = true;
    public static bool DebugTrackMaterialEvents = false;
    public static bool DebugLaneCreationEvents = false;
    public static bool DebugNoteCreationEvents = false;

    public static bool DebugPlayerMovementEvents = false;
    public static bool DebugPlayerCameraAnimEvents = false;
    public static bool DebugTrackSeekEvents = true;

    public static bool DebugNextNoteCheckEvents = true;

    // Draw debug
    public static bool DebugDrawLanes = false;
    public static bool DebugCatcherCasting = true;    
}
