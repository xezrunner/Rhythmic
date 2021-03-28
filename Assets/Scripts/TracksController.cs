using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using System.Linq;
using System.Threading.Tasks;

/// New track controller
// Manages the new tracks, section creations, captures etc...

public class TracksController : MonoBehaviour
{
    [Header("Editor test variables")]
    public Vector3 TestTrackSectionPos;
    public float TestTrackSectionRot;

    public static TracksController Instance;
    public SongController SongController { get { return SongController.Instance; } }
    TrackStreamer TrackStreamer { get { return TrackStreamer.Instance; } }
    Clock Clock { get { return Clock.Instance; } }

    [Header("Common")]
    public Tunnel Tunnel;
    public AmpPlayerCatching Catching;

    [Header("Prefabs")]
    public GameObject TrackPrefab; // Change to public property?

    [Header("Properties")]
    public int CurrentRealTrackID = -1; // This is the RealID of the track that the player is currently on | -1 is none
    public int CurrentTrackID = -1; // This is the ID of the track that the player is currently on | -1 is none
    public AmpTrack CurrentTrack; // The track that the player is currently on
    public AmpTrackSection CurrentMeasure { get { return CurrentTrack.CurrentMeasure; } }

    public float LocalEmission = 1f; // Local material
    public float GlobalEmission = 1.5f; // Global material

    [Header("Variables")]
    public AmpTrack[] Tracks;
    public AmpTrack[] MainTracks;
    public AmpTrack[][] TrackSets;
    public AmpTrack[] CurrentTrackSet;
    public List<string> songTracks = new List<string>();

    // Clipping
    public ClipManager clipManager;
    GameObject lengthPlane;

    /// Functionality

    void Awake()
    {
        Instance = this; // static instance
        gameObject.layer = 11; // Assign to Tracks layer

        TrackPrefab = (GameObject)Resources.Load("Prefabs/AmpTrack");

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

        // Clip manager init

        clipManager = gameObject.AddComponent<ClipManager>();

        lengthPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        lengthPlane.name = "Length clip plane";
        lengthPlane.transform.parent = transform;
        lengthPlane.GetComponent<MeshRenderer>().enabled = false;

        // TODO: might not be needed if we force mostly straight paths or fall back to 'local' global edge lights.
        //inversePlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        //inversePlane.name = "Clip behind plane";
        //inversePlane.transform.parent = transform;
        //inversePlane.GetComponent<MeshRenderer>().enabled = false;

        clipManager.plane = lengthPlane;
        //clipManager.inverse_plane = inversePlane;

        StartCoroutine(AddTrackMaterialsToClipper());
    }

    // Shared AmpNote material (TODO: move somewhere else? Into AmpNote as static?)
    public Material SharedNoteMaterial;

    bool clipping_lastVisualControlState = false; // false: has not set materials to OFF yet | true: ignore Clip() completely
    IEnumerator AddTrackMaterialsToClipper()
    {
        while (Tracks.Last().Measures.Count < RhythmicGame.HorizonMeasures) yield return null;

        foreach (var t in Tracks)
        {
            clipManager.AddMaterial(t.Track_Bottom_Mat);
            clipManager.AddMaterial(t.Track_Bottom_Global_Mat);
        }

        // Instantiate a shared Note Material | (TODO: move somewhere else? Into AmpNote as static?)
        SharedNoteMaterial = Instantiate((Material)Resources.Load("Materials/NoteMaterial"));

        clipManager.AddMaterial(SharedNoteMaterial);

        LengthClip();
    }

    // WARNING! Paths should not traverse backwards, as the global edge lights are going to be visible ahead of the path!
    // TODO: either fix this, or design paths in a way that they don't traverse backwards!
    public void LengthClip()
    {
        // Visual clipping control - disable all clipping!
        // TODO: is comparing a static variable slow?
        if (RhythmicGame.DisableTrackLengthClipping)
        {
            if (!clipping_lastVisualControlState) // Do this once
                clipManager.materials.ForEach(m => m.SetFloat("_PlaneEnabled", 0));

            clipping_lastVisualControlState = true;
            return;
        }
        else clipping_lastVisualControlState = false;

        // Calculate clip plane offset based on measure draw distance
        float dist = AmpPlayerLocomotion.Instance.HorizonLength;

        Vector3 planePos = PathTools.GetPositionOnPath(PathTools.Path, dist);
        Quaternion planeRot = PathTools.GetRotationOnPath(PathTools.Path, dist, new Vector3(90, 0, 0));

        lengthPlane.transform.position = planePos;
        lengthPlane.transform.rotation = planeRot;

        clipManager.Clip();
    }
    void Update()
    {
        // Clipmanager update
        if (SongController.IsPlaying || AmpPlayerLocomotion.Instance.IsPlaying) LengthClip();
    }

    /// Events
    public event EventHandler<int[]> OnTrackSwitched;
    private void Tracks_OnTrackSwitched(object sender, int[] e)
    {
        //Debug.LogFormat("TRACKS <event>: Track switched from {0} to {1}", e[0], e[1]);
        AmpTrack target = Tracks[e[1]];
        CurrentTrackSet = TrackSets[target.TrackSetID];
    }

    /// Tracks
    // Track creation
    /// <summary>
    /// Creates the Tracks list. <br/>
    /// Note: this does not create the Tunnel mode duplicated tracks!
    /// </summary>
    void CreateTracks()
    {
        // Initialize Tracks arrays:
        Tracks = new AmpTrack[songTracks.Count * RhythmicGame.TunnelTrackDuplicationNum];
        MainTracks = new AmpTrack[songTracks.Count];

        // Initialize Track sets arrays:
        TrackSets = new AmpTrack[RhythmicGame.TunnelTrackDuplicationNum][];
        for (int i = 0; i < TrackSets.Length; i++)
            TrackSets[i] = new AmpTrack[songTracks.Count];

        // Create tracks - populate Tracks array:
        int counter = 0;
        for (int i = 0; i < RhythmicGame.TunnelTrackDuplicationNum; i++)
        {
            for (int x = 0; x < songTracks.Count; x++)
            {
                string trackName = songTracks[x];
                bool isCloneTrack = i > 0;
                var inst = AmpTrack.InstrumentFromString(trackName);

                var track = AmpTrack.CreateTrack(x, trackName, inst, counter, isCloneTrack, i);

                Tracks[counter] = track;
                if (i == 0) MainTracks[counter] = track;
                TrackSets[i][x] = track;

                counter++;
            }
        }

        // Assign twins in Tunnel mode:
        if (RhythmicGame.IsTunnelTrackDuplication)
        {
            int duplCount = RhythmicGame.TunnelTrackDuplicationNum; // 3
            for (int i = 0, z = 0; i < Tracks.Length; i++, z++) // i: 18 | z: 6
            {
                if (z >= MainTracks.Length) z = 0; // Reset main track counter

                AmpTrack t = Tracks[i];
                t.TrackTwins = new AmpTrack[duplCount - 1];
                for (int x = 0, y = 0; x < TrackSets.Length; x++) // x: 3
                {
                    // Assign those tracksets' tracks, which are not identical to current track set (ignore self)
                    if (t.TrackSetID != x)
                        t.TrackTwins[y] = TrackSets[x][z];
                    else continue;

                    y++;
                }
            }
        }
    }

    /// Track states
    public void SetTrackState(int id, bool state) => SetTrackState(Tracks[id], state);
    public void SetTrackState(AmpTrack track, bool state) => track.IsEnabled = state;

    public void DisableCurrentMeasures(bool current = false)
    {
        foreach (AmpTrack t in Tracks)
        {
            if (!current && t.ID == CurrentTrack.ID) continue; // Ignore current track
            t.Measures[Clock.Fbar].IsEnabled = false;
            t.IsTrackBeingPlayed = false;
        }
    }

    /// Sequences

    // This is an array of upcoming notes that the player is supposed to catch
    public AmpNote[] targetNotes;

    [NonSerialized]
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
            //Debug.Break();
            //Debug.DebugBreak();

            if (!lastRefreshUpcomingState)
                RefreshSequences(track);

            AmpNote note = null;

            // Find next note to catch!
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

            if (!note)
                Debug.LogError($"Tracks/RefreshTargetNotes({track.ID}): upcoming note was null!"); 

            note.Color = Color.green;
            targetNotes[track.ID] = note;

            /*
            { // debug log targetNotes:
                string s = "targetNotes: ";
                foreach (AmpNote n in targetNotes)
                    s += $"[{n.ID}{$"/{n.TrackID}".AddColor(.5f)}]  ";
                Logger.Log(s);
            }
            */
        }

        foreach (AmpTrack t in MainTracks)
        {
            if (track && t.ID == track.ID) continue; // Ignore specified track
            if (track && lastRefreshUpcomingState) break; // If we already refreshed, skip
            if (t.Sequences.Count == 0) // No sequences were found in this track!
            {
                Debug.LogWarning($"Tracks/RefreshTargetNotes(): Track {t.TrackName} [{t.RealID}] has no sequences! No target notes for this track.");
                targetNotes[t.ID] = null; // Set this track's targetNote to null for the time being
                continue;
            }

            AmpNote note = t.Sequences[0].Notes[0];
            if (!note) { Debug.LogError($"Tracks/RefreshTargetNotes({track == null}): couldn't find the first note for track {t.ID} sequence [0]"); Debug.Break(); System.Diagnostics.Debugger.Break(); }

            note.Color = Color.green;
            targetNotes[t.ID] = note;

            if (RhythmicGame.DebugTargetNoteRefreshEvents)
            {
                string endMarker = (t.ID == 0 || t.ID == MainTracks.Length - 1) ? "  ******" : "";
                Debug.Log($"RefreshTargetNotes(): Target note for {t.TrackName}: {note.name}" + endMarker);
            }
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
        int sequenceNum = RhythmicGame.SequenceAmount;
        if (sequenceNum < 1) { Debug.LogError("Tracks: There cannot be less than 1 measures set as sequences!"); return; }

        foreach (AmpTrack t in MainTracks)
        {
            if (track && t.ID == track.ID) continue;

            t.Sequences.ForEach(t => t.IsSequence = false); // Make previous sequences inactive
            t.Sequences.Clear();

            int currentMeasure;
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

            t.UpdateSequenceStates();

            if (RhythmicGame.DebugSequenceRefreshEvents)
            {
                if (t.Sequences.Count == 0) { Debug.LogWarning($"RefreshSequences(): Sequences for {t.TrackName}: null"); return; }

                string seq_string = "";

                foreach (var m in t.Sequences) seq_string += m.ID + ", ";
                seq_string = seq_string.Substring(0, seq_string.Length - 2); // Remove final trailing ', '

                string endMarker = (t.ID == 0 || t.ID == MainTracks.Length - 1) ? "  ******" : ""; // Mark final line
                Debug.Log($"RefreshSequences(): Sequences for {t.TrackName}: {seq_string}" + endMarker);
            }
        }
    }

    // TODO: global WaitForLoading or something
    [NonSerialized] public bool IsLoaded;
    IEnumerator RefreshSequences_Init()
    {
        while (Tracks[Tracks.Length - 1].Measures.Count < RhythmicGame.HorizonMeasures)
            yield return null;

        IsLoaded = true;

        RefreshSequences();
        RefreshTargetNotes();
    }

    /// Track switching
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
        foreach (AmpTrack t in MainTracks)
            t.SetIsTrackFocused(track.RealID); // Focused state is whether t's ID is the same as the requested track's ID

        if (RhythmicGame.DebugPlayerTrackSwitchEvents)
            Debug.LogFormat("TRACKS: Track switched to {0} [{1}]", track.RealID, track.name);

        // Invoke event!
        OnTrackSwitched?.Invoke(this, eventArgs);
    }
    public void SwitchToTrack(int ID) => SwitchToTrack(Tracks[ID]);

    /// Measure capturing
    /// <summary>
    /// Captures measures from a given start point until an end point
    /// </summary>
    /// <param name="start">Measure ID to start capturing from</param>
    /// <param name="end">Last Measure ID to capture</param>
    public void CaptureMeasureRange(int start, int end, int id)
    {
        foreach (AmpTrack[] set in TrackSets)
            StartCoroutine(_CaptureMeasureRange(start, end, set[id]));
    }
    public void CaptureMeasureRange(int start, int end, AmpTrack track) => CaptureMeasureRange(start, end, track.ID);
    public void CaptureMeasureRange(int start, int end, AmpTrack[] tracks)
    {
        foreach (AmpTrack t in tracks)
            CaptureMeasureRange(start, end, t);

        RefreshSequences();
        RefreshTargetNotes();
    }

    /// <summary>
    /// Captures measures from a given start point and onward
    /// </summary>
    /// <param name="start">Measure ID to start capturing from</param>
    /// <param name="amount">Amount of measures to capture from starting point onward</param>
    public void CaptureMeasureAmount(int start, int amount, AmpTrack track) => CaptureMeasureRange(start, start + amount, track);
    public void CaptureMeasureAmount(int start, int amount, int trackID) => CaptureMeasureRange(start, start + amount, MainTracks[trackID]);
    public void CaptureMeasureAmount(int start, int amount, AmpTrack[] tracks) => CaptureMeasureRange(start, start + amount, tracks);

    IEnumerator _CaptureMeasureRange(int start, int end, AmpTrack track)
    {
        if (RhythmicGame.DebugTrackCapturingEvents) Debug.Log($"CAPTURE: started | start: {start}, end: {end}, track: {track.TrackName} | {track.RealID} ");

        // TODO: state enum like in AmpTrackSection?
        track.IsTrackCapturing = true;
        track.IsTrackCaptured = true;

        // Immediately consider all measures as captured (isCaptured returns true even when capturing)
        for (int i = start; i < end; i++)
        {
            if (i >= track.Measures.Count) continue;
            AmpTrackSection m = track.Measures[i];
            if (!m) continue;

            if (m.CaptureState != MeasureCaptureState.Captured)
                m.CaptureState = MeasureCaptureState.Capturing;

            m.IsSequence = false;
        }

        // Init capture process - wait for captures to finish before proceeding to next one
        for (int i = start; i < end; i++)
        {
            if (i < track.Measures.Count && track.Measures[i])
                yield return track.CaptureMeasure(track.Measures[i]);
            else // This measure doesn't yet exist - change meta measure to captured state in TrackStreamer!
                TrackStreamer.metaMeasures[track.ID][i].IsCaptured = true;
        }

        track.IsTrackCapturing = false;
        track.captureAnimStep = 0.85f; // Reset easing anim step value for specific track

        if (RhythmicGame.DebugTrackCapturingEvents) Debug.Log("CAPTURE: done");
    }
}