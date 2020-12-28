using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

// Track streaming system
// Purpose: stream in the measures and notes in real-time as we are playing

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
        StreamMeasureRange(0, RhythmicGame.HorizonMeasures, -1);
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

    public int destroyCounter = 0; // Keep track of the last destroyed ID
    private void Clock_OnBar(object sender, int e)
    {
        // Stream measures on every bar tick
        StreamMeasure(RhythmicGame.HorizonMeasures + e, -1, RhythmicGame.FastStreaming);

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

    public void StreamNotes(int id, int trackID, AmpTrackSection measure) => StartCoroutine(_StreamNotes(id, trackID, measure));
    IEnumerator _StreamNotes(int id, int trackID, AmpTrackSection measure)
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

                track.CreateNote(kv.Value, measure);
                yield return new WaitForSeconds(0.1f);
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
        }
        else // Stream in the measure from all of the tracks!
        {
            for (int i = 0; i < TrackController.Tracks.Count; i++)
            {
                StartCoroutine(_StreamMeasure(id, i));
                yield return new WaitForSeconds(!immediate ? 0.1f : 0f); // delay by a bit to account for performance drop
            }
            yield return null;
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
            //yield return new WaitForSeconds(0.1f);
        }
        yield return null;
    }
}
