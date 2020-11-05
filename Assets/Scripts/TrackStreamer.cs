using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

// Track streaming system
// Purpose: stream in the measures and notes in real-time as we are playing

public class TrackStreamer : MonoBehaviour
{
    SongController SongController { get { return SongController.Instance; } }
    AmpTrackController TrackController { get { return AmpTrackController.Instance; } }
    Clock Clock { get { return Clock.Instance; } }

    public List<IDictionary<int, MetaMeasure>> metaMeasures = new List<IDictionary<int, MetaMeasure>>();

    void Awake()
    {
        Clock.OnBar += Clock_OnBar;
    }
    void Start()
    {
        // Build metalist
        foreach (string track in TrackController.songTracks)
        {
            var inst = AmpTrack.InstrumentFromString(track);

            // create dictionary and metameasures
            IDictionary<int, MetaMeasure> dict = new Dictionary<int, MetaMeasure>();
            for (int i = 0; i < SongController.songLengthInMeasures + 1; i++)
            {
                MetaMeasure metameasure = new MetaMeasure() { ID = i, Instrument = inst };
                dict.Add(i, metameasure);
            }

            metaMeasures.Add(dict);
        }

        // Stream in the horizon!
        StreamMeasureRange(0, RhythmicGame.HorizonMeasures);
    }

    /// ***** ----- DEBUG TEST ----- *****
    int wowCounter = 1;
    void LateUpdate()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            StreamMeasure((int)SongController.Clock.bar + RhythmicGame.HorizonMeasures + wowCounter);
            wowCounter++;
        }
    }

    private void Clock_OnBar(object sender, int e)
    {
        Debug.Log(e);

        // Stream measures on every bar tick
        StreamMeasure(RhythmicGame.HorizonMeasures + e);

        // Delete measures behind us
        if (e < 2) return;
        for (int t = 0; t < TrackController.Tracks.Count; t++)
        {
            var track = TrackController.Tracks[t];
            var measure = track.Measures[0];
            Destroy(measure.gameObject);
            track.Measures.RemoveAt(0);
        }
    }

    /// <summary>
    /// Streams in a specific measure ID.
    /// </summary>
    /// <param name="id">Measure ID to stream in</param>
    /// <param name="trackID">Track to stream in from - use -1 to stream in from all the tracks!</param>
    /// <returns></returns>
    public void StreamMeasure(int id, int trackID = -1) => StartCoroutine(_StreamMeasure(id, trackID));
    IEnumerator _StreamMeasure(int id, int trackID = -1)
    {
        if (trackID != -1)
        {
            MetaMeasure meta = metaMeasures[trackID][id];
            AmpTrack track = TrackController.Tracks[trackID];

            // Create section!
            track.CreateMeasure(meta);
        }
        else // Stream in the measure from all of the tracks!
        {
            for (int i = 0; i < TrackController.songTracks.Count; i++)
            {
                StartCoroutine(_StreamMeasure(id, i));
                yield return new WaitForSeconds(0.1f); // delay by a bit to account for performance drop
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
    public void StreamMeasureRange(int startID, int endID, int trackID = -1) => StartCoroutine(_StreamMeasureRange(startID, endID, trackID));
    IEnumerator _StreamMeasureRange(int startID, int endID, int trackID = -1)
    {
        for (int i = startID; i < endID; i++)
        {
            StreamMeasure(i, trackID);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
