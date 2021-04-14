using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

// Track streaming system
// Purpose: stream in the measures and notes in real-time as we are playing

public enum FastStreamingLevel
{
    None = 0,
    Notes = 1 << 0,
    Measures = 1 << 1,
    Tracks = 1 << 2,

    All = Notes | Measures | Tracks,
    MeasuresAndNotes = Measures | Notes,
    TracksAndMeasures = Measures | Tracks,
}

public class TrackStreamer : MonoBehaviour
{
    public static TrackStreamer Instance;

    SongController SongController;
    TracksController TracksController;
    Clock Clock;

    public float StreamDelay = 0.1f;
    public float DestroyDelay = 0f;

    public bool IsStreaming = true;

    public MetaMeasure[,] metaMeasures;
    void CreateMetaMeasures()
    {
        int m_count = SongController.songLengthInMeasures;
        metaMeasures = new MetaMeasure[TracksController.MainTracks.Length, m_count];
        for (int t = 0; t < TracksController.MainTracks.Length; t++)
        {
            for (int i = 0; i < m_count; i++)
            {
                MetaMeasure m = new MetaMeasure() { ID = i, StartDistance = SongController.MeasureToPos(i), IsEmpty = (SongController.songNotes[t, i].Length == 0) };
                metaMeasures[t, i] = m;
            }
        }
    }

    void Awake()
    {
        Instance = this;
        SongController = SongController.Instance;
        TracksController = TracksController.Instance;
        Clock = Clock.Instance;

        Clock.OnBar += Clock_OnBar;
        CreateMetaMeasures();
    }
    void Start()
    {
        // ---- initialize recycle arrays -----
        destroyed_measures = new AmpTrackSection[TracksController.Tracks_Count][];
        for (int i = 0; i < TracksController.Tracks_Count; ++i) destroyed_measures[i] = new AmpTrackSection[SongController.songLengthInMeasures];

        measure_recycle_count = new int[TracksController.Tracks_Count]; for (int i = 0; i < TracksController.Tracks_Count; ++i) measure_recycle_count[i] = -1;
        measure_recycle_counter = new int[TracksController.Tracks_Count];

        destroyed_notes = new AmpNote[SongController.total_note_count * RhythmicGame.TunnelTrackDuplicationNum]; // TODO!!! Total note count of song!!
        Logger.Log($"TrackStreamer: Allocated {SongController.total_note_count * RhythmicGame.TunnelTrackDuplicationNum} entries for recyclable notes.");

        // Stream in the horizon!
        //StreamMeasureRange(0, RhythmicGame.HorizonMeasures, -1, RhythmicGame.FastStreaming);
        StreamMeasureRange(0, RhythmicGame.StreamAllMeasuresOnStart ? SongController.songLengthInMeasures : RhythmicGame.HorizonMeasures, -1, RhythmicGame.FastStreamingLevel.HasFlag(FastStreamingLevel.Measures));
    }

    private void Clock_OnBar(object sender, int e)
    {
        if (SongController.IsSongOver) return;

        if (RhythmicGame.StreamAllMeasuresOnStart) { }
        else
            // Stream measures on every bar tick
            StreamMeasure(e + RhythmicGame.HorizonMeasures, -1, RhythmicGame.FastStreamingLevel.HasFlag(FastStreamingLevel.Measures));

        // Delete measures behind us | TODO: revise!
        if (e < 2) return;
        DestroyBehind(DestroyDelay == 0);
    }

    /// Recycling
    // TODO: Assign a pool of measures</notes> to start off with? (* countin)
    int[] measure_recycle_count, measure_recycle_counter;
    public AmpTrackSection[][] destroyed_measures;
    public AmpTrackSection GetDestroyedMeasure(int track_id)
    {
        if (destroyed_measures[track_id][measure_recycle_counter[track_id]] != null)
        {
            AmpTrackSection m = destroyed_measures[track_id][measure_recycle_counter[track_id]];
            destroyed_measures[track_id][measure_recycle_counter[track_id]++] = null;
            return m;
        }
        else return null;
    }

    int note_recycle_count = -1, note_recycle_counter = 0;
    public AmpNote[] destroyed_notes;
    public AmpNote GetDestroyedNote()
    {
        if (destroyed_notes[note_recycle_counter] != null)
        {
            AmpNote m = destroyed_notes[note_recycle_counter];
            destroyed_notes[note_recycle_counter++] = null;
            return m;
        }
        else return null;
    }

    int destroyCounter = 0; // Keep track of the last destroyed ID
    public void DestroyBehind(bool immediate = false) => StartCoroutine(_DestroyBehind(immediate));
    IEnumerator _DestroyBehind(bool immediate = false)
    {
        if (destroyCounter < 0) yield break;
        for (int t = 0; t < TracksController.Tracks_Count; ++t)
        {
            AmpTrack track = TracksController.Tracks[t];
            for (int i = 0; i < Clock.Fbar - 1; ++i)
            {
                var measure = track.Measures[i];
                if (!measure) continue;

                //Destroy(measure.gameObject);
                destroyed_measures[t][++measure_recycle_count[t]] = measure;

                // Recycle notes as well:
                measure.Notes.ForEach(n => { n.ResetComponent(); destroyed_notes[++note_recycle_count] = n; });
                measure.ResetComponent();

                track.Measures[destroyCounter] = null;

                if (!immediate) yield return new WaitForSeconds(DestroyDelay);
            }
        }
        destroyCounter = Clock.Fbar - 1;
    }

    public void StreamNotes(int id, int trackID, AmpTrackSection measure) => StartCoroutine(_StreamNotes(id, trackID, measure, RhythmicGame.FastStreamingLevel.HasFlag(FastStreamingLevel.Notes)));
    IEnumerator _StreamNotes(int id, int trackID, AmpTrackSection measure, bool immediate = false)
    {
        AmpTrack track = TracksController.Tracks[trackID];

        MetaNote[] measureNotes = SongController.songNotes[track.ID, id];
        //if (measureNotes == null || measureNotes.Length == 0)
        //{ measure.IsEmpty = true; yield break; }

        for (int i = 0; i < measureNotes.Length; i++)
        {
            MetaNote meta_note = measureNotes[i];
            meta_note.IsCaptured = measure.IsCaptured;

            AmpNote note = track.CreateNote(meta_note, measure, i, false);
            // Target note from meta
            if (meta_note.IsTargetNote && track.Sequence_Start_IDs[0] == meta_note.MeasureID)
                TracksController.SetTargetNote(meta_note.TrackID, note);

            if (!immediate) yield return new WaitForSeconds(StreamDelay);
        }
    }

    /// <summary>
    /// Streams in a specific measure ID.
    /// </summary>
    /// <param name="id">Measure ID to stream in</param>
    /// <param name="trackID">Track to stream in from - use -1 to stream in from all the tracks!</param>
    /// <returns></returns>
    public void StreamMeasure(int id, int trackID = -1, bool immediate = false) => StartCoroutine(_StreamMeasure(id, trackID, immediate));
    IEnumerator _StreamMeasure(int id, int trackID = -1, bool immediate = false)
    {
        IsStreaming = true;
        if (trackID != -1) // stream measure!
        {
            if (id >= SongController.songLengthInMeasures) { IsStreaming = false; yield break; }
            MetaMeasure meta = metaMeasures[trackID % TracksController.MainTracks.Length, id];
            AmpTrack track = TracksController.Tracks[trackID];

            // Error checking:
            {
                // TODO: Not sure if this is needed:
                //if (!RhythmicGame.StreamAllMeasuresOnStart && id > Clock.Fbar + RhythmicGame.HorizonMeasures) { Logger.LogMethodE($"Tried streaming measure {id.ToString().AddColor(Colors.Warning)}, which is beyond the horizon! ", this); yield break; }
                if (id < track.Measures.Count && track.Measures[id] != null)
                {
                    Logger.LogMethodE($"Tried streaming measure {id.ToString().AddColor(Colors.Warning)}, which already exists!", this);
                    IsStreaming = false;
                    yield break;
                }
            }

            // Create section!
            AmpTrackSection measure = track.CreateMeasure(meta);
            if (track.Sequence_Start_IDs.Contains(meta.ID) && meta.ID >= track.Measures[track.Measures.Count - 1].ID) // TODO: Performance!
            {
#if false
                Logger.LogMethod($"META: meta.ID: {meta.ID}, seq_start_ids: ", this);
                string s = "";
                for (int i = 0; i < track.Sequence_Start_IDs.Length; i++)
                    s += track.Sequence_Start_IDs[i] + "; ";
                Logger.LogMethod(s, this);
#endif
                track.AddSequence(measure, false);
            }

            // Stream notes!
            // Get all meta notes from the current measure
            StreamNotes(id, track.RealID, measure);

            //if (measure.Position.z > AmpPlayerLocomotion.Instance.HorizonLength) measure.enabled = false;
            //if (measure.ID > Clock.Fbar + RhythmicGame.HorizonMeasures) measure.gameObject.SetActive(false);
            //if (measure.ID > Clock.Fbar + RhythmicGame.HorizonMeasures) measure.ModelRenderer.enabled = false;
        }
        else // Stream in the measure from all of the tracks!
        {
            for (int i = 0; i < TracksController.Tracks_Count; i++)
            {
                StartCoroutine(_StreamMeasure(id, i));
                if (!immediate && !RhythmicGame.FastStreamingLevel.HasFlag(FastStreamingLevel.Tracks)) yield return new WaitForSeconds(StreamDelay);
            }
        }
        IsStreaming = false;
    }

    /// <summary>
    /// Streams in multiple measure IDs (range from start to end) asynchronously
    /// </summary>
    /// <param name="startID"></param>
    /// <param name="endID"></param>
    /// <param name="trackID">Tracks to stream in from - use -1 to stream in all the tracks!</param>
    /// <returns></returns>
    public void StreamMeasureRange(int startID, int endID, int trackID = -1, bool immediate = false) => StartCoroutine(_StreamMeasureRange(startID, endID, trackID, immediate));
    IEnumerator _StreamMeasureRange(int startID, int endID, int trackID = -1, bool immediate = false)
    {
        for (int i = startID; i < endID; i++)
        {
            StreamMeasure(i, trackID, immediate);
            if (!immediate) yield return new WaitForSeconds(StreamDelay);
        }
    }
}
