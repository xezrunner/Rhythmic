using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Track streaming system
// Purpose: stream in the measures and notes in real-time as we are playing

public class TrackStreamer : MonoBehaviour
{
    SongController SongController { get { return SongController.Instance; } }
    TracksController TracksController { get { return TracksController.Instance; } }
    Clock Clock { get { return Clock.Instance; } }
    TrackMeshCreator TrackMeshCreator { get { return TrackMeshCreator.Instance; } } // TODO: performance?
    public List<IDictionary<int, MetaMeasure>> metaMeasures = new List<IDictionary<int, MetaMeasure>>();

    void Awake()
    {
        Clock.OnBar += Clock_OnBar;
    }

    void Start()
    {
        // Build metalist
        foreach (string track in SongController.songTracks)
        {
            var inst = Track.InstrumentFromString(track);

            // create dictionary and metameasures
            IDictionary<int, MetaMeasure> dict = new Dictionary<int, MetaMeasure>();
            for (int i = 0; i < SongController.songLengthInMeasures + 3; i++)
            {
                MetaMeasure metameasure = new MetaMeasure() { ID = i, Instrument = inst };
                dict.Add(i, metameasure);
            }

            metaMeasures.Add(dict);
        }

        // Stream in the starting horizon
        StartCoroutine(StreamMeasure(0, 0));
        StartCoroutine(StreamMeasure(1, 0));
    }

    private void Clock_OnBar(object sender, int e)
    {
        // Stream measures on every bar tick

    }

    /// <summary>
    /// Streams in a specific measure ID.
    /// </summary>
    /// <param name="id">Measure ID to stream in</param>
    /// <param name="inst">Measure instrument to stream in - use -1 to stream in all instruments!</param>
    /// <returns></returns>
    public IEnumerator StreamMeasure(int id, int trackID = -1)
    {
        if (trackID != -1)
        {
            float startDist = (id * SongController.measureLengthInzPos);
            GameObject obj = TrackMeshCreator.CreateTrackObject(startDistance: startDist, length: SongController.measureLengthInzPos, xPosition: trackID);
        }
        else
        {

        }
        yield return new WaitForSeconds(0.1f);
    }

    /// <summary>
    /// Streams in multiple measure IDs asynchronously
    /// </summary>
    /// <param name="startID"></param>
    /// <param name="endID"></param>
    /// <param name="inst">Measure instrument to stream in - use -1 to stream in all instruments!</param>
    /// <returns></returns>
    public IEnumerator StreamMeasureRange(int startID, int endID, int trackID = -1)
    {
        yield return new WaitForSeconds(0.1f);
    }
}
