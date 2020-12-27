using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

/// New track controller
// Manages the new tracks, section creations, captures etc...

public class TracksController : MonoBehaviour
{
    [Header("Editor test variables")]
    public Vector3 TestTrackSectionPos;
    public float TestTrackSectionRot;

    public static TracksController Instance;
    public SongController SongController { get { return SongController.Instance; } }

    [Header("Common")]
    public Tunnel Tunnel;
    public AmpPlayerCatching Catching;

    PathCreator pathCreator;

    [Header("Prefabs")]
    GameObject trackPrefab; // Change to public property?

    [Header("Variables")]
    public List<AmpTrack> Tracks = new List<AmpTrack>();
    public List<string> songTracks = new List<string>();
    public List<Dictionary<int, MetaMeasure>> metaMeasures
    {
        get { return SongController.metaMeasures; }
        set { SongController.metaMeasures = value; }
    }

    [Header("Properties")]
    public int CurrentRealTrackID = -1; // This is the RealID of the track that the player is currently on | -1 is none
    public int CurrentTrackID = -1; // This is the ID of the track that the player is currently on | -1 is none
    public AmpTrack CurrentTrack; // The track that the player is currently on

    /// Events
    public event EventHandler<int[]> OnTrackSwitched;

    /// Functionality

    void Awake()
    {
        Instance = this; // static instance
        gameObject.layer = 11; // Assign to Tracks layer

        pathCreator = GameObject.Find("Path").GetComponent<PathCreator>();
        trackPrefab = (GameObject)Resources.Load("Prefabs/AmpTrack");

        OnTrackSwitched += Tracks_OnTrackSwitched;

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

        // Create Tunnel component (must have songTracks ready before this!)
        Tunnel = gameObject.AddComponent<Tunnel>();
        Tunnel.Init(songTracks.Count * RhythmicGame.TunnelTrackDuplicationNum);

        // Create tracks!
        CreateTracks();

        StartCoroutine(RefreshSequences_Init());
    }

    private void Tracks_OnTrackSwitched(object sender, int[] e)
    {
        Debug.LogFormat("TRACKS: Track switched from {0} to {1}", e[0], e[1]);
    }

    /// Tracks
    // Track creation
    /// <summary>
    /// Creates the Tracks list. <br/>
    /// Note: this does not create the Tunnel mode duplicated tracks!
    /// </summary>
    void CreateTracks()
    {
        int counter = 0;
        for (int i = 0; i < RhythmicGame.TunnelTrackDuplicationNum; i++)
        {
            for (int x = 0; x < songTracks.Count; x++)
            {
                string trackName = songTracks[x];
                var inst = AmpTrack.InstrumentFromString(trackName);

                CreateTrack(x, trackName, inst, counter);
                counter++;
            }
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

    // Track states
    public void SetTrackState(int id, bool state) => SetTrackState(Tracks[id], state);
    public void SetTrackState(AmpTrack track, bool state) => track.IsEnabled = state;

    // Sequences
    IEnumerator RefreshSequences_Init()
    {
        while (Tracks[Tracks.Count - 1].Measures.Count < 2)
            yield return null;

        RefreshSequences();
        Catching.RefreshTargetNotes();
    }

    /// <summary>
    /// Finds the next sequences in all tracks. <br/>
    /// Populates the Sequences list in AmpTracks with measures.
    /// </summary>
    public void RefreshSequences()
    {
        //if (Tracks[Tracks.Count - 1].Measures.Count < 2)
        //    await Task.Delay(TimeSpan.FromSeconds(2));

        int sequenceNum = RhythmicGame.SequenceAmount;
        if (sequenceNum < 1) { Debug.LogError("Tracks: There cannot be less than 1 measures set as sequences!"); return; }

        foreach (AmpTrack t in Tracks)
        {
            foreach (AmpTrackSection m in t.Measures)
            {
                if (m.ID < SongController.songCountIn) continue; // If the measures are part of countin, don't consider them sequences.
                if (m.IsEmpty || m.IsCaptured) continue;

                t.Sequences.Add(m);

                if (t.Sequences.Count == sequenceNum) break;
            }

            t.UpdateSequenceColors();
        }
    }

    // Track switching
    /// <summary>
    /// Switches the track. <br/>
    /// This handles setting the new track ID and preparing the tracks for their focus states.
    /// </summary>
    public void SwitchToTrack(AmpTrack track)
    {
        // Prepare event args
        int[] eventArgs = new int[2] { CurrentTrackID, track.ID };

        // Set track variables
        CurrentTrack = track;
        CurrentTrackID = track.ID;
        CurrentRealTrackID = track.RealID;

        // Handle focus states
        foreach (AmpTrack t in Tracks)
            t.IsTrackFocused = (t.RealID == track.RealID); // Focused state is whether t's ID is the same as the requested track's ID

        if (RhythmicGame.DebugPlayerTrackSwitchEvents)
            Debug.LogFormat("TRACKS: Track switched to {0} [{1}]", track.ID, track.TrackName != "" ? track.TrackName : track.name);

        track.UpdateSequenceColors();

        // Invoke event!
        OnTrackSwitched?.Invoke(this, eventArgs);
    }
    public void SwitchToTrack(int ID) => SwitchToTrack(Tracks[ID]);

    // Measure capturing
    /// <summary>
    /// Captures measures from a given start point until an end point
    /// </summary>
    /// <param name="start">Measure ID to start capturing from</param>
    /// <param name="end">Last Measure ID to capture</param>
    public void CaptureMeasureRange(int start, int end, AmpTrack track) => StartCoroutine(_CaptureMeasureRange(start, end, track));

    /// <summary>
    /// Captures measures from a given start point and onward
    /// </summary>
    /// <param name="start">Measure ID to start capturing from</param>
    /// <param name="amount">Amount of measures to capture from starting point onward</param>
    public void CaptureMeasureAmount(int start, int amount, AmpTrack track) => StartCoroutine(_CaptureMeasureRange(start, start + amount, track));
    public void CaptureMeasureAmount(int start, int amount, int trackID) => CaptureMeasureRange(start, start + amount, Tracks[trackID]);

    IEnumerator _CaptureMeasureRange(int start, int end, AmpTrack track)
    {
        if (RhythmicGame.DebugTrackCapturingEvents) Debug.Log($"CAPTURE: started | start: {start}, end: {end}, track: {track.TrackName} | {track.RealID} ");

        track.IsTrackCapturing = true;

        for (int i = start; i < end; i++)
        {
            if (i < track.Measures.Count)
                yield return track.CaptureMeasure(track.Measures[i]);
            else // This measure doesn't yet exist - change meta measure to captured state!
                metaMeasures[track.RealID][i].IsCaptured = true;
        }

        track.IsTrackCapturing = false;
        AmpTrackSectionDestruct.step = 0.5f;

        if (RhythmicGame.DebugTrackCapturingEvents) Debug.Log("CAPTURE: done");
    }
}