using UnityEngine;
using System.Collections;

public class RhythmicGame : MonoBehaviour
{
    private void Start()
    {
        Debug.LogFormat("GAME [init]: Game type is {0}", GameType.ToString());
    }

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
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

    // Event debug
    public static bool DebugTrackCreationEvents = true;
    public static bool DebugTrackMaterialEvents = false;
    public static bool DebugLaneCreationEvents = false;
    public static bool DebugNoteCreationEvents = false;

    public static bool DebugPlayerMovementEvents = false;

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
