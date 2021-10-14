using UnityEngine;
using System.Collections.Generic;
using static Logger;

public partial class TrackStreamer : MonoBehaviour
{
    public static TrackStreamer Instance;

    Clock Clock = Clock.Instance;
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
        Vars = GameState.Variables;
        TrackSystem = TrackSystem.Instance;
    }

    public void Start()
    {
        SongSystem = SongSystem.Instance;
        song = SongSystem.song;
        Log("Initialized track streamer for song %.".T(this), song.name);

        STREAMER_Init();
    }

    void STREAMER_Init()
    {
        // Create the tracks: 
        for (int i = 0; i < song.data.track_defs.Count; ++i)
        {
            string s = song.data.track_defs[i];
            Track t = Track.CreateTrack(s, (Instrument)i, i, i);
            tracks.Add(t);
        }
        TrackSystem.Tracks = tracks; // TODO: is this by reference?

        // Prepare measures based on horizon: 

    }

    /// <summary>Stream in an amount of bars from the current bar position.</summary>
    void Stream(int count, int track_id)
    {
        int max_horizon = (Clock.bar + Vars.horizon_bars);

    }
}
