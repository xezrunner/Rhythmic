using System.Collections.Generic;
using UnityEngine;
using static Logger;

public enum Instrument { Drums, Bass, Synth, Guitar, Vocals, UNKNOWN = -1 }

public class Track : MonoBehaviour
{
    SongSystem SongSystem;
    Song song;

    TrackSystem TrackSystem;

    public static Track CreateTrack(string track_name, Instrument instr, int id_w, int id_r)
    {
        GameObject go = new GameObject(track_name);
        Track t = go.AddComponent<Track>();
        t.track_name = track_name;
        t.instrument = instr;
        t.id_weak = id_w;
        t.id_real = id_r;

        // TODO: Is this okay?
        go.transform.parent = TrackSystem.Instance.transform;
        return t;
    }

    public PathTransform path_trans;
    public MeshFilter mesh_filter;
    public MeshRenderer mesh_renderer;

    public string track_name = null;

    public int id_weak; // Identical ID in case of cloned tracks.
    public int id_real; // Unique ID, even for cloned tracks.

    public Instrument instrument;

    public Measure[] measures;

    public void Start()
    {
        SongSystem = SongSystem.Instance;
        song = SongSystem.song;
        TrackSystem = TrackSystem.Instance;

        measures = new Measure[song.duration_bars];

        // TODO: materials, setup other stuff ...

        // Tests:
        //TEST_GenerateMeasures();
    }

    void TEST_GenerateMeasures()
    {
        if (measures == null || measures.Length <= 0) return;

        for (int i = 0; i < /*song.duration_bars*/5; ++i)
            measures[i] = Measure.CreateMeasure(this, i);
    }
}
