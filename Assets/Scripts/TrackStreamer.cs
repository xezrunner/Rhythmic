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
    SongController SongController { get { return SongController.Instance; } }
    TracksController TrackController { get { return TracksController.Instance; } }
    Clock Clock { get { return Clock.Instance; } }

    public List<Dictionary<int, MetaMeasure>> metaMeasures
    {
        get { return SongController.metaMeasures; }
        set { SongController.metaMeasures = value; }
    }

    void Awake() { Clock.OnBar += Clock_OnBar; }
    void Start()
    {
        // Stream in the horizon!
        //StreamMeasureRange(0, RhythmicGame.HorizonMeasures, -1, RhythmicGame.FastStreaming);
        StreamMeasureRange(0, (RhythmicGame.StreamAllMeasuresOnStart) ? SongController.songLengthInMeasures : RhythmicGame.HorizonMeasures, -1, RhythmicGame.FastStreamingLevel.HasFlag(FastStreamingLevel.Measures));
    }

    /// ***** ----- DEBUG TEST ----- *****
    int wowCounter = 0;
    void LateUpdate()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            StreamMeasure((int)SongController.Clock.bar + RhythmicGame.HorizonMeasures + wowCounter);
            wowCounter++;
        }
    }

    int destroyCounter = 0; // Keep track of the last destroyed ID
    private void Clock_OnBar(object sender, int e)
    {
        if (RhythmicGame.StreamAllMeasuresOnStart)
            foreach (AmpTrack t in TracksController.Instance.Tracks) { }
        //t.Measures[Clock.Fbar + RhythmicGame.HorizonMeasures].gameObject.SetActive(true);
        //t.Measures[Clock.Fbar + RhythmicGame.HorizonMeasures].ModelRenderer.enabled = true;
        else
            // Stream measures on every bar tick
            StreamMeasure(RhythmicGame.HorizonMeasures + e, -1, RhythmicGame.FastStreamingLevel.HasFlag(FastStreamingLevel.Measures));

        // Delete measures behind us
        // TODO: revise!
        if (e < 2) return;
        for (int t = 0; t < TrackController.Tracks.Count; t++)
        {
            var track = TrackController.Tracks[t];
            var measure = track.Measures[destroyCounter];
            Destroy(measure.gameObject);
            //track.Measures.RemoveAt(0);
            track.Measures[destroyCounter] = null;
        }
        destroyCounter++;
    }

    public void StreamNotes(int id, int trackID, AmpTrackSection measure) => StartCoroutine(_StreamNotes(id, trackID, measure, RhythmicGame.FastStreamingLevel.HasFlag(FastStreamingLevel.Notes)));
    IEnumerator _StreamNotes(int id, int trackID, AmpTrackSection measure, bool immediate = false)
    {
        //for (int x = 1; x <= RhythmicGame.TunnelTrackDuplicationNum; x++)
        //{
        AmpTrack track = TrackController.Tracks[trackID];

        var measureNotes = SongController.songNotes[track.ID].Where(i => i.Key == id);
        if (measureNotes.Count() == 0)
            measure.IsEmpty = true;
        else
        {
            foreach (KeyValuePair<int, MetaNote> kv in measureNotes)
            {
                kv.Value.IsCaptured = measure.IsCaptured; // foreshadowing

                var note = track.CreateNote(kv.Value, measure);
                //measure.ClipManager.AddMeshRenderer(note.NoteMeshRenderer); // TODO: CLIPPING - this is hacky

                if (!immediate) yield return null;
            }

            measure.Notes.Last().IsLastNote = true; // TODO: optimization?
        }
        //}
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
        if (trackID != -1) // stream measure!
        {
            MetaMeasure meta = metaMeasures[trackID][id];
            AmpTrack track = TrackController.Tracks[trackID];

            // Create section!
            AmpTrackSection measure = track.CreateMeasure(meta);

            // Stream notes!
            // Get all meta notes from the current measure
            StreamNotes(id, track.RealID, measure);

            //if (measure.Position.z > AmpPlayerLocomotion.Instance.HorizonLength) measure.enabled = false;
            //if (measure.ID > Clock.Fbar + RhythmicGame.HorizonMeasures) measure.gameObject.SetActive(false);
            //if (measure.ID > Clock.Fbar + RhythmicGame.HorizonMeasures) measure.ModelRenderer.enabled = false;
        }
        else // Stream in the measure from all of the tracks!
        {
            for (int i = 0; i < TrackController.Tracks.Count; i++)
            {
                StartCoroutine(_StreamMeasure(id, i));

                if (!RhythmicGame.FastStreamingLevel.HasFlag(FastStreamingLevel.Tracks)) yield return null;
                //else yield return new WaitForEndOfFrame(); // delay by a bit to account for performance drop
            }
        }
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
            if (!immediate) yield return null;
        }
    }
}
