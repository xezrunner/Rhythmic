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

    public void Awake()
    {
        Instance = this;
        Vars = GameState.Variables;
    }

    public void Start()
    {
        //Test();
    }

    public void Update()
    {
        UPDATE_Test();
    }
}
