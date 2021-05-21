using UnityEngine;

public enum GameMode { Metagame, Gameplay, Practice, Editor, Charting, Debugging }
public enum GameMatchType { Singleplayer = 0, LocalMultiplayer = 1, OnlineMultiplayer = 2 }

public class GameState : MonoBehaviour
{
    public static GameState Instance;

    // TODO: Probably shouldn't be a static bool!
    public static bool IsLoading = true;

    // Only call this once when initializing the game.
    public static void CreateGameState()
    {
        if (!RhythmicGame.GameState)
        {
            var gStateObj = new GameObject("Game state");
            var gState = gStateObj.AddComponent<GameState>();
            DontDestroyOnLoad(gStateObj); // Keep as global entity

            Instance = gState;
            RhythmicGame.GameState = gState;
        }
    }

    // ----------------------------------------

    public static GameMode GameMode = GameMode.Gameplay;
    public static GameMatchType GameMatchTpye = GameMatchType.Singleplayer;
}
