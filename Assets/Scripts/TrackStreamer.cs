using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;

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

    PlayerPowerupManager PlayerPowerupManager { get { return PlayerPowerupManager.Instance; } }
    GenericSongController SongController;
    TracksController TracksController;
    Clock Clock;

    SongInfo song_info;
    
    public float StreamDelay = 0.1f;
    public float DestroyDelay = 0f;
    
    public bool IsStreaming = true;
    
    public MetaMeasure[,] metaMeasures;
    void CreateMetaMeasures()
    {
        int m_count = song_info.song_length_bars;
        metaMeasures = new MetaMeasure[TracksController.MainTracks.Length, m_count];
        for (int t = 0; t < TracksController.MainTracks.Length; t++)
        {
            for (int i = 0; i < m_count; i++)
            {
                MetaMeasure m = new MetaMeasure() { ID = i, StartDistance = song_info.time_units.BarToPos(i), IsEmpty = (song_info.data_notes[t, i].Length == 0) };
                metaMeasures[t, i] = m;
            }
        }
    }
    
    void GC_Incremental() => GarbageCollector.CollectIncremental();
    
    void Awake()
    {
        Instance = this;
        SongController = GenericSongController.Instance;
        TracksController = TracksController.Instance;
        Clock = Clock.Instance;
        
        song_info = TracksController.song_info;
        
        Clock.OnBar += Clock_OnBar;
        CreateMetaMeasures();
    }
    void Start()
    {
        // ---- initialize recycle arrays -----
        destroyed_measures = new Measure[TracksController.Tracks_Count][];
        for (int i = 0; i < TracksController.Tracks_Count; ++i) destroyed_measures[i] = new Measure[song_info.song_length_bars];

        measure_recycle_count = new int[TracksController.Tracks_Count]; for (int i = 0; i < TracksController.Tracks_Count; ++i) measure_recycle_count[i] = -1;
        measure_recycle_counter = new int[TracksController.Tracks_Count];

        destroyed_notes = new Note[song_info.total_note_count * RhythmicGame.TunnelTrackDuplicationNum]; // TODO!!! Total note count of song!!
        Logger.Log($"TrackStreamer: Allocated {song_info.total_note_count * RhythmicGame.TunnelTrackDuplicationNum} entries for recyclable notes.");

        // ------ SET GARBAGE COLLECTION TO MANUAL ------ //
        if (!Application.isEditor) GarbageCollector.GCMode = GarbageCollector.Mode.Manual;

        // Generate the powerup map
        PlayerPowerupManager?.STREAMER_GeneratePowerupMap();

        // Stream in the horizon!
        //StreamMeasureRange(0, RhythmicGame.HorizonMeasures, -1, RhythmicGame.FastStreaming);
        StreamMeasureRange(0, RhythmicGame.StreamAllMeasuresOnStart ? song_info.song_length_bars : RhythmicGame.HorizonMeasures, -1, RhythmicGame.FastStreamingLevel.HasFlag(FastStreamingLevel.Measures));
    }

    public void Clock_OnBar(object sender, int e)
    {
        if (SongController.is_song_over) return;

        //if (RhythmicGame.StreamAllMeasuresOnStart) { }
        //else
        // Stream measures on every bar tick
        StreamMeasure(e + RhythmicGame.HorizonMeasures, -1, RhythmicGame.FastStreamingLevel.HasFlag(FastStreamingLevel.Measures));

        // Delete measures behind us
        //if (e < DestroyBehind_Offset) return;
        DestroyBehind(DestroyDelay == 0);
    }

    /// Recycling
    public bool AllowRecycling = true;
    [NonSerialized] public int DestroyBehind_Offset = 1;

    // TODO: Assign a pool of measures</notes> to start off with? (* countin)
    int[] measure_recycle_count, measure_recycle_counter;
    public Measure[][] destroyed_measures;
    public Measure GetDestroyedMeasure(int track_id)
    {
        if (AllowRecycling && destroyed_measures[track_id][measure_recycle_counter[track_id]] != null)
        {
            Measure m = destroyed_measures[track_id][measure_recycle_counter[track_id]];
            destroyed_measures[track_id][measure_recycle_counter[track_id]++] = null;
            return m;
        }
        else return null;
    }

    int note_recycle_count = -1, note_recycle_counter = 0;
    public Note[] destroyed_notes;
    public Note GetDestroyedNote()
    {
        if (AllowRecycling && destroyed_notes[note_recycle_counter] != null)
        {
            Note m = destroyed_notes[note_recycle_counter];
            destroyed_notes[note_recycle_counter++] = null;
            return m;
        }
        else return null;
    }

    public void DestroyBehind(bool immediate = false) => StartCoroutine(_DestroyBehind(immediate));
    IEnumerator _DestroyBehind(bool immediate = false)
    {
        for (int t = 0; t < TracksController.Tracks_Count; ++t)
        {
            Track track = TracksController.Tracks[t];
            for (int i = 0; i < Clock.Fbar - DestroyBehind_Offset; ++i)
            {
                var measure = track.Measures[i];
                if (!measure) continue;

                if (AllowRecycling) destroyed_measures[t][++measure_recycle_count[t]] = measure;
                else Destroy(measure.gameObject);

                // Recycle notes as well:
				// TODO: above, we do it elsewhere for measures. Same here?
                if (AllowRecycling)
                {
                    measure.Notes.ForEach(n => { n.ResetComponent(); destroyed_notes[++note_recycle_count] = n; });
                    measure.ResetComponent();
                }
                else measure.Notes.ForEach(n => Destroy(n.gameObject));

                track.Measures[i] = null;

                if (!immediate) yield return new WaitForSeconds(DestroyDelay);
            }
        }
    }

    public void StreamNotes(int id, int trackID, Measure measure, PowerupType powerup_type = PowerupType.None) => StartCoroutine(_StreamNotes(id, trackID, measure, RhythmicGame.FastStreamingLevel.HasFlag(FastStreamingLevel.Notes), powerup_type));
    IEnumerator _StreamNotes(int id, int trackID, Measure measure, bool immediate = false, PowerupType powerup_type = PowerupType.None)
    {
        Track track = TracksController.Tracks[trackID];
        
        MetaNote[] measureNotes = song_info.data_notes[track.ID, id];
        //if (measureNotes == null || measureNotes.Length == 0)
        //{ measure.IsEmpty = true; yield break; }
        
        if (measure.IsEmpty && measureNotes.Length > 0)
            Logger.LogError("WTF!!!");
        
        for (int i = 0; i < measureNotes.Length; i++)
        {
            MetaNote meta_note = measureNotes[i];
            meta_note.IsCaptured = measure.IsCaptured;

            Note note = track.CreateNote(meta_note, measure, i, false, powerup_type);
			
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
            if (id >= song_info.song_length_bars) { IsStreaming = false; yield break; }
            MetaMeasure meta = metaMeasures[trackID % TracksController.MainTracks.Length, id];
            Track track = TracksController.Tracks[trackID];

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
            Measure measure = track.CreateMeasure(meta);
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
                TracksController.RefreshSeeker();
            }

            // Stream notes!
            // Get all meta notes from the current measure
            StreamNotes(id, track.RealID, measure, meta.Powerup);

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
            if (!Application.isEditor) GC_Incremental();
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
