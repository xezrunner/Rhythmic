using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using PathCreation;

using static Logger;

public class TrackSystem : MonoBehaviour
{
    public static TrackSystem Instance;
    public GameVariables Vars;

    public PathCreator pathcreator;
    public WorldSystem worldsystem;

#if UNITY_EDITOR
    public VertexPath path { get { return pathcreator.path; } }
#else
    public VertexPath path;
#endif

    public void Awake()
    {
        Instance = this;
        Vars = GameState.Variables; // class by reference, so this does reflect changes.

        worldsystem = WorldSystem.Instance;
        pathcreator = WorldSystem.GetAPathCreator();
        if (!pathcreator && LogE("No pathcreator!".T(this))) return;

        // Changes to the path are reflected in debug builds:
#if !UNITY_EDITOR
        path = pathcreator.path;
#endif
    }

    void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
            SceneManager.LoadScene("test0");
    }
}
