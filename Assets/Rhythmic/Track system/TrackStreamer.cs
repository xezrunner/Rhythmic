using UnityEngine;
using System.Collections.Generic;
using static Logger;

/// TODO:
// - We should calculate an ideal value for stream inst delay based on the framerate, with a min limit to not miss streaming.

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
        Stream(0, Clock.bar + Vars.horizon_bars, -1, enqueue: false);
    }

    /// <summary>Stream in an amount of bars from the current bar position.</summary>
    void Stream(int from, int to, int track_id, bool enqueue = true)
    {
        for (int i = from; i < to; ++i)
            Stream(i, track_id, enqueue);
    }
    void Stream(int measure_id, int track_id, bool enqueue = true)
    {
        int max_horizon = (Clock.bar + Vars.horizon_bars);

        if (measure_id > max_horizon)
        {
            LogW("Cannot stream beyond horizon!  horizon: %, requested target:% ", max_horizon, measure_id);
            return;
        }

        if (track_id == -1)
        {
            for (int i = 0; i < TrackSystem.Tracks.Count; ++i)
                Stream(measure_id, i, enqueue);
            return;
        }

        // Ignore already streamed in measures: 
        if (TrackSystem.Tracks[track_id].measures[measure_id] != null)
            return;

        if (enqueue)
            queue.Add((measure_id, track_id));
        else
            Measure.CreateMeasure(TrackSystem.Tracks[track_id], measure_id);
    }

    List<(int, int)> queue = new List<(int, int)>();

    int last_bar;
    float elapsed_ms;
    void Update()
    {
        //if (!SongSystem.is_playing) return;

        if (last_bar != Clock.bar)
        {
            // Stream in the current bar:
            Stream(Clock.bar, -1);
            last_bar = Clock.bar;
        }

        if (queue.Count == 0) return;

        elapsed_ms += Time.deltaTime * 1000;
        if (elapsed_ms < Vars.stream_inst_delay_ms) return;
        else
        {
            elapsed_ms = 0;
            LogW("elapsed_ms was reset!".TM(this));
        }


        (int, int) a = queue[0];
        Stream(a.Item1, a.Item2, enqueue: false);
        queue.RemoveAt(0);
    }
}
