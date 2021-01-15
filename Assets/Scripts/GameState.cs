using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState Instance;

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


}
