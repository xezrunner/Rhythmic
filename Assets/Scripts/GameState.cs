using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static Logger;
using static InputHandler;

public enum GameMode { Metagame, Gameplay, Practice, Editor, UNKNOWN = -1 }
public enum GameMatchType { Singleplayer = 0, LocalMultiplayer = 1, OnlineMultiplayer = 2 }
public enum StartupTarget { Startup = 0, Metagame = 1, Ingame = 2, NONE = -1, UNINITIALIZED = -2 }

public class GameState : MonoBehaviour
{
    public static GameState Instance;

    bool is_state_startup = false;

    // Only call this once when initializing the game.
    public static GameState CreateGameState(bool from_startup = false)
    {
        if (RhythmicGame.GameState)
        {
            LogW("Attempted to create a GameState when one already exists - ignoring!".M());
            return null;
        }

        var obj = new GameObject("Game state");
        var game_state = obj.AddComponent<GameState>();

        game_state.is_state_startup = from_startup;

        Instance = game_state;
        return game_state;
    }

    public RhythmicGame Props;
    public bool IsLoading = false;

    public StartupTarget StartupTarget = StartupTarget.UNINITIALIZED;
    public string StartupTarget_IngameScene = "RH_Main"; // @Prop

    void Awake()
    {
        if (!Instance) Instance = this; else Destroy(gameObject);
        DontDestroyOnLoad(gameObject); // Keep as global entity

        CreateProps();
        DebugConsole.RegisterCommand("scene", (string[] args) => LoadScene(args[0]));
    }

    Keyboard Keys = Keyboard.current;
    void Start()
    {
        LogConsoleW("GameState startup procedure".TM(this)); Debug.Log("GameState startup procedure".TM(this));

        // MetaSystem can be initialized by this component.

        // BUILD:  Hold [SHIFT] to jump into a default game | TODO: only in debug builds! | TODO: skip cinematic intros!
        if (is_state_startup)
        {

            if (!Application.isEditor) StartupTarget = !(Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed) ? StartupTarget.Startup : StartupTarget.Ingame;
            // EDITOR: Hold [SHIFT] to start into Metagame, hold [CTRL + SHIFT] to fully start the game, [ALT] to jump into a default game
            else if (Application.isEditor)
            {
                StartupTarget = StartupTarget.NONE; // Does not load anything automatically on startup.
                if (Keyboard.current != null)
                {
                    // NOTE: The new input system does not register keys before the game was started.
                    // Should probably try to use the Win32 APIs to get the keys here.
                    bool shift = IsPressed(Keys.leftShiftKey); bool ctrl = IsPressed(Keys.leftCtrlKey);
                    bool alt = IsPressed(Keys.altKey);

                    if (shift && ctrl) StartupTarget = StartupTarget.Startup;
                    else if (shift) StartupTarget = StartupTarget.Metagame;
                    else if (alt) StartupTarget = StartupTarget.Ingame;
                }
            }
        }

        if (StartupTarget == StartupTarget.Ingame)
        {
            // Load into a default scene ...
            LoadScene(StartupTarget_IngameScene);
            Log("StartupTarget was Ingame - loaded %".TM(this), StartupTarget_IngameScene);
        }
        else if (StartupTarget == StartupTarget.UNINITIALIZED)
            LogE("Uninitialized StartupTarget!".TM(this));
    }

    void CreateProps() => Props = new RhythmicGame();

    void CreateGame()
    {

    }

    // TODO: Move to a better place? Global game utilities-like (static) class?
    public static void LoadScene(string scene_name, LoadSceneMode mode = LoadSceneMode.Single) => SceneManager.LoadSceneAsync(scene_name, mode);
    public static void UnloadScene(string scene_name) {
        try { SceneManager.UnloadSceneAsync(scene_name); }
        catch (System.Exception ex) { LogWarning("UnloadScene(): %", ex.Message); }
    }

    // ----------------------------------------

    public GameMode GameMode = GameMode.Gameplay;
    public GameMatchType GameMatchType = GameMatchType.Singleplayer;

    public bool IsGamePaused = false;

    public string current_song_name = "allthetime";
}
