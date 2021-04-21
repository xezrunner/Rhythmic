using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Assertions;

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
    public GameObject SeekerPrefab;

    [Header("Properties")]
    public int CurrentRealTrackID = -1; // This is the RealID of the track that the player is currently on | -1 is none
    public int CurrentTrackID = -1; // This is the ID of the track that the player is currently on | -1 is none
    public AmpTrack CurrentTrack; // The track that the player is currently on
    public AmpTrackSection CurrentMeasure { get { return CurrentTrack.CurrentMeasure; } }

    public float LocalEmission = 1f; // Local material
    public float GlobalEmission = 1.5f; // Global material

    [Header("Variables")]
    public AmpTrack[] MainTracks;
    public AmpTrack[] Tracks;
    public int MainTracks_Count = 0;
    public int Tracks_Count = 0;

    public AmpTrack[][] TrackSets;
    public AmpTrack[] CurrentTrackSet;
    public int CurrentTrackSetID;
    public List<string> songTracks = new List<string>();

    // Clipping
    public ClipManager clipManager;
    GameObject lengthPlane;

    // Seeker
    public static bool SeekerEnabled = true;
    public GameObject Seeker;
    public MeshRenderer SeekerRenderer;
    public MeshFilter SeekerMesh;
    static Vector3[] og_vertsSeeker;

    /// Functionality

    void Awake()
    {
        Instance = this; // static instance
        gameObject.layer = 11; // Assign to Tracks layer

        TrackPrefab = (GameObject)Resources.Load("Prefabs/AmpTrack");
        SeekerPrefab = (GameObject)Resources.Load("Models/seeker_frame");

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
        Tracks_Count = Tracks.Length;
        MainTracks_Count = MainTracks.Length;

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

        // Instantiate seeker
        Seeker = Instantiate(SeekerPrefab);
        SeekerMesh = Seeker.GetComponent<MeshFilter>();
        SeekerRenderer = Seeker.GetComponent<MeshRenderer>();
        SeekerRenderer.material = (Material)Resources.Load("Materials/EdgeLightMaterial");

        if (og_vertsSeeker == null)
        {
            og_vertsSeeker = new Vector3[SeekerMesh.mesh.vertices.Length];
            SeekerMesh.mesh.vertices.CopyTo(og_vertsSeeker, 0);
        }

        StartCoroutine(_Seeker_Init_WaitForStart());
    }

    int seeker_lastTrackID = -1;
    int seeker_lastSequenceID = -1;
    void DeformSeeker(AmpTrackSection m, int count = 1)
    {
        if (!SeekerEnabled) Seeker.SetActive(false);

        if (!m.IsEnabled || !m.IsSequence || m.IsEmpty || m.IsCaptured) Seeker.SetActive(false);
        else Seeker.SetActive(true);

        MeshDeformer.DeformMesh(PathTools.Path, SeekerMesh.mesh, m.Position, m.Rotation, ogVerts: og_vertsSeeker, offset: new Vector3(0, 0.018f, 0), RhythmicGame.TrackWidth + 0.05f, -1, m.Length * count, movePivotToStart: false); // TODO: unneccessary parameters
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
        SharedNoteMaterial = Instantiate((Material)Resources.Load("Materials/note-test/note_material"));

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
        CurrentTrackSetID = target.TrackSetID;
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

    public void DisableCurrentMeasures(bool current = false, int id = -1)
    {
        int measure = (id == -1) ? Clock.Fbar : id;
        foreach (AmpTrack t in MainTracks)
        {
            if (!current && t.ID == CurrentTrack.ID) continue; // Ignore current track
            t.Measures[measure].SetIsEnabled(false);
            t.SetIsTrackBeingPlayed(false);
        }
    }

    /// Sequences

    // This is an array of upcoming notes that the player is supposed to catch
    public AmpNote[] targetNotes;
    public void SetTargetNote(int track_id, AmpNote note)
    {
        targetNotes[track_id] = note;
        //note.Color = Color.green; // TODO: TEMP!
    }

    [NonSerialized]
    public bool lastRefreshUpcomingState; // This is set to true when we've already found the upcoming notes for the other tracks.
    /// <summary>
    /// Finds the next notes that the player is supposed to catch. <br/>
    /// In case we're already playing a track, this will only update the current track's upcoming notes.
    /// </summary>
    /// <param name="c_track">Giving a track will skip forward a sequence amount of measures for the other tracks.</param>
    public void RefreshTargetNotes(AmpTrack c_track = null)
    {
        // Find notes:
        for (int t = 0; t < MainTracks_Count; t++)
        {
            AmpTrack track = MainTracks[t];
            if (c_track && t == c_track.ID) continue; // Ignore current track

            if (track.Sequences.Count == 0 || track.Sequences[0].Notes.Count == 0) // No sequences or no notes in them - META
            {
                // It's probably far away and we don't have it streamed in yet.
                // Mark as target note in the meta notes.

                // TODO: better checks here!
                if ((track.Sequence_Start_IDs[0] == -1) ||
                    (SongController.songNotes[t, track.Sequence_Start_IDs[0]] == null) ||
                    (SongController.songNotes[t, track.Sequence_Start_IDs[0]].Length == 0)) continue;

                SongController.songNotes[t, track.Sequence_Start_IDs[0]][0].IsTargetNote = true;
            }
            else
            {
                // TODO: This doesn't work if the sequence length is large & horizon is short.
                // We'll have to do some magic above as well!
                SetTargetNote(t, track.Sequences[0].Notes[0]);
            }
        }
    }
    /// <summary>Finds the next targetNote for a given track.</summary>
    public void IncrementTargetNote(AmpTrack track)
    {
        int target_id = 0;
        for (int i = 0; i < track.Sequences.Count; i++)
        {
            AmpTrackSection m = track.Sequences[i];
            AmpNote current_note = targetNotes[track.ID];
            target_id = current_note.ID + 1;

            if (target_id >= m.Notes.Count)
            {
                target_id -= m.Notes.Count;
                if (i + 1 < track.Sequences.Count) m = track.Sequences[++i];
                else return;
                track.RemoveSequence(track.Sequences[i - 1]); // Remove previous sequence so that it no longer counts
            }

            AmpNote target_note = m.Notes[target_id];
            if (!target_note) Logger.LogMethodE($"target_note was null! - target_id: {target_id}", this);
            else
            {
                SetTargetNote(track.ID, target_note);
                return;
            }
        }

        if (targetNotes[track.ID] == null) // TODO: this may be unnecessary
        {
            for (int i = 0; i < track.Sequences.Count; i++)
            {
                AmpTrackSection m = track.Sequences[i];
                for (int x = 0; x < m.Notes.Count; x++)
                    if (!m.Notes[x].IsCaptured) SetTargetNote(i, m.Notes[x]);
            }
        }
    }

    /// <summary>
    /// Finds the next sequences in all tracks. <br/>
    /// Populates the Sequences list in AmpTracks with measures.
    /// </summary>
    public void RefreshSequences(AmpTrack c_track = null)
    {
        int measure_offset = (c_track) ? RhythmicGame.SequenceAmount : 0;

        for (int t = 0; t < MainTracks_Count; t++)
        {
            AmpTrack track = MainTracks[t];
            if (c_track && t == c_track.ID) continue; // Ignore current track

            track.ClearSequences();

            for (int i = Clock.Fbar + measure_offset, total = 0; i < SongController.songLengthInMeasures; i++)
            {
                if (track.Sequences.Count == RhythmicGame.SequenceAmount)
                    break;

                if (i >= track.Measures.Count) // META
                {
                    MetaMeasure meta_measure = TrackStreamer.metaMeasures[t, i];
                    MetaNote[] meta_notes = SongController.songNotes[t, i];
                    if (meta_notes == null || meta_notes.Length == 0 || meta_measure.IsCaptured)
                        continue;

                    for (int x = 0; x < RhythmicGame.SequenceAmount; x++)
                        track.SetSequenceStartIDs(x, i + x);
                    //Logger.LogMethod($"META: t: {t}, start_id: {i} - {i + RhythmicGame.SequenceAmount - 1}", this);
                    break;
                }
                AmpTrackSection measure = track.Measures[i];
                if (measure.IsEmpty || measure.IsCaptured || !measure.IsEnabled) // Not eligible!
                    if (total >= 1) break;
                    else continue;

                track.AddSequence(measure);
                ++total;
            }
        }

        if (CurrentTrack && CurrentTrack.Sequences.Count > 0 && (CurrentTrack.RealID != seeker_lastTrackID && CurrentTrack.Sequences[0].ID != seeker_lastSequenceID))
            DeformSeeker(CurrentTrack.Sequences[0], CurrentTrack.Sequences.Count);
    }

    IEnumerator _Seeker_Init_WaitForStart()
    {
        while (!CurrentTrack || CurrentTrack.Sequences.Count == 0) yield return null;
        DeformSeeker(CurrentTrack.Sequences[0], CurrentTrack.Sequences.Count);
    }

    public void RefreshAll(AmpTrack c_track = null)
    {
        RefreshSequences(c_track);
        RefreshTargetNotes(c_track);
    }

    // TODO: global WaitForLoading or something
    [NonSerialized] public bool IsLoaded;
    IEnumerator RefreshSequences_Init()
    {
        //while (Tracks[Tracks.Length - 1].Measures.Count < RhythmicGame.HorizonMeasures)
        while (!TrackStreamer || TrackStreamer.IsStreaming) // Wait for streaming to finish
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
        CurrentTrackSetID = track.TrackSetID;

        // Handle focus states
        foreach (AmpTrack t in MainTracks)
            t.SetIsTrackFocused(track.RealID); // Focused state is whether t's ID is the same as the requested track's ID

        if (RhythmicGame.DebugPlayerTrackSwitchEvents)
            Debug.LogFormat("TRACKS: Track switched to {0} [{1}]", track.RealID, track.name);

        if (CurrentTrack && CurrentTrack.Sequences.Count > 0)
            DeformSeeker(CurrentTrack.Sequences[0], CurrentTrack.Sequences.Count);

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
            if (i >= SongController.songLengthInMeasures) continue;
            if (i >= track.Measures.Count) // META!
            {
                TrackStreamer.metaMeasures[track.ID, i].IsCaptured = true;
                continue;
            }
            AmpTrackSection m = track.Measures[i];
            if (!m) continue;

            if (m.CaptureState != MeasureCaptureState.Captured)
                m.CaptureState = MeasureCaptureState.Capturing;

            m.IsSequence = false;
        }

        // Refresh sequences & target notes
        RefreshSequences();
        RefreshTargetNotes();

        // Init capture process - wait for captures to finish before proceeding to next one
        for (int i = start; i < end; i++)
        {
            if (i >= SongController.songLengthInMeasures) continue;
            if (i < track.Measures.Count && track.Measures[i])
                yield return track.CaptureMeasure(track.Measures[i]);
            // META is now set above!
            //else // This measure doesn't yet exist - change meta measure to captured /state /in TrackStreamer!
            //    TrackStreamer.metaMeasures[track.ID, i].IsCaptured = true;
        }

        track.IsTrackCapturing = false;
        track.captureAnimStep = 0.85f; // Reset easing anim step value for specific track

        if (RhythmicGame.DebugTrackCapturingEvents) Debug.Log("CAPTURE: done");
    }
}