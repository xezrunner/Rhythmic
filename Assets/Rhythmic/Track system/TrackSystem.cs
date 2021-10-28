using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using PathCreation;

using static Logger;
using System.Collections.Generic;

public class TrackSystem : MonoBehaviour
{
    public static TrackSystem Instance;
    public GameVariables Vars;
    SongSystem SongSystem;
    Song song;

    public PathCreator pathcreator;
    public WorldSystem worldsystem;
    public TrackStreamer streamer;

    public XZ_Path path;

    public List<Track> Tracks = new List<Track>();

    public void Awake()
    {
        Instance = this;
        Vars = GameState.Variables; // class by reference, so this does reflect changes.

        path = new XZ_Path(pathcreator, false);
        if (!streamer) GetComponent<TrackStreamer>();
        if (!streamer) streamer = gameObject.AddComponent<TrackStreamer>();
    }
    void Start()
    {
        SongSystem = SongSystem.Instance;
        song = SongSystem.song;
        Log("Initialized with song: %".T(this), song.name);
        INIT_CreateTracks();
    }

    void INIT_CreateTracks()
    {
        for (int i = 0; i < song.data.track_defs.Count; ++i)
        {
            string s = song.data.track_defs[i];
            Track t = Track.CreateTrack(s, (Instrument)i, i, i);
            Tracks.Add(t);
        }
    }

    void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
            SceneManager.LoadScene("test0");
    }
}
