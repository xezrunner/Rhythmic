using UnityEngine;
using System.Collections.Generic;
using static Logger;

public partial class TrackStreamer : MonoBehaviour
{
    SongSystem SongSystem;
    Song song;

    public static TrackStreamer Instance;
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
        for (int i = 0; i < song.data.track_defs.Count; ++i)
        {
            string s = song.data.track_defs[i];
            Track t = Track.CreateTrack(s, Instrument.Bass, i, i);
            tracks.Add(t);
        }
    }
}
