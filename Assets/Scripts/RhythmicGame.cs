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
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;
    }

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

    // Gameplay props
    public static bool IsTunnelMode = false; // Whether to use tunnel gameplay mode
    public static bool TunnelTrackDuplication = false; // Whether to duplicate tracks when using tunnel mode
    public static int TunnelTrackDuplicationCount = 2; // How many times to duplicate each track

    public static bool TrackSeekEmpty = true; // Whether to skip empty tracks when switching tracks
    public static int TrackCaptureLength = 7; // How many measures to capture when you clear a sequence
    public static bool PlayableFreestyleTracks = false;

    public static float TrackWidth = 2.36f;

    // Event debug
    public static bool DebugTrackCreationEvents = true;
    public static bool DebugTrackMaterialEvents = false;
    public static bool DebugLaneCreationEvents = false;
    public static bool DebugNoteCreationEvents = false;

    public static bool DebugPlayerMovementEvents = false;
    public static bool DebugTrackSeekEvents = true;

    public static bool DebugNextNoteCheckEvents = true;

    // Draw debug
    public static bool DebugDrawLanes = false;
    public static bool DebugCatcherCasting = true;

    // AMPLITUDE properties
    public static string AMP_songFolder = @"H://HMXAMPLITUDE//Extractions//amplitude_ps4_extraction//ps4//songs";
    public static string AMP_GetSongFilePath(string songName, AMP_FileExtension extension)
    {
        return string.Format("{0}//{1}//{1}.{2}", AMP_songFolder, songName, extension);
    }

    public enum AMP_FileExtension { mid, mogg, moggsong }
}
