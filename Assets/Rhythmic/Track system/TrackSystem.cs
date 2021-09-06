using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using PathCreation;

using static Logger;

public class TrackSystem : MonoBehaviour
{
    public static TrackSystem Instance;
    public GameVariables Vars;

    public PathCreator pathcreator;
    public WorldSystem worldsystem;

    public XZ_Path path;

    public void Awake()
    {
        Instance = this;
        Vars = GameState.Variables; // class by reference, so this does reflect changes.

        path = new XZ_Path(pathcreator, false);
    }

    void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
            SceneManager.LoadScene("test0");
    }
}
