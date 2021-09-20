using UnityEngine;
using System.Collections.Generic;

public partial class TrackStreamer : MonoBehaviour
{
    public static TrackStreamer Instance;
    GameVariables Vars;

    public TrackSystem TrackSystem;
    public Transform trans;

    [Header("Prefabs")]
    public GameObject Track_Prefab;

    public bool perftest_mode = false;

    public List<Track> tracks = new();

    public void Awake()
    {
        Instance = this;
        Vars = GameState.Variables;
        TrackSystem = TrackSystem.Instance;
    }

    public void Start()
    {
        if (perftest_mode)
        {
            PerfTest();
            return;
        }
    }

    public void Update()
    {
        UPDATE_Test();
    }
}
