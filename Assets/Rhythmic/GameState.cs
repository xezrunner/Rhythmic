using UnityEngine;
using static Logger;

public enum Song_Type { RHYTHMIC, AMPLITUDE }

public class GameState : MonoBehaviour
{
    public static GameState Instance;
    public static GameVariables Variables;

    public GameVariables variables;

    public void Start()
    {
        if (Instance)
        {
            LogE("A GameState instance already exists - destroying!".T(this));
            Destroy(this);
            return;
        }
        Instance = this;

        // Log("GameState created.".T(this));

        STARTUP_Main();
    }

    void STARTUP_Main()
    {
        STARTUP_InitVariables();

        // TODO: Create systems / handle meta too!
        // For now, we'll just start the song system with an example song:
        if (!SongSystem.Instance && LogE("Can't find the SongSystem!")) return;
        SongSystem.Instance.InitializeSong("allthetime", Song_Type.AMPLITUDE);
    }
    void STARTUP_InitVariables()
    {
        if (!variables) variables = gameObject.AddComponent<GameVariables>();
        Variables = variables;
        // Load variables from file(s) etc...
    }
}
