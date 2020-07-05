using UnityEngine;
using System.Collections;

public class RhythmicGame: MonoBehaviour
{
    /// <summary>
    /// The game supports playing Amplitude songs. By using AMPLITUDE, the game will use a different
    /// logic compared to the default RHYTHMIC song format.
    /// </summary>
    public enum _GameType
    {
        AMPLITUDE,
        RHYTHMIC
    }

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

    public static bool DebugTrackMaterialEvents = false;
    public static bool DebugTrackCreationEvents = true;
    public static bool DebugLaneCreationEvents = false;
    public static bool DebugNoteCreationEvents = false;

    private void Start()
    {
        Debug.LogFormat("GAME [init]: Game type is {0}", GameType.ToString());
    }
}
