using UnityEngine;
using System.Collections.Generic;
using static Logger;

public partial class TrackStreamer : MonoBehaviour
{
    public static TrackStreamer Instance;

    Clock Clock;
    SongSystem SongSystem;
    Song song;

    GameVariables Vars;

    public TrackSystem TrackSystem;
    public Transform trans;

    [Header("Prefabs")]
    public GameObject Track_Prefab;

    public List<Track> tracks = new();

    public void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        SongSystem = SongSystem.Instance;
        Clock = Clock.Instance;
        Vars = GameState.Variables;
        TrackSystem = TrackSystem.Instance;
        tracks = TrackSystem.Tracks;

        song = SongSystem.song;
        Log("Initialized track streamer for song %.".T(this), song.name);

        STREAMER_Init();
    }

    void STREAMER_Init()
    {
        // Prepare measures based on horizon: 
        Stream(Clock.bar + Vars.horizon_bars, -1);
    }

    /// <summary>Stream in an amount of bars from the current bar position.</summary>
    void Stream(int count, int track_id)
    {
        int max_horizon = (Clock.bar + Vars.horizon_bars);
        int target = (Clock.bar + count);

        if (target > max_horizon)
        {
            LogW("Cannot stream beyond horizon!  horizon: %, requested target:% ", max_horizon, target);
            return;
        }

        if (track_id == -1)
        {
            for (int i = 0; i < TrackSystem.Tracks.Count; ++i)
                Stream(count, i);
            return;
        }

        for (int i = 0; i < count; ++i)
            Measure.CreateMeasure(TrackSystem.Tracks[track_id], i);
    }
}
