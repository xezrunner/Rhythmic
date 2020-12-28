using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using NUnit.Framework;
using System.Linq;

/// New track controller
// Manages the new tracks, section creations, captures etc...

public class TracksController : MonoBehaviour
{
    [Header("Editor test variables")]
    public Vector3 TestTrackSectionPos;
    public float TestTrackSectionRot;

    public static TracksController Instance;
    public SongController SongController { get { return SongController.Instance; } }
    Clock Clock { get { return Clock.Instance; } }

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
    public AmpTrackSection CurrentMeasure { get { return CurrentTrack.CurrentMeasure; } }

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

    public void DisableCurrentMeasures(bool current = false)
    {
        foreach (AmpTrack t in Tracks)
        {
            if (t == CurrentTrack & !current) continue;
            t.Measures[Clock.Fbar].IsEnabled = false;
            t.IsTrackBeingPlayed = false;
        }
    }

    // Sequences

    // This is an array of upcoming notes that the player is supposed to catch
    public AmpNote[] targetNotes;

    [HideInInspector]
    public bool lastRefreshUpcomingState; // This is set to true when we've already found the upcoming notes for the other tracks.
    /// <summary>
    /// Finds the next notes that the player is supposed to catch. <br/>
    /// In case we're already playing a track, this will only update the current track's upcoming notes.
    /// </summary>
    /// <param name="track">Giving a track will skip forward a sequence amount of measures for the other tracks.</param>
    public void RefreshTargetNotes(AmpTrack track = null)
    {
        if (track)
        {
            if (!lastRefreshUpcomingState)
                RefreshSequences(track);

            AmpNote note = null;

            foreach (AmpTrackSection m in track.Sequences)
            {
                foreach (AmpNote n in m.Notes)
                    if (!n.IsCaptured & n.IsEnabled)
                    {
                        note = n;
                        break;
                    }
                if (note) break;
            }

            if (!note) { Debug.LogError($"Tracks/RefreshTargetNotes({track.ID}): upcoming note was null!"); Debug.Break(); System.Diagnostics.Debugger.Break(); }

            note.NoteMeshRenderer.material.color = Color.green;
            targetNotes[track.ID] = note;
        }

        foreach (AmpTrack t in Tracks)
        {
            if (track && t == track) continue; // Ignore specified track
            if (track && lastRefreshUpcomingState) break; // If we already refreshed, skip
            if (t.IsTrackCaptured) continue; // Ignore tracks that have been captured
            if (t.Sequences.Count == 0) { Debug.LogWarning($"Tracks/RefreshTargetNotes(): Track {t.TrackName} [{t.RealID}] has no sequences! No target notes for this track."); continue; }

            AmpNote firstNote = t.Sequences[0].Notes[0];
            if (!firstNote) { Debug.LogError($"Tracks/RefreshTargetNotes({track == null}): couldn't find the first note for track {t.ID} sequence [0]"); Debug.Break(); System.Diagnostics.Debugger.Break(); }

            firstNote.NoteMeshRenderer.material.color = Color.green;
            targetNotes[t.ID] = firstNote;
        }

        if (track && !lastRefreshUpcomingState) lastRefreshUpcomingState = true;
        else if (!track & lastRefreshUpcomingState) lastRefreshUpcomingState = false;
    }

    /// <summary>
    /// Finds the next sequences in all tracks. <br/>
    /// Populates the Sequences list in AmpTracks with measures.
    /// </summary>
    public void RefreshSequences(AmpTrack track = null)
    {
        if (lastRefreshUpcomingState) return;

        int sequenceNum = RhythmicGame.SequenceAmount;
        if (sequenceNum < 1) { Debug.LogError("Tracks: There cannot be less than 1 measures set as sequences!"); return; }

        foreach (AmpTrack t in Tracks)
        {
            if (track & t == track) continue;

            t.Sequences.Clear();

            //int currentMeasure = Clock.Fbar + (track ? 1 + sequenceNum : 1); // Jump ahead if track specified
            int currentMeasure = 0;
            if (track) currentMeasure = track.Sequences.Last().ID + 1;
            else if (t.IsTrackCaptured) currentMeasure = Clock.Fbar + RhythmicGame.TrackCaptureLength;
            else currentMeasure = Clock.Fbar;

            for (int i = currentMeasure; i < SongController.songLengthInMeasures; i++)
            {
                if (i >= t.Measures.Count) break;
                if (t.Sequences.Count == sequenceNum) break;

                if (t.Measures[i] == null) continue;
                var m = t.Measures[i];

                if (m.IsEmpty || m.IsCaptured || !m.IsEnabled) // Not eligible measures
                    if (t.Sequences.Count > 0) break;
                    else continue;

                t.Sequences.Add(m);
            }

            t.UpdateSequenceColors();
        }
    }

    IEnumerator RefreshSequences_Init()
    {
        while (Tracks[Tracks.Count - 1].Measures.Count < 2)
            yield return null;

        RefreshSequences();
        RefreshTargetNotes();
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
        track.IsTrackCaptured = true;

        for (int i = start; i < end; i++)
        {
            if (i < track.Measures.Count)
                yield return track.CaptureMeasure(track.Measures[i]);
            else // This measure doesn't yet exist - change meta measure to captured state!
                metaMeasures[track.RealID][i].IsCaptured = true;
        }

        track.IsTrackCapturing = false;
        track.captureAnimStep = 0.85f;

        if (RhythmicGame.DebugTrackCapturingEvents) Debug.Log("CAPTURE: done");
    }
}