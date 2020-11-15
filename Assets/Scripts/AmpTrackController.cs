using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

/// New track controller
// Manages the new tracks, section creations, captures etc...

public class AmpTrackController : MonoBehaviour
{
    #region Editor test variables
    public Vector3 TestTrackSectionPos;
    public float TestTrackSectionRot;
    #endregion

    public static AmpTrackController Instance;
    public SongController SongController { get { return SongController.Instance; } }
    Player Player { get { return Player.Instance; } }

    public Tunnel Tunnel;

    GameObject trackPrefab;

    public List<AmpTrack> Tracks = new List<AmpTrack>();
    public List<string> songTracks = new List<string>();

    /// Events
    public event EventHandler<int[]> OnTrackSwitched;

    PathCreator pathCreator;

    void Awake()
    {
        Instance = this; // static instance
        gameObject.layer = 11; // Assign to Tracks layer

        pathCreator = GameObject.Find("Path").GetComponent<PathCreator>();
        trackPrefab = (GameObject)Resources.Load("Prefabs/AmpTrack");

        Player.OnTrackSwitched += Player_OnTrackSwitched; // wire up Player track switching event
    }
    void Start()
    {
        // Create a list of playable song tracks in string form
        // Used for tunnel and track creation
        songTracks.Clear();
        foreach (string s in SongController.songTracks)
        {
            var inst = AmpTrack.InstrumentFromString(s);

            if (inst == AmpTrack.InstrumentType.FREESTYLE & !RhythmicGame.PlayableFreestyleTracks) continue;
            if (inst == AmpTrack.InstrumentType.bg_click) continue;
            songTracks.Add(s);
        }

        // Create Tunnel component
        Tunnel = gameObject.AddComponent<Tunnel>();
        Tunnel.Init(songTracks.Count * RhythmicGame.TunnelTrackDuplicationNum);

        // Create tracks!
        CreateTracks(SongController.songTracks);
    }

    /// Tracks
    
    /// <summary>
    /// Creates the Tracks list. <br/>
    /// Note: this does not create the Tunnel mode duplicated tracks!
    /// </summary>
    void CreateTracks(List<string> songTracks)
    {
        int counter = 0;
        foreach (string trackName in this.songTracks)
        {
            var inst = AmpTrack.InstrumentFromString(trackName);

            CreateTrack(counter, trackName, inst);
            counter++;
        }
    }
    public AmpTrack CreateTrack(int ID, string name, AmpTrack.InstrumentType instrument, int? realID = null)
    {
        //GameObject trackObject = new GameObject() { name = name };
        var obj = Instantiate(trackPrefab, gameObject.transform);
        obj.name = name;

        // Add Track component:
        AmpTrack com = obj.GetComponent<AmpTrack>();

        // TODO: temp - assign PathCreator here until it isn't global

        com.PathCreator = pathCreator;
        com.ID = ID;
        com.RealID = realID.HasValue ? realID.Value : ID; // Assign the same ID if realID was not desired
        com.TrackName = name;
        com.Instrument = instrument;

        Tracks.Add(com);
        return com;
    }

    private void Player_OnTrackSwitched(object sender, Track e)
    {

    }
}
