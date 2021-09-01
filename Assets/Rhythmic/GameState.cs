using UnityEngine;
using static Logger;

public class GameState : MonoBehaviour
{
    public static GameState Instance;
    public static GameVariables Variables;

    public GameVariables variables;

    public void Awake()
    {
        if (Instance)
        {
            LogE("A GameState instance already exists - destroying!".T(this));
            Destroy(this);
            return;
        }
        Instance = this;

        Log("GameState created.".T(this));

        STARTUP_Main();
    }

    void STARTUP_Main()
    {
        STARTUP_InitVariables();
    }

    void STARTUP_InitVariables()
    {
        if (!variables) variables = gameObject.AddComponent<GameVariables>();
        Variables = variables;
        // Load variables from file(s) etc...
    }
}
