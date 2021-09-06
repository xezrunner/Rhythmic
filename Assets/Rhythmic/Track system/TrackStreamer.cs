using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using static Logger;

public partial class TrackStreamer : MonoBehaviour
{
    public static TrackStreamer Instance;
    GameVariables Vars;

    public Transform trans;

    [Header("Prefabs")]
    public GameObject Track_Prefab;

    public bool perftest_mode = false;

    public void Awake()
    {
        Instance = this;
        Vars = GameState.Variables;
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
